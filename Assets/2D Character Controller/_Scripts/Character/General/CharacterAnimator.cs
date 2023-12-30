using UnityEngine;

public class CharacterAnimator : MonoBehaviour {

    private Animator animator;
    private HumanoidController humanoidController;
    private CharacterCombat characterCombat;
    private EntityHealth entityHealth;

    #region --- Animator Constants ---
    // Animation - Variables
    private const string VERTICAL_VELOCITY = "VerticalVelocity";
    private const string IS_RUNNING = "IsRunning";
    private const string IS_FLOOR_SLIDING = "IsFloorSliding";
    private const string IS_WALL_SLIDING = "IsWallSliding";
    private const string IS_GROUNDED = "IsGrounded";
    private const string IS_DASHING = "IsDashing";
    private const string IS_ATTACKING = "IsAttacking";
    private const string IS_CHARGING = "ChargingAttack";
    private const string RELEASE_CHARGE_ATTACK = "ReleaseChargeAttack";
    private const string CANCEL_CHARGE_ATTACK = "CancelChargeAttack";
    private const string DOUBLE_JUMP = "DoubleJump";
    private const string LEDGE_CLIMB = "LedgeClimb";
    private const string LEDGE_GRAB_ENTER = "LedgeGrabEnter";
    private const string LEDGE_GRAB_LOOP = "LedgeGrabLoop";
    private const string TAKE_DAMAGE = "TakeDamage";
    private const string IS_DEAD = "IsDead";

    // Animation - State Names
    private const string IDLE_NAME = "Idle";
    private const string RUN_NAME = "Run";
    private const string DOUBLE_JUMP_NAME = "Double-Jump";
    private const string SLIDE_LOOP_NAME = "Slide-Loop";
    private const string DASH_NAME = "Dash";
    private const string WALL_SLIDE_NAME = "Wall-Slide";
    private const string LEDGE_GRAB_ENTER_NAME = "Ledge-Grab";
    private const string LEDGE_GRAB_LOOP_NAME = "Ledge-Idle";
    private const string LEDGE_CLIMB_NAME = "Ledge-Climb";
    private const string TAKE_DAMAGE_NAME = "Take-Damage";
    private const string DEATH_NAME = "Death";
    #endregion

    private void Awake() {
        animator = GetComponent<Animator>();
        humanoidController = GetComponentInParent<HumanoidController>();
        characterCombat = GetComponentInParent<CharacterCombat>();
        entityHealth = GetComponentInParent<EntityHealth>();
    }

    private void Start() {
        humanoidController.OnCharacterJump += Controller_OnCharacterJump;
        characterCombat.OnInitiateNormalAttack += Controller_OnCharacterNormalAttack;
        characterCombat.OnInitiateChargeAttack += Controller_OnCharacterChargeAttack;
        characterCombat.OnReleaseChargeAttack += Controller_OnCharacterReleaseAttack;
        characterCombat.OnCancelChargeAttack += Controller_OnCharacterCancelAttack;
        entityHealth.OnTakeDamage += Health_OnTakeDamage;
        entityHealth.OnEntityDeath += Health_OnDeath;
        entityHealth.OnEntityAlive += Health_OnAlive;
    }

    private void OnDisable() {
        humanoidController.OnCharacterJump -= Controller_OnCharacterJump;
        characterCombat.OnInitiateNormalAttack -= Controller_OnCharacterNormalAttack;
        characterCombat.OnInitiateChargeAttack -= Controller_OnCharacterChargeAttack;
        characterCombat.OnReleaseChargeAttack -= Controller_OnCharacterReleaseAttack;
        characterCombat.OnCancelChargeAttack -= Controller_OnCharacterCancelAttack;
        entityHealth.OnTakeDamage -= Health_OnTakeDamage;
        entityHealth.OnEntityDeath -= Health_OnDeath;
        entityHealth.OnEntityAlive -= Health_OnAlive;
    }

