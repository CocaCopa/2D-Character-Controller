using UnityEngine;

public enum HumanoidStateName {
    None,
    Idle,
    LedgeGrabEnter,
    LedgeGrabLoop,
    LedgeClimb,
    Attack_1,
    Attack_2,
};
public class CharacterAnimator : MonoBehaviour {

    private Animator animator;
    private HumanoidController humanoidController;

    #region Animator Constants
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

    // Animation - Names
    private const string IDLE = "Idle";
    private const string LEDGE_GRAB_ENTER_NAME = "Ledge-Grab";
    private const string LEDGE_GRAB_LOOP_NAME = "Ledge-Idle";
    private const string LEDGE_CLIMB_NAME = "Ledge-Climb";
    private const string MELEE_NAME_1 = "Melee-1";
    private const string MELEE_NAME_2 = "Melee-2";
    #endregion

    private void Awake() {
        animator = GetComponent<Animator>();
        humanoidController = GetComponentInParent<HumanoidController>();
    }

    private void Start() {
        humanoidController.OnCharacterJump += Controller_OnCharacterJump;
        humanoidController.OnInitiateNormalAttack += Controller_OnCharacterNormalAttack;
        humanoidController.OnInitiateChargeAttack += Controller_OnCharacterChargeAttack;
        humanoidController.OnReleaseChargeAttack += Controller_OnCharacterReleaseAttack;
        humanoidController.OnCancelChargeAttack += Controller_OnCharacterCancelAttack;
    }

    private void Controller_OnCharacterNormalAttack(object sender, HumanoidController.OnInitiateNormalAttackEventArgs e) {
        animator.Play(e.attackClip.name);
    }

    private void Controller_OnCharacterChargeAttack(object sender, HumanoidController.OnInitiateChargeAttackEventArgs e) {
        animator.ResetTrigger(CANCEL_CHARGE_ATTACK);
        animator.ResetTrigger(RELEASE_CHARGE_ATTACK);
        animator.Play(e.chargeClip.name);
    }

    private void Controller_OnCharacterReleaseAttack(object sender, System.EventArgs e) {
        animator.ResetTrigger(CANCEL_CHARGE_ATTACK);
        animator.SetTrigger(RELEASE_CHARGE_ATTACK);
    }

    private void Controller_OnCharacterCancelAttack(object sender, System.EventArgs e) {
        animator.SetTrigger(CANCEL_CHARGE_ATTACK);
    }

    private void Controller_OnCharacterJump(object sender, System.EventArgs e) {
        if (humanoidController.IsGrounded == false) {
            animator.SetTrigger(DOUBLE_JUMP);
        }
    }

    private void Update() {

        float verticalVelocity = humanoidController.VerticalVelocity;
        bool isRunning = humanoidController.IsRunning;
        bool isFloorSliding = humanoidController.IsFloorSliding;
        bool isWallSliding = humanoidController.IsWallSliding;
        bool isGrounded = humanoidController.IsGrounded;
        bool isDashing = humanoidController.IsDashing;
        bool isAttacking = humanoidController.IsAttacking;
        bool isCharging = humanoidController.IsCharging;
        bool isLedgeGrabbing = humanoidController.IsLedgeClimbing;

        animator.SetFloat(VERTICAL_VELOCITY, verticalVelocity);
        animator.SetBool(IS_RUNNING, isRunning);
        animator.SetBool(IS_FLOOR_SLIDING, isFloorSliding);
        animator.SetBool(IS_WALL_SLIDING, isWallSliding);
        animator.SetBool(IS_GROUNDED, isGrounded);
        animator.SetBool(IS_DASHING, isDashing);
        animator.SetBool(IS_ATTACKING, isAttacking);
        animator.SetBool(IS_CHARGING, isCharging);
        animator.SetBool(LEDGE_CLIMB, isLedgeGrabbing);

        LedgeGrabLogic();
    }

    private void LedgeGrabLogic() {

        bool isLedgeGrabbing = humanoidController.IsLedgeGrabbing;
        bool ledgeEnterPlaying = IsStateActive(HumanoidStateName.LedgeGrabEnter);

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
    /// Checks if the animation clip with the given name has played less or more than the given percentage
    /// </summary>
    /// <param name="stateName">Name of the animation to check</param>
    /// <param name="percentage">Percentage to compare</param>
    /// <param name="lessThan">True to check for 'less than', False to check for 'more than'</param>
    /// <returns>True, once the animation clip has played more than the given percentage</returns>
    public bool CheckStatePercentage(HumanoidStateName stateName, float percentage, bool lessThan = false) {

        string stateNameString = ConvertToAnimationClipName(stateName);
        bool stateIsPlaying = animator.GetCurrentAnimatorStateInfo(0).IsName(stateNameString);
        bool stateMoreThanPercentage = lessThan
            ? animator.GetCurrentAnimatorStateInfo(0).normalizedTime <= percentage
            : animator.GetCurrentAnimatorStateInfo(0).normalizedTime >= percentage;
        bool reachedPercentage = stateIsPlaying && stateMoreThanPercentage;

        return reachedPercentage;
    }

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
    /// Checks if the animation clip with the given name is playing
    /// </summary>
    /// <param name="stateName"></param>
    /// <returns></returns>
    public bool IsStateActive(HumanoidStateName stateName) {

        string stateNameString = ConvertToAnimationClipName(stateName);
        bool stateIsPlaying = animator.GetCurrentAnimatorStateInfo(0).IsName(stateNameString);

        return stateIsPlaying;
    }

    private static string ConvertToAnimationClipName(HumanoidStateName stateName) {

        string name = "";
        switch (stateName) {
            case HumanoidStateName.Idle:
            name = IDLE;
            break;
            case HumanoidStateName.LedgeGrabEnter:
            name = LEDGE_GRAB_ENTER_NAME;
            break;
            case HumanoidStateName.LedgeGrabLoop:
            name = LEDGE_GRAB_LOOP_NAME;
            break;
            case HumanoidStateName.LedgeClimb:
            name = LEDGE_CLIMB_NAME;
            break;
            case HumanoidStateName.Attack_1:
            name = MELEE_NAME_1;
            break;
            case HumanoidStateName.Attack_2:
            name = MELEE_NAME_2;
            break;
        }
        return name;
    }
}