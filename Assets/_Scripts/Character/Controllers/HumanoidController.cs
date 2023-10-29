using System;
using UnityEngine;
using CocaCopa;
using System.Collections;
using System.Collections.Generic;
using Mono.Cecil.Cil;

[RequireComponent(typeof(CharacterMovement), typeof(CharacterEnvironmentalQuery), typeof(Rigidbody2D))]
public abstract class HumanoidController : MonoBehaviour {

    #region --- Events ---
    public class OnCharacterAttackStartEventArgs {
        public AnimationClip attackClip;
        public int attackCounter;
    }
    public event EventHandler OnCharacterJump;
    public event EventHandler OnCharacterDash;
    public event EventHandler<OnCharacterAttackStartEventArgs> OnCharacterAttackStart;
    #endregion

    #region --- Serializable Variables ---
#if UNITY_EDITOR
    [Header("--- Debug ---")]
    [SerializeField, Range(0.05f, 1)] private float timeScale = 1f;
#endif
    [Header("--- Colliders ---")]
    [Tooltip("Collider to use when sliding")]
    [SerializeField] private CapsuleCollider2D horizontalCollider;
    [Tooltip("Collider to use when in an upright position")]
    [SerializeField] private CapsuleCollider2D verticalCollider;
    [Header("--- Movement ---")]
    [Tooltip("Higher values -> snappier movement | Lower values -> smoother movement")]
    [SerializeField] private float smoothMovement = 24f;
    [Header("--- Jump ---")]
    [Tooltip("Time in seconds where jump can be performed if the character leaves 'Grounded' state")]
    [SerializeField] private float coyoteTime = 0.1f;
    [Tooltip("Number of jumps the character is allowed to perform while on air")]
    [SerializeField] private int numberOfAirJumps = 1;
    [Tooltip("True: numberOfJumps - WallJump - LedgeJump || False: numberOfJumps + WallJump + LedgeJump")]
    [SerializeField] private bool alwaysDecreaseJumpCounter = false;
#if DASH_COMPONENT
    [Header("--- Dash ---")]
    [Tooltip("Dash cooldown in seconds")]
    [SerializeField] private float dashCooldown = 1.2f;
    [Tooltip("Dash will be allowed only if the travel distance exceeds the minimum dash distance")]
    [SerializeField] private float minimumDashDistance = 1f;
#endif
#if SLIDE_COMPONENT
    [Header("--- Slide ---")]
    [Tooltip("Speed of which if reached, the floor slide should be canceled and transition back to running")]
    [SerializeField] private float minimumFloorSlideSpeed = 6f;
#endif
#if LEDGE_GRAB_COMPONENT
    [Header("--- Wall ---")]
    [Tooltip("What percentage of the 'Ledge Enter' animation should be completed before the character can jump while in the 'LedgeGrab' state?")]
    [SerializeField, Range(0.01f, 0.99f)] private float ledgeJumpThreshold = 25f / 100f;
    [Tooltip("For how long should the character be able to hang from a ledge, before falling")]
    [SerializeField] private float maxLedgeGrabTime = 4f;
#endif
#if COMBAT_COMPONENT
    [Header("--- Attack ---")]
    [Tooltip("Determines the time window during which your character can initiate a follow-up attack after an initial attack")]
    [SerializeField] private float meleeAttackBufferTime = 0.4f;
#endif
    #endregion

    #region --- Private Properties ---
    private Rigidbody2D characterRb;
    private CapsuleCollider2D activeCollider;
    private CharacterAnimator characterAnimator;
    private CharacterEnvironmentalQuery envQuery;
    private CharacterMovement characterMovement;
    private CharacterSlide characterSlide;
    private CharacterDash characterDash;
    private CharacterLedgeGrab characterLedgeGrab;
    private CharacterCombat characterCombat;
    private List<AttackSO> comboData;
    private AnimationClip currentAttackClip = null;

    private bool floorSlideInputHold = false;
    private bool jumpKeyPressed = false;
    private bool preventFloorSlide = false;
    private bool floorSlideFlag = true;
    private bool wallAboveWhenSliding = false;
    private bool exitLedgeGrab = false;
    private bool canWallSlide = false;
    private bool attackBufferButton = false;
    private bool attackCompleted = false;
    private bool attackOnCooldown = false;

    private int airJumpCounter = 0;
    private int attackCounter = 0;

    private float attackCooldownTimer = 0;
    private float dashCooldownTimer = 0;
    private float ledgeGrabTimer = 0;
    private float coyoteTimer = 0;
    private float attackBufferTimer;

