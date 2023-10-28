public class PlayerController : HumanoidController {

    private PlayerInput input;
    private bool canLedgeClimb = false;

    protected override void Awake() {
        base.Awake();
        input = FindObjectOfType<PlayerInput>();
    }

    protected override void Start() {
        base.Start();
        SubscribeToEvents();
    }

    protected override void OnDisable() {
        base.OnDisable();
        UnsubscribeFromEvents();
    }

    protected override void Update() {
        base.Update();
        Controller();
    }

    private void SubscribeToEvents() {
        input.OnJumpPerformed += Input_OnJumpPerformed;
        input.OnDashPerformed += Input_OnDashPerformed;
        input.OnAttackPerformed += Input_OnAttackPerformed;
    }

    private void UnsubscribeFromEvents() {
        input.OnJumpPerformed -= Input_OnJumpPerformed;
        input.OnDashPerformed -= Input_OnDashPerformed;
        input.OnAttackPerformed -= Input_OnAttackPerformed;
    }

    private void Input_OnAttackPerformed(object sender, System.EventArgs e) {
        TryMeleeAttack();
    }

    private void Input_OnDashPerformed(object sender, System.EventArgs e) {
        TryDashing();
    }

    private void Input_OnJumpPerformed(object sender, System.EventArgs e) {
        if (IsLedgeGrabbing) {
            canLedgeClimb = true;
        }
        else {
            TryJumping();
        }
    }

    private void Controller() {
        if (IsAttacking || IsDashing) {
            return;
        }

        if (input.OnSlideKeyContinuous()) {
            FloorSlideEnter();
        }
        else {
            FloorSlideExit();
        }

        if (!IsFloorSliding) {
            LedgeGrab(input.GetMovementInput().x * transform.right.x > 0);
            LedgeClimb(canLedgeClimb);
            if (!IsLedgeClimbing && canLedgeClimb) {
                canLedgeClimb = false;
            }

            if (!IsLedgeGrabbing && !IsLedgeClimbing) {
                FlipCharacter(input.GetMovementInput().x);
                WallSlide(input.GetMovementInput().x * transform.right.x > 0);
                if (!IsWallSliding) {
                    ChangeHorizontalVelocity(input.GetMovementInput());
                }
            }
        }
    }
}
