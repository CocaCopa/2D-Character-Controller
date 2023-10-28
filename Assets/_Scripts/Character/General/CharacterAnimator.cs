using UnityEngine;

public enum HumanoidAnimator {
    None,
    Idle,
    LedgeGrabEnter,
    LedgeGrabLoop,
    LedgeClimb,
    Attack_1,
    Attack_2,
};
public class CharacterAnimator : MonoBehaviour {

    /*[SerializeField] private string controllerScriptName = "PlayerController";
    [SerializeField] private MonoBehaviour test;*/
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
    private const string DOUBLE_JUMP = "DoubleJump";
    private const string LEDGE_CLIMB = "LedgeClimb";
    private const string LEDGE_GRAB_ENTER = "LedgeGrabEnter";
    private const string LEDGE_GRAB_LOOP = "LedgeGrabLoop";
    private const string ATTACK_ = "Attack_";

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

        /*var type = System.Type.GetType(controllerScriptName);
        test = (MonoBehaviour)GetComponentInParent(type);*/
    }

    private void Start() {
        humanoidController.OnCharacterJump += Controller_OnCharacterJump;
        humanoidController.OnCharacterAttackStart += Controller_OnCharacterAttackStart;
    }

    private void Controller_OnCharacterAttackStart(object sender, HumanoidController.OnCharacterAttackStartEventArgs e) {
        animator.SetTrigger(ATTACK_ + e.attackCounter);
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
        bool isLedgeGrabbing = humanoidController.IsLedgeClimbing;

        animator.SetFloat(VERTICAL_VELOCITY, verticalVelocity);
        animator.SetBool(IS_RUNNING, isRunning);
        animator.SetBool(IS_FLOOR_SLIDING, isFloorSliding);
        animator.SetBool(IS_WALL_SLIDING, isWallSliding);
        animator.SetBool(IS_GROUNDED, isGrounded);
        animator.SetBool(IS_DASHING, isDashing);
        animator.SetBool(IS_ATTACKING, isAttacking);
        animator.SetBool(LEDGE_CLIMB, isLedgeGrabbing);

        LedgeGrabLogic();
    }

    private void LedgeGrabLogic() {

        bool isLedgeGrabbing = humanoidController.IsLedgeGrabbing;
        bool ledgeEnterPlaying = CheckAnimClipPlaying(HumanoidAnimator.LedgeGrabEnter);

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
    /// <param name="clipName">Name of the animation to check</param>
    /// <param name="percentage">Percentage to compare</param>
    /// <param name="lessThan">True to check for 'less than', False to check for 'more than'</param>
    /// <returns>True, once the animation clip has played more than the given percentage</returns>
    public bool CheckAnimClipPercentage(HumanoidAnimator clipName,  float percentage, bool lessThan = false) {

        string animationName = ConvertToAnimationClipName(clipName);
        bool ledgeEnterPlaying = animator.GetCurrentAnimatorStateInfo(0).IsName(animationName);
        bool ledgeMoreThanPercentage = lessThan
            ? animator.GetCurrentAnimatorStateInfo(0).normalizedTime <= percentage
            : animator.GetCurrentAnimatorStateInfo(0).normalizedTime >= percentage;
        bool reachedPercentage = ledgeEnterPlaying && ledgeMoreThanPercentage;

        return reachedPercentage;
    }

    /// <summary>
    /// Checks if the animation clip with the given name is playing
    /// </summary>
    /// <param name="clipName"></param>
    /// <returns></returns>
    public bool CheckAnimClipPlaying(HumanoidAnimator clipName) {

        string clipNameString = ConvertToAnimationClipName(clipName);
        bool clipIsPlaying = animator.GetCurrentAnimatorStateInfo(0).IsName(clipNameString);

        return clipIsPlaying;
    }

    private static string ConvertToAnimationClipName(HumanoidAnimator clipName) {

        string name = "";
        switch (clipName) {
            case HumanoidAnimator.Idle:
            name = IDLE;
            break;
            case HumanoidAnimator.LedgeGrabEnter:
            name = LEDGE_GRAB_ENTER_NAME;
            break;
            case HumanoidAnimator.LedgeGrabLoop:
            name = LEDGE_GRAB_LOOP_NAME;
            break;
            case HumanoidAnimator.LedgeClimb:
            name = LEDGE_CLIMB_NAME;
            break;
            case HumanoidAnimator.Attack_1:
            name = MELEE_NAME_1;
            break;
            case HumanoidAnimator.Attack_2:
            name = MELEE_NAME_2;
            break;
        }
        return name;
    }
}
