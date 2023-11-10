using System.Collections.Generic;
using UnityEngine;

public class PlayerController : HumanoidController {

    [Header("--- Attack ---")]
    [SerializeField] private List<AttackSO> meleeCombo_1 = new();
    [SerializeField] private AttackSO singleChargeAttack;
    [SerializeField] private AttackSO gunFireAttack;
    [SerializeField] private Transform chargeProjectileTransform;
    [SerializeField] private Transform gunProjectileTransform;

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
        PerformingChargedAttacks();
        if (input.OnGunContinuous() && CanGunAttack()) {
            characterCombat.PerformNormalAttack(gunFireAttack, false, gunProjectileTransform);
        }
    }

    private void Input_OnComboNomalAttacksPerformed(object sender, System.EventArgs _) {
        if (CanComboAttack()) {
            if (characterCombat.AttackCounter < meleeCombo_1.Count) {
                characterCombat.PerformNormalAttack(meleeCombo_1[characterCombat.AttackCounter], true);
            }
        }
    }

    private void PerformingChargedAttacks() {
        if (input.OnBowContinuous() && CanBowAttack()) {
            if (IsGrounded) {
                characterCombat.PerformChargedAttack(singleChargeAttack, chargeProjectileTransform);
            }
            else {
                characterCombat.CancelChargedAttack(singleChargeAttack);
            }
        }
    }

    private void Input_OnBowReleased(object sender, System.EventArgs _) {
        characterCombat.ReleaseChargedAttack(chargeProjectileTransform);
    }

    private void Input_OnDashPerformed(object sender, System.EventArgs _) {
        TryDashing();
    }

    private void Input_OnJumpPerformed(object sender, System.EventArgs _) {
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

    private void SubscribeToEvents() {
        input.OnJumpPerformed += Input_OnJumpPerformed;
        input.OnDashPerformed += Input_OnDashPerformed;
        input.OnCombatNormalAttacks += Input_OnComboNomalAttacksPerformed;
        input.OnBowReleased += Input_OnBowReleased;
    }

    private void UnsubscribeFromEvents() {
        input.OnJumpPerformed -= Input_OnJumpPerformed;
        input.OnDashPerformed -= Input_OnDashPerformed;
        input.OnCombatNormalAttacks -= Input_OnComboNomalAttacksPerformed;
        input.OnBowReleased -= Input_OnBowReleased;
    }

    private bool CanBowAttack() {
        return !IsFloorSliding && !IsLedgeClimbing && !IsLedgeGrabbing && !IsDashing && !IsWallSliding;
    }

    private bool CanComboAttack() {
        return IsGrounded && !IsFloorSliding && !IsLedgeClimbing && !IsLedgeGrabbing && !IsDashing && !IsWallSliding;
    }

    private bool CanGunAttack() {
        return IsGrounded && !IsFloorSliding && !IsLedgeClimbing && !IsLedgeGrabbing && !IsDashing && !IsWallSliding;
    }
}