    private void Health_OnDeath(object sender, System.EventArgs e) {
        animator.ResetTrigger(TAKE_DAMAGE);
        animator.SetBool(IS_DEAD, true);
    }

    private void Health_OnAlive(object sender, System.EventArgs e) {
        animator.ResetTrigger(TAKE_DAMAGE);
        animator.SetBool(IS_DEAD, false);
    }

    private void Controller_OnCharacterNormalAttack(object sender, CharacterCombat.CurrentAttackEventArgs e) {
        animator.Play(e.attackData.AttackAnimation.name, 0, 0);
    }

    private void Controller_OnCharacterChargeAttack(object sender, CharacterCombat.CurrentAttackEventArgs e) {
        animator.ResetTrigger(CANCEL_CHARGE_ATTACK);
        animator.ResetTrigger(RELEASE_CHARGE_ATTACK);
        if (e.attackData.InitiateChargeAnimation) {
            animator.Play(e.attackData.InitiateChargeAnimation.name);
        }
        else {
            animator.Play(e.attackData.ChargeAnimation.name);
        }
    }

    private void Controller_OnCharacterReleaseAttack(object sender, CharacterCombat.CurrentAttackEventArgs _) {
        animator.ResetTrigger(CANCEL_CHARGE_ATTACK);
        animator.SetTrigger(RELEASE_CHARGE_ATTACK);
    }

    private void Controller_OnCharacterCancelAttack(object sender, CharacterCombat.CurrentAttackEventArgs _) {
        animator.SetTrigger(CANCEL_CHARGE_ATTACK);
    }

    private void Controller_OnCharacterJump(object sender, System.EventArgs _) {
        if (humanoidController.IsGrounded == false || characterCombat.IsAttacking) {
            animator.SetTrigger(DOUBLE_JUMP);
        }
    }

    private void Health_OnTakeDamage(object sender, EntityHealth.OnTakeDamageEventArgs _) {
        animator.SetTrigger(TAKE_DAMAGE);
    }

    private void Update() {
        float verticalVelocity = humanoidController.VerticalVelocity;
        bool isRunning = humanoidController.IsRunning;
        bool isFloorSliding = humanoidController.IsFloorSliding;
        bool isWallSliding = humanoidController.IsWallSliding;
        bool isGrounded = humanoidController.IsGrounded;
        bool isDashing = humanoidController.IsDashing;
        bool isLedgeGrabbing = humanoidController.IsLedgeClimbing;

        bool isAttacking = characterCombat.IsAttacking;
        bool isCharging = characterCombat.IsCharging;

        animator.SetFloat(VERTICAL_VELOCITY, verticalVelocity);
        animator.SetBool(IS_RUNNING, isRunning);
        animator.SetBool(IS_FLOOR_SLIDING, isFloorSliding);
        animator.SetBool(IS_WALL_SLIDING, isWallSliding);
        animator.SetBool(IS_GROUNDED, isGrounded);
        animator.SetBool(IS_DASHING, isDashing);
        animator.SetBool(LEDGE_CLIMB, isLedgeGrabbing);

        animator.SetBool(IS_ATTACKING, isAttacking);
        animator.SetBool(IS_CHARGING, isCharging);

        LedgeGrabLogic();
    }

    private void LedgeGrabLogic() {

        bool isLedgeGrabbing = humanoidController.IsLedgeGrabbing;
        bool ledgeEnterPlaying = IsStateActive(HumanoidAnimationStateName.LedgeGrabEnter);

        if (isLedgeGrabbing == false) {

            animator.SetBool(LEDGE_GRAB_LOOP, false);
            animator.SetBool(LEDGE_GRAB_ENTER, false);
        }
        else if (ledgeEnterPlaying && animator.GetBool(LEDGE_GRAB_ENTER)) {

            animator.SetBool(LEDGE_GRAB_ENTER, false);
            animator.SetBool(LEDGE_GRAB_LOOP, true);
        }

        if (isLedgeGrabbing && !animator.GetBool(LEDGE_GRAB_LOOP)) {

            animator.SetBool(LEDGE_GRAB_ENTER, true);
        }
    }

