using System.Collections.Generic;
using UnityEngine;

public class PlayerController : HumanoidController {

    [Header("--- Combo Attack ---")]
    [SerializeField] private List<AttackSO> meleeCombo_1 = new();
    [Header("--- Bow Charge Attack ---")]
    [SerializeField] private AttackSO bowChargeAttack;
    [SerializeField] private Transform arrowSpawnTransform;
    [Header("--- Gun Normal Attack ---")]
    [SerializeField] private AttackSO gunFireAttack;
    [Tooltip("Position at which the projectile of this attack will be spawned.")]
    [SerializeField] private Transform bulletSpawnTransform;
    [Tooltip("Position at which the muzzle flash effect should be spawned.")]
    [SerializeField] private Transform muzzleEffectSpawnTransform;
    [Tooltip("Prefab to spawn as muzzle flash.")]
    [SerializeField] private GameObject muzzleEffectPrefab;
    [Tooltip("Duration before the game object is automatically destroyed.")]
    [SerializeField] private float destroyMuzzleEffectTime;

    [HideInInspector] private PlayerInput input;
    private bool canLedgeClimb = false;

    protected override void Awake() {
        base.Awake();
        input = FindObjectOfType<PlayerInput>();
    }

    protected override void Start() {
        base.Start();
        SubscribeToEvents();
        characterCombat.OnProjectileThrown += Combat_ProjectileThrown;
    }

    protected override void OnDisable() {
        base.OnDisable();
        UnsubscribeFromEvents();
    }

    protected override void Update() {
        base.Update();
        LocomotionController();
        PerformingChargedAttacks();
        if (input.OnGunContinuous() && CanGunAttack()) {
            characterCombat.PerformNormalAttack(gunFireAttack, false, bulletSpawnTransform);
        }
    }

    private void Combat_ProjectileThrown(object sender, CharacterCombat.OnProjectileThrownEventArgs e) {
        if (e.attackData == gunFireAttack) {
            GameObject effect = Instantiate(muzzleEffectPrefab, muzzleEffectSpawnTransform.position, Quaternion.identity);
            effect.transform.right = transform.right;
            Destroy(effect, destroyMuzzleEffectTime);
        }
    }

    private void Input_OnComboNomalAttacksPerformed(object sender, System.EventArgs _) {
        if (CanComboAttack()) {
            if (characterCombat.AttackComboCounter < meleeCombo_1.Count) {
                characterCombat.PerformNormalAttack(meleeCombo_1[characterCombat.AttackComboCounter], isPartOfCombo: true);
            }
        }
    }

    private void PerformingChargedAttacks() {
        if (input.OnBowContinuous() && CanBowAttack()) {
            if (IsGrounded) {
                characterCombat.PerformChargedAttack(bowChargeAttack, arrowSpawnTransform);
            }
            else {
                characterCombat.CancelChargedAttack(bowChargeAttack);
            }
        }
    }

    private void Input_OnBowReleased(object sender, System.EventArgs _) {
        if (IsGrounded)
            characterCombat.TryReleaseChargedAttack(arrowSpawnTransform);
        else {
            characterCombat.CancelChargedAttack(bowChargeAttack);
        }
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

    private void LocomotionController() {
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
