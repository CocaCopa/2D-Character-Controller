using System.Collections.Generic;
using UnityEngine;

public class PlayerController : HumanoidController {

    [SerializeField] private List<AttackSO> meleeCombo_1 = new();
    [SerializeField] private AttackSO singleChargeAttack;
    [SerializeField] private Transform chargeProjectileTransform;

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

        if (Input.GetKey(KeyCode.Z)) {
            TryChargeAttack(singleChargeAttack);
        }
        else if (Input.GetKeyUp(KeyCode.Z)) {
            ReleaseChargeAttack(chargeProjectileTransform);
        }
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
        if (AttackCounter < meleeCombo_1.Count) {
            TryNormalAttack(meleeCombo_1[AttackCounter], true);
        }
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
        FlipCharacter(input.GetMovementInput().x);
        ChangeHorizontalVelocity(input.GetMovementInput());
        TryFloorSlide(input.OnSlideKeyContinuous());
        WallSlide(input.GetMovementInput().x * transform.right.x > 0);
        LedgeGrab(input.GetMovementInput().x * transform.right.x > 0);
        LedgeClimb(canLedgeClimb);
        if (!IsLedgeClimbing && canLedgeClimb) {
            canLedgeClimb = false;
        }
    }
}