    private Vector3 ledgePosition;
    #endregion

    #region --- Public Properties ---
    private float verticalVelocity = 0f;
    private float horizontalVelocity = 0f;
    private bool isGrounded = true;
    private bool isRunning = false;
    private bool isLedgeGrabbing = false;
    private bool isLedgeClimbing = false;
    private bool isFloorSliding = false;
    private bool isWallSliding = false;
    private bool isDashing = false;
    private bool isAttacking = false;
    private bool ledgeDetected = false;

    public float VerticalVelocity => verticalVelocity;
    public float HorizontalVelocity => horizontalVelocity;
    public bool IsGrounded => isGrounded;
    public bool IsRunning => isRunning;
    public bool IsFloorSliding => isFloorSliding;
    public bool IsWallSliding => isWallSliding;
    public bool IsDashing => isDashing;
    public bool IsLedgeGrabbing => isLedgeGrabbing;
    public bool IsLedgeClimbing => isLedgeClimbing;
    public bool IsAttacking => isAttacking;
    public bool LedgeDetected => ledgeDetected;
    #endregion

    #region --- Callbacks ---

    protected virtual void Awake() {
        FindComponents();
        InitializeProperties();
    }

    protected virtual void Start() {
        if (characterDash)
            characterDash.OnDashDistanceCovered += CharacterDash_OnDashDistanceCovered;
    }

    protected virtual void OnDisable() {
        if (characterDash)
            characterDash.OnDashDistanceCovered -= CharacterDash_OnDashDistanceCovered;
    }

    protected virtual void Update() {
#if UNITY_EDITOR
        Debugging();
#endif
        ToggleColliders(IsFloorSliding);
        AdjustProperties();
        SpeedCalculations();
        ManageTimers();
        AttackInputBuffer();
        UpdatePlayerState();
        if (IsDashing || IsAttacking) {
            return;
        }
        if (IsFloorSliding) {
            FloorSlide();
        }
    }
    #endregion

    #region --- Debugging ---
#if UNITY_EDITOR
    private void Debugging() {

        if (Input.GetKeyDown(KeyCode.G)) {
            transform.position = new(13, -3);
            characterRb.position = transform.position;
        }
        Time.timeScale = timeScale;
    }
#endif
    #endregion

    #region --- Character State ---
    private void UpdatePlayerState() {

        verticalVelocity = characterRb.velocity.y;
        horizontalVelocity = characterRb.velocity.x;

        ledgeDetected = LedgeDetected();
        isGrounded = Grounded();
        isRunning = Run();
        isWallSliding = WallSlide();
        isFloorSliding = FloorSlide();
        HandleAttackState();

        bool LedgeDetected() {
            bool canLedgeGrab = envQuery.LedgeGrabCheck(ref exitLedgeGrab, out ledgePosition) && !exitLedgeGrab;
            bool ledgeCondition = !isGrounded;
            return characterLedgeGrab
                ? (canLedgeGrab && ledgeCondition)
                : false;
        }
        bool Grounded() {
            bool canBeGrounded = !IsLedgeClimbing;
            bool groundCheck = envQuery.GroundCheck();
            return canBeGrounded && groundCheck;
        }
        bool Run() {
            return IsGrounded && !RunsIntoWall(wallSlideCheck: false);
        }
        bool WallSlide() {
            bool canWallSlide = this.canWallSlide && envQuery.WallInFront() && !IsGrounded && !IsLedgeClimbing && !IsLedgeGrabbing;
            bool wallSlideCondition = IsLedgeGrabbing == false && RunsIntoWall(wallSlideCheck: true);
            return characterSlide
                ? canWallSlide && wallSlideCondition
                : false;
        }
        bool FloorSlide() {
            bool canFloorSlide = IsGrounded;
            bool floorSlideCondition = floorSlideInputHold && CanPerformFloorSlide();
            bool playerFloorSlides = canFloorSlide && floorSlideCondition;
            bool keepSliding = wallAboveWhenSliding;
            return characterSlide
                ? playerFloorSlides || keepSliding
                : false;
        }
        void HandleAttackState() {
            if (!characterCombat) {
                isAttacking = false;
                return;
            }

            if (IsAttacking) {
                if (attackCompleted) {
                    characterCombat.ExitAttackState();
                }
                StartCoroutine(CheckAttackAnimation());
            }

            if (attackCompleted && !attackBufferButton) {
                isAttacking = false;
                attackCounter = 0;
                attackOnCooldown = !Utilities.TickTimer(ref attackCooldownTimer, comboData[attackCounter].Cooldown, false);
            }
        }
    }
    private IEnumerator CheckAttackAnimation() {
        yield return new WaitForEndOfFrame();
        attackCompleted = !characterAnimator.IsClipPlaying(currentAttackClip);
    }
    #endregion
    
