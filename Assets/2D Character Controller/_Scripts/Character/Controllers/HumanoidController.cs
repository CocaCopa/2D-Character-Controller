using System;
using UnityEngine;
using System.Collections;
using CocaCopa;

[RequireComponent(typeof(CharacterMovement), typeof(CharacterEnvironmentalQuery), typeof(Rigidbody2D))]
public abstract class HumanoidController : MonoBehaviour {

    #region --- Events ---
    public event EventHandler OnCharacterJump;
    public event EventHandler OnCharacterDash;
    public event EventHandler OnLedgeGrabEnter;
    public event EventHandler OnLedgeClimbEnter;
    public event EventHandler OnLedgeExit;
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
    [Tooltip("True means that wall jumps and ledge jumps will be considered as air jumps, while False indicates that only standard air jumps will be counted.")]
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
    #endregion

    #region --- Constants ---
    private const float ATTACK_CLIP_THRESHOLD = 0.95f;
    private const int OPPOSITE_DIRECTION = 180;
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
    protected CharacterCombat characterCombat;

    private bool floorSlideInputHold = false;
    private bool jumpKeyPressed = false;
    private bool preventFloorSlide = false;
    private bool floorSlideFlag = true;
    private bool wallAboveWhenSliding = false;
    private bool exitLedgeGrab = false;
    private bool canWallSlide = false;
    private bool canFireLedgeGrabEnterEvent = true;
    private bool canFireLedgeClimbEnterEvent = true;
    private bool canFireLedgeExitEvent = false;
    private bool ledgeClimbActive = false;

    private int airJumpCounter = 0;

    private float dashCooldownTimer = 0;
    private float ledgeGrabTimer = 0;
    private float coyoteTimer = 0;

    private Vector3 ledgePosition;
    #endregion

    #region --- Public Properties ---
    private float verticalVelocity = 0f;
    private float horizontalVelocity = 0f;

    private int attackCounter = 0;
    private bool ledgeDetected = false;

    private bool isGrounded = true;
    private bool isRunning = false;
    private bool isLedgeGrabbing = false;
    private bool isLedgeClimbing = false;
    private bool isFloorSliding = false;
    private bool isWallSliding = false;
    private bool isDashing = false;

    public float VerticalVelocity => verticalVelocity;
    public float HorizontalVelocity => horizontalVelocity;

    public bool LedgeDetected => ledgeDetected;
    public int AttackCounter => attackCounter;

    public bool IsGrounded => isGrounded;
    public bool IsRunning => isRunning;
    public bool IsFloorSliding => isFloorSliding;
    public bool IsWallSliding => isWallSliding;
    public bool IsDashing => isDashing;
    public bool IsLedgeGrabbing => isLedgeGrabbing;
    public bool IsLedgeClimbing => isLedgeClimbing;
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
        UpdatePlayerState();
    }
    #endregion

    #region --- Debugging ---