    /// <summary>
    /// Checks if the animation state with the given name has played more or less than the specified percentage.
    /// </summary>
    /// <param name="stateName">Name of the animation state to check.</param>
    /// <param name="percentage">Percentage threshold for comparison.</param>
    /// <param name="lessThan">Set to true to check if the play percentage is less than the given value; false to check if it's more than.</param>
    /// <returns>True, if the animation state has played more or less than the provided percentage, based on the specified comparison; otherwise, false.</returns>
    public bool CheckStatePercentage(HumanoidAnimationStateName stateName, float percentage, bool lessThan = false) {
        string stateNameString = ConvertToAnimationStateName(stateName);
        bool stateIsPlaying = animator.GetCurrentAnimatorStateInfo(0).IsName(stateNameString);
        bool stateMoreThanPercentage = lessThan
            ? animator.GetCurrentAnimatorStateInfo(0).normalizedTime <= percentage
            : animator.GetCurrentAnimatorStateInfo(0).normalizedTime >= percentage;
        bool reachedPercentage = stateIsPlaying && stateMoreThanPercentage;

        return reachedPercentage;
    }

    /// <summary>
    /// Determines if the specified animation clip is currently playing within a given percentage of its duration.
    /// </summary>
    /// <param name="clip">The animation clip to check.</param>
    /// <param name="percentage">The desired percentage of the clip's duration.</param>
    /// <returns>True, if the playhead is within the specified percentage of the animation clip; otherwise, false.</returns>
    public bool IsClipPlaying(AnimationClip clip, float percentage) {
        AnimatorClipInfo[] currentClipInfo = animator.GetCurrentAnimatorClipInfo(0);
        foreach (var clipInfo in currentClipInfo) {
            if (clipInfo.clip == clip) {
                if (animator.GetCurrentAnimatorStateInfo(0).normalizedTime <= percentage)
                return true;
            }
        }
        return false;
    }

    /// <summary>
    /// Checks if the animation state with the given name is currently active.
    /// </summary>
    /// <param name="stateName">Name of the animation state to check.</param>
    /// <returns>True, if the specified animation state is playing; otherwise, false.</returns>
    public bool IsStateActive(HumanoidAnimationStateName stateName) {
        string stateNameString = ConvertToAnimationStateName(stateName);
        bool stateIsPlaying = animator.GetCurrentAnimatorStateInfo(0).IsName(stateNameString);
        return stateIsPlaying;
    }

    private static string ConvertToAnimationStateName(HumanoidAnimationStateName stateName) {

        string name = "";
        switch (stateName) {
            case HumanoidAnimationStateName.Idle:
            name = IDLE_NAME;
            break;
            case HumanoidAnimationStateName.Run:
            name = RUN_NAME;
            break;
            case HumanoidAnimationStateName.DoubleJump:
            name = DOUBLE_JUMP_NAME;
            break;
            case HumanoidAnimationStateName.SlideLoop:
            name = SLIDE_LOOP_NAME;
            break;
            case HumanoidAnimationStateName.Dash:
            name = DASH_NAME;
            break;
            case HumanoidAnimationStateName.WallSlide:
            name = WALL_SLIDE_NAME;
            break;
            case HumanoidAnimationStateName.LedgeGrabEnter:
            name = LEDGE_GRAB_ENTER_NAME;
            break;
            case HumanoidAnimationStateName.LedgeGrabLoop:
            name = LEDGE_GRAB_LOOP_NAME;
            break;
            case HumanoidAnimationStateName.LedgeClimb:
            name = LEDGE_CLIMB_NAME;
            break;
            case HumanoidAnimationStateName.TakeDamage:
            name = TAKE_DAMAGE_NAME;
            break;
            case HumanoidAnimationStateName.Death:
            name = DEATH_NAME;
            break;
        }
        return name;
    }
}