    #region --- General Every Frame Adjustments ---
    private void AdjustProperties() {

        activeCollider = verticalCollider.enabled ? verticalCollider : horizontalCollider;
        envQuery.SetActiveCollider(activeCollider);

        if (jumpKeyPressed && VerticalVelocity < 0) {
            jumpKeyPressed = false;
        }
    }

    private void SpeedCalculations() {

        if (characterRb.velocity.magnitude <= 0.0001f) {
            characterRb.velocity = Vector2.zero;
        }
        if (characterRb.velocity == Vector2.zero) {
            characterMovement.CurrentSpeed = 0;
        }
    }

    private void ManageTimers() {

        if (IsGrounded && !jumpKeyPressed) {
            coyoteTimer = coyoteTime;
        }
        else {
            coyoteTimer -= Time.deltaTime;
            coyoteTimer = Mathf.Clamp(coyoteTimer, 0, coyoteTime);
        }

        if (IsLedgeGrabbing) {
            ledgeGrabTimer -= Time.deltaTime;
        }
        else {
            ledgeGrabTimer = maxLedgeGrabTime;
        }
    }
    #endregion

    #region --- Horizontal Velocity | Movement ---
    /// <summary>
    /// Change your character's horizontal velocity, based on the given direction
    /// </summary>
    /// <param name="moveDirection"></param>
    protected void ChangeHorizontalVelocity(Vector2 moveDirection) {
        if (!characterMovement) {
            return;
        }
        if (moveDirection == Vector2.zero) {
            isRunning = false;
        }
        Vector2 horizontalVelocity = IsGrounded
            ? characterMovement.OnGroundHorizontalVelocity(moveDirection, RunsIntoWall(false))
            : characterMovement.OnAirHorizontalVelocity(moveDirection, RunsIntoWall(false));
        Vector2 verticalVelocity = new (0, characterRb.velocity.y);
        Vector2 characterVelocity = horizontalVelocity + verticalVelocity;

        characterRb.velocity = Vector3.Lerp(characterRb.velocity, characterVelocity, smoothMovement * Time.deltaTime);
    }
    #endregion

    #region --- Vertical Velocity | Jump ---
    protected void TryJumping() {
        if (CanJump()) {
            Jump();
        }
    }

    private bool CanJump() {

        if (IsLedgeClimbing || IsDashing) {
            return false;
        }

        bool desiredLedgePercentage = characterAnimator.CheckStatePlayPercentage(HumanoidStateName.LedgeGrabEnter, ledgeJumpThreshold);
        bool ledgeLoopPlaying = characterAnimator.IsStateActive(HumanoidStateName.LedgeGrabLoop);
        bool canLedgeJump = IsLedgeGrabbing && (desiredLedgePercentage || ledgeLoopPlaying);
        bool bypassJumpCheck = IsWallSliding || canLedgeJump;

        if (bypassJumpCheck) {
            if (alwaysDecreaseJumpCounter) {
                airJumpCounter--;
            }
            return true;
        }
        else if (coyoteTimer > 0) {
            // On ground jump
            coyoteTimer = 0;
            airJumpCounter = numberOfAirJumps;
            bool canJump = !IsAttacking && !IsFloorSliding;
            return canJump;
        }
        else if (airJumpCounter > 0 && !IsLedgeGrabbing) {
            // On air jump
            airJumpCounter--;
            return true;
        }
        return false;
    }

    private void Jump() {

        exitLedgeGrab = true;
        jumpKeyPressed = true;

        characterRb.velocity = CalculateJumpVelocity();
        OnCharacterJump?.Invoke(this, EventArgs.Empty);
    }

    private Vector2 CalculateJumpVelocity() {

        Vector2 jumpVelocity = characterMovement.VerticalVelocity(IsWallSliding);

        if (IsWallSliding) {
            return jumpVelocity;
        }
        else {
            Vector2 verticalVelocity = characterRb.velocity;
            verticalVelocity.y = jumpVelocity.y;
            return verticalVelocity;
        }
    }
    #endregion