#if UNITY_EDITOR
    private void Debugging() {

        if (Input.GetKeyDown(KeyCode.G)) {
            transform.position += new Vector3(0, 10f, 0);
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
        // Isn't it annoying when the value shown is something like: -2.964e ?
        if (Mathf.Abs(verticalVelocity) < 0.00001f) {
            verticalVelocity = 0;
        }
        if (Mathf.Abs(horizontalVelocity) < 0.00001f) {
            horizontalVelocity = 0;
        }

        ledgeDetected = LedgeDetected();
        isGrounded = Grounded();
        isRunning = Run();
        isWallSliding = WallSlide();
        isFloorSliding = FloorSlide();

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
            return !characterCombat.IsAttacking && IsGrounded && HorizontalVelocity != 0 && !RunsIntoWall(wallSlideCheck: false);
        }
        bool WallSlide() {
            bool canWallSlide = this.canWallSlide && envQuery.WallInFront()
                && !IsGrounded && !IsLedgeClimbing && !IsLedgeGrabbing && !IsFloorSliding && !IsDashing && !characterCombat.IsAttacking;
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
    }
    #endregion

    #region --- General Every Frame Adjustments ---
    private void AdjustProperties() {

        activeCollider = verticalCollider.enabled ? verticalCollider : horizontalCollider;
        envQuery.SetActiveCollider(activeCollider);

        if ((jumpKeyPressed && VerticalVelocity < 0) || IsLedgeGrabbing || IsLedgeClimbing) {
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
    }
    #endregion

    #region --- Horizontal Velocity | Movement ---
    /// <summary>
    /// Change your character's horizontal velocity, based on the given direction
    /// </summary>
    /// <param name="moveDirection">Desired horizontal direction to move towards</param>
    protected void ChangeHorizontalVelocity(Vector2 moveDirection) {
        if (DontAllowMovement()) {
            isRunning = false;
            return;
        }
        if (moveDirection == Vector2.zero) {
            isRunning = false;
        }

        Vector2 horizontalVelocity = IsGrounded
            ? characterMovement.OnGroundHorizontalVelocity(moveDirection, RunsIntoWall(false))
            : characterMovement.OnAirHorizontalVelocity(moveDirection, RunsIntoWall(false));
        characterCombat.CanMoveWhileCastingAttack(ref horizontalVelocity);
        Vector2 currentVerticalVelocity = new (0, characterRb.velocity.y);
        Vector2 characterVelocity = horizontalVelocity + currentVerticalVelocity;

        characterRb.velocity = Vector3.Lerp(characterRb.velocity, characterVelocity, smoothMovement * Time.deltaTime);
    }

    private bool DontAllowMovement() {
        bool restrictMovement = characterCombat.IsAttacking || IsFloorSliding || IsDashing || IsLedgeGrabbing || IsLedgeClimbing || IsWallSliding;
        bool bypassRestriction = characterCombat.CanMoveWhileAttacking;
        return restrictMovement && !bypassRestriction;
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

        bool desiredLedgePercentage = characterAnimator.CheckStatePercentage(HumanoidStateName.LedgeGrabEnter, ledgeJumpThreshold);
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
            bool canJump = !characterCombat.IsAttacking && !IsFloorSliding;
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
        if (!characterLedgeGrab || characterCombat.IsAttacking || IsDashing || IsFloorSliding || IsLedgeClimbing) {
            return;
        }
        // LedgeDetected will be forced to return false if 'exitLedgeGrab' is true.
        // Thus, if the character 'IsLedgeGrabbing' and not 'IsLedgeClimbing' they will be forced to exit 'LedgeGrab' state.
        // If the character 'IsLedgeClimbing' they will exit 'LedgeGrab' state, once ledge climb is completed.
        if (LedgeDetected && canLedgeGrab) {
            isLedgeGrabbing = true;
            characterLedgeGrab.EnterLedgeGrabState(ledgePosition);
            if (Utilities.TickTimer(ref ledgeGrabTimer, maxLedgeGrabTime, false)) {
                exitLedgeGrab = true;
                canFireLedgeExitEvent = true;
            }
        }
        else if (IsLedgeGrabbing && !IsLedgeClimbing) {
            ledgeGrabTimer = maxLedgeGrabTime;
            isLedgeGrabbing = false;
            exitLedgeGrab = true;
            characterLedgeGrab.ExitLedgeGrabState();
            canFireLedgeGrabEnterEvent = true;
            canFireLedgeExitEvent = true;
        }

        if (IsLedgeGrabbing && canFireLedgeGrabEnterEvent) {
            canFireLedgeGrabEnterEvent = false;
            OnLedgeGrabEnter?.Invoke(this, EventArgs.Empty);
        }
        if (!IsLedgeGrabbing && canFireLedgeExitEvent) {
            canFireLedgeExitEvent = false;
            OnLedgeExit?.Invoke(this, EventArgs.Empty);
        }
    }
    
    /// <summary>
    /// If a ledge is detected, your character will climb it automatically
    /// </summary>
    protected void LedgeClimb(bool canLedgeClimb = true) {
        if (!characterLedgeGrab || characterCombat.IsAttacking || IsDashing || IsFloorSliding) {
            return;
        }
        if ((LedgeDetected && canLedgeClimb) || IsLedgeClimbing) {
            ledgeClimbActive = true;
            characterLedgeGrab.LedgeClimb(ledgePosition, out isLedgeClimbing, out Vector3 endPosition);
            if (IsLedgeClimbing == false) {
                StartCoroutine(TeleportToPosition(endPosition));
            }
        }
        else if (!IsLedgeGrabbing && !IsLedgeClimbing && ledgeClimbActive) {
            canFireLedgeClimbEnterEvent = true;
            ledgeClimbActive = false;
            characterLedgeGrab.ExitLedgeGrabState();
            OnLedgeExit?.Invoke(this, EventArgs.Empty);
        }

        if (IsLedgeClimbing && canFireLedgeClimbEnterEvent) {
            canFireLedgeClimbEnterEvent = false;
            OnLedgeClimbEnter?.Invoke(this, EventArgs.Empty);
        }
    }
    
    IEnumerator TeleportToPosition(Vector3 endPosition) {
        yield return new WaitForEndOfFrame();
        characterRb.position = endPosition;
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
        bool canDash = !IsWallSliding && !IsFloorSliding && !characterCombat.IsAttacking;
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
    protected void TryFloorSlide(bool inputHold) {
        if (inputHold) {
            FloorSlideEnter();
        }
        else {
            FloorSlideExit();
        }
        if (IsFloorSliding) {
            FloorSlide();
        }
    }

    private void FloorSlideEnter() {
        if (characterSlide)
            floorSlideInputHold = true;
    }

    private void FloorSlideExit() {
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
        /// Prevents the character from floor sliding if their speed is less than their top speed.
        /// If not for this flag, the character would change to "Floor Slide" state at any speed,
        /// but since she can only "Floor Slide" if their speed is more than the minimum slide speed,
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

    #region --- Utilities ---
    private void ToggleColliders(bool sliding) {
        verticalCollider.enabled = !sliding;
        horizontalCollider.enabled = sliding;
    }

    /// <summary>
    /// Flips your character towards the given direction
    /// </summary>
    /// <param name="directionX"></param>
    protected void FlipCharacter(float directionX) {
        if (IsLedgeClimbing || IsLedgeGrabbing || !characterCombat.CanChangeDirections) {
            return;
        }

        Vector3 normalRotation = Vector3.zero;
        Vector3 flipRotation = Vector3.up * OPPOSITE_DIRECTION;

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
        ledgeGrabTimer = maxLedgeGrabTime;
    }
    #endregion
}