    #region --- Ledge Grab / Climb ---
#if LEDGE_GRAB_COMPONENT
    /// <summary>
    /// Your character will enter ledge grab state, if a ledge is detected
    /// </summary>
    /// <param name="canLedgeGrab">Leave this parameter as is if you want your character to automatically grab the ledge when detected</param>
    protected void LedgeGrab(bool canLedgeGrab = true) {
        if (!characterLedgeGrab) {
            return;
        }
        if (LedgeDetected && canLedgeGrab) {
            isLedgeGrabbing = true;
            characterLedgeGrab.EnterLedgeGrabState(ledgePosition);
        }
        else {
            isLedgeGrabbing = false;
            if (!IsLedgeClimbing)
                characterLedgeGrab.ExitLedgeGrabState();
        }

        if (ledgeGrabTimer <= 0) {
            exitLedgeGrab = true;
        }
    }

    /// <summary>
    /// If a ledge is detected, your character will climb it automatically
    /// </summary>
    protected void LedgeClimb(bool canLedgeClimb = true) {
        if (!characterLedgeGrab) {
            return;
        }
        if (LedgeDetected && canLedgeClimb) {
            characterLedgeGrab.LedgeClimb(ledgePosition, out isLedgeClimbing, out Vector3 endPosition);
            if (IsLedgeClimbing == false) {
                StartCoroutine(TeleportToPosition(endPosition));
            }
        }
        else if (!IsLedgeGrabbing && !IsLedgeClimbing) {
            characterLedgeGrab.ExitLedgeGrabState();
        }
    }

    IEnumerator TeleportToPosition(Vector3 position) {
        yield return new WaitForEndOfFrame();
        characterRb.position = position;
    }
#endif
    #endregion

    #region --- Dash ---
#if DASH_COMPONENT
    /// <summary>
    /// Your character will perform a Dash, if certain conditions are met
    /// </summary>
    protected void TryDashing() {
        if (!characterDash) {
            return;
        }
        if (CanDash()) {
            Dash();
        }
    }

    private bool CanDash() {
        bool canDash = !IsWallSliding && !IsFloorSliding && !IsAttacking;
        bool allowDash = !envQuery.WallInFront(minimumDashDistance) && Time.time >= dashCooldownTimer;

        return canDash && allowDash;
    }

    private void CharacterDash_OnDashDistanceCovered(object sender, CharacterDash.OnDasDistanceCoveredEventArgs e) {
        characterRb.position = e.targetDashPosition;
        transform.position = characterRb.position;
        characterRb.velocity = ClampVelocityAfterDash();
        characterMovement.CurrentSpeed = characterMovement.TopSpeed;
        characterMovement.MoveDirection = transform.right;
        isDashing = false;

        Vector3 ClampVelocityAfterDash() {

            Vector3 velocity = characterRb.velocity;
            velocity.x = Mathf.Clamp(velocity.x, -characterMovement.TopSpeed, characterMovement.TopSpeed);
            velocity.y = 0;
            return velocity;
        }
    }

    private void Dash() {
        dashCooldownTimer = Time.time + dashCooldown;
        OnCharacterDash?.Invoke(this, EventArgs.Empty);
        characterDash.Dash(activeCollider.bounds.size.y, IsGrounded);
        isDashing = true;
    }
#endif
    #endregion

    #region --- FloorSlide ---
#if SLIDE_COMPONENT
    /// <summary>
    /// You character will begin floor sliding, if possible
    /// </summary>
    protected void FloorSlideEnter() {
        if (characterSlide)
            floorSlideInputHold = true;
    }

    /// <summary>
    /// Your character will stop floor sliding, if possible
    /// </summary>
    protected void FloorSlideExit() {
        if (characterSlide) {
            floorSlideInputHold = false;
            preventFloorSlide = false;
        }
    }

    private void FloorSlide() {
        if (!characterSlide) {
            return;
        }

        bool wallAbove = envQuery.WallAbove();
        bool noNeedToKeepSliding = !wallAbove && wallAboveWhenSliding;
        bool endSlide = !IsGrounded;

        if (wallAbove) {
            wallAboveWhenSliding = true;
        }
        if (noNeedToKeepSliding || endSlide) {
            wallAboveWhenSliding = false;
        }
        characterSlide.FloorSlide(minimumFloorSlideSpeed);
    }

    private bool CanPerformFloorSlide() {

        /// --- Use of preventFloorSlide ---
        /// You can hold the 'slide key' for as long as you want, but if you reach the minimum slide speed, then,
        /// if you want your character to slide again, you need to release the 'slide key' and press it again.
        bool exitSlide = floorSlideInputHold && characterRb.velocity.magnitude < minimumFloorSlideSpeed;
        if (exitSlide) {
            preventFloorSlide = true;
        }

        /// --- Use of floorSlideFlag ---
        /// Prevents the character from floor sliding if her speed is less than her top speed.
        /// If not for this flag, the character would change to "Floor Slide" state at any speed,
        /// but since she can only "Floor Slide" if her speed is more than the minimum slide speed,
        /// she would instantly return to "Run" state.
        float topSpeed = characterMovement.TopSpeed;
        bool topSpeedReached = Mathf.Abs(HorizontalVelocity) > topSpeed - topSpeed * 5 / 100;
        bool canSlide = isFloorSliding == false && topSpeedReached;

        if (canSlide) {
            floorSlideFlag = true;
        }
        else if (isFloorSliding == false) {
            floorSlideFlag = false;
        }

        return !preventFloorSlide && floorSlideFlag;
    }
#endif
    #endregion

    #region --- WallSlide ---
#if SLIDE_COMPONENT
    protected void WallSlide(bool canWallSlide = true) {
        if (!characterSlide) {
            return;
        }
        this.canWallSlide = canWallSlide;
        if (IsWallSliding) {

            characterSlide.EnterWallSlide();
        }
        else {

            characterSlide.ExitWallSlide();
        }
    }
#endif
    #endregion

    #region --- Attack ---
    protected void TrySingleAttack(AttackSO singleAttack) {
        List<AttackSO> singleToList = new() { singleAttack };
        TryComboAttack(singleToList);
    }
    /// <summary>
    /// An attack will be initiated, if certain conditions are met
    /// </summary>
    protected void TryComboAttack(List<AttackSO> comboData) {
        if (!characterCombat) {
            return;
        }
        if (attackOnCooldown) {
            return;
        }
        attackBufferButton = true;
        bool allowAttack = IsGrounded && !IsFloorSliding && !IsLedgeClimbing;
        this.comboData = comboData;
        if (allowAttack && !IsAttacking) {
            MeleeAttack();
        }
        else if (allowAttack) {
            attackBufferTimer = meleeAttackBufferTime;
        }
    }

    private void MeleeAttack() {
        if (attackCounter + 1 == comboData.Count) {
            attackOnCooldown = true;
        }
        attackCooldownTimer = comboData[attackCounter].Cooldown;
        characterCombat.EnterAttackState(comboData[attackCounter]);
        characterMovement.CurrentSpeed = 0;
        isAttacking = true;
        attackCompleted = false;
        currentAttackClip = comboData[attackCounter].AttackAnimation;
        OnCharacterAttackStart?.Invoke(this, new OnCharacterAttackStartEventArgs {
            attackClip = comboData[attackCounter].AttackAnimation,
            attackCounter = attackCounter
        });
        attackCounter++;
    }

    private void AttackInputBuffer() {
        Utilities.TickTimer(ref attackBufferTimer, meleeAttackBufferTime, false);
        if (attackBufferTimer == 0) {
            attackBufferButton = false;
        }
        if (attackCompleted && attackBufferButton) {
            attackBufferTimer = 0;
            isAttacking = true;
            MeleeAttack();
        }
    }
    #endregion

    #region --- Utilities ---
    private void ToggleColliders(bool sliding) {
        verticalCollider.enabled = !sliding;
        horizontalCollider.enabled = sliding;
    }

    /// <summary>
    /// Flips your character to look at the given direction
    /// </summary>
    /// <param name="directionX"></param>
    protected void FlipCharacter(float directionX) {

        Vector3 normalRotation = Vector3.zero;
        Vector3 flipRotation = Vector3.up * 180;

        if (directionX > 0) {

            transform.eulerAngles = normalRotation;
        }
        else if (directionX < 0) {

            transform.eulerAngles = flipRotation;
        }
    }

    private bool RunsIntoWall(bool wallSlideCheck) {
        bool wallDetected = wallSlideCheck == true
            ? envQuery.HeadCollisionCheck() && envQuery.FeetCollisionCheck()
            : envQuery.WallInFront();
        return wallDetected;
    }
    #endregion

    #region --- Initialization ---
    private void FindComponents() {
        characterRb = GetComponent<Rigidbody2D>();
        characterAnimator = GetComponentInChildren<CharacterAnimator>();
        envQuery = GetComponent<CharacterEnvironmentalQuery>();
        characterMovement = GetComponent<CharacterMovement>();
        TryGetComponent(out characterSlide);
        TryGetComponent(out characterDash);
        TryGetComponent(out characterLedgeGrab);
        TryGetComponent(out characterCombat);
    }

    private void InitializeProperties() {
        activeCollider = verticalCollider.enabled ? verticalCollider : horizontalCollider;
        envQuery.SetActiveCollider(activeCollider);
        airJumpCounter = numberOfAirJumps;
    }
    #endregion
}
