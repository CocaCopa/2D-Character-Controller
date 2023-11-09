using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using CocaCopa;

[RequireComponent(typeof(Rigidbody2D))]
public class CharacterCombat : MonoBehaviour {

    public class OnInitiateNormalAttackEventArgs {
        public AnimationClip attackClip;
    }
    public class OnInitiateChargeAttackEventArgs {
        public AnimationClip chargeClip;
    }
    public event EventHandler<OnInitiateNormalAttackEventArgs> OnInitiateNormalAttack;
    public event EventHandler<OnInitiateChargeAttackEventArgs> OnInitiateChargeAttack;
    public event EventHandler OnProjectileThrown;
    public event EventHandler OnReleaseChargeAttack;
    public event EventHandler OnCancelChargeAttack;

    [Tooltip("Determines the time window during which your character can initiate a follow-up attack after an initial attack")]
    [SerializeField] private float attackBufferTime;
    [SerializeField] private bool attackCharged = false;
    [SerializeField] private float chargeTimer;
    [SerializeField] private float holdAttackTimer;

    private const float ATTACK_CLIP_THRESHOLD = 0.95f;

    private Rigidbody2D playerRb;
    private CharacterEnvironmentalQuery envQuery;
    private CharacterAnimator characterAnimator;
    private AnimationClip currentAttackClip;
    private AttackSO receivedAttackData;    // The attack in queque to play
    private AttackSO currentAttackData;     // Current attack playing
    private List<AttackSO> currentComboData = new();

    private float defaultLinearDrag = 0f;
    private float defaultGravityScale = 0f;
    private float attackBufferTimer = 0f;

    private int attackCounter;

    private bool moveWhileCastingAttack = false;
    private bool attackBufferButton = false;
    private bool attackCompleted = true;
    private bool isAttacking = false;
    private bool isCharging = false;
    private bool canReleaseChargedAttack = false;

    public int AttackCounter => attackCounter;
    public bool IsAttacking => isAttacking;
    public bool IsCharging => isCharging;
    public bool CanMoveWhileAttacking => currentAttackData != null && (currentAttackData.CanMoveWhileAttacking || currentAttackData.CanMoveWhileCharging);
    public bool CanChangeDirections => currentAttackData == null || currentAttackData.CanChangeDirections;

    private void Awake() {
        InitializeComponents();
        InitializeProperties();
    }

    private void Update() {
        NormalAttackInputBuffer();
        HandleAttackState();
        ResetAttackData();
    }

    private void InitializeComponents() {
        playerRb = GetComponent<Rigidbody2D>();
        envQuery = GetComponent<CharacterEnvironmentalQuery>();
        characterAnimator = GetComponentInChildren<CharacterAnimator>();
    }

    private void ResetAttackData() {
        if (!IsAttacking && receivedAttackData != null) {
            receivedAttackData = null;
            currentAttackData = null;
            currentAttackClip = null;
        }
    }

    private void InitializeProperties() {
        defaultLinearDrag = playerRb.drag;
        defaultGravityScale = playerRb.gravityScale;
    }

    private void HandleAttackState() {
        if (IsAttacking && !IsCharging) {
            StartCoroutine(CheckAttackAnimation());
            if (attackCompleted && !attackBufferButton) {
                attackCompleted = false;
                isAttacking = false;
                attackCounter = 0;
                if (currentComboData.Count > 0) {
                    currentComboData[0].CurrentCooldownTime = Time.time + currentComboData[^1].Cooldown;
                    currentComboData.Clear();
                }
            }
        }

        if (IsCharging && characterAnimator.IsClipPlaying(currentAttackData.AttackAnimation, ATTACK_CLIP_THRESHOLD)) {
            isCharging = false;
        }
    }

    private IEnumerator CheckAttackAnimation() {
        yield return new WaitForEndOfFrame();
        attackCompleted = !characterAnimator.IsClipPlaying(currentAttackClip, ATTACK_CLIP_THRESHOLD);
        if (attackCompleted && IsAttacking) {
            ExitAttackState(currentAttackData);
        }
    }

    public void PerformNormalAttack(AttackSO attackData, bool isPartOfCombo) {
        receivedAttackData = attackData;
        if (receivedAttackData.IsChargeableAttack) {
            Debug.LogError("The attack is set as chargeable. Call 'TryChargeAttack()' instead");
            return;
        }

        if (CanPerformNormalAttack()) {
            NormalAttack(isPartOfCombo);
        }
    }

    private void NormalAttack(bool isPartOfCombo) {
        currentAttackData = receivedAttackData;
        UpdateAttackInformation(isPartOfCombo);
        EnterAttackState(currentAttackData);
        OnInitiateNormalAttack?.Invoke(this, new OnInitiateNormalAttackEventArgs {
            attackClip = currentAttackData.AttackAnimation
        });
        attackCounter++;
    }

    private void NormalAttackInputBuffer() {
        Utilities.TickTimer(ref attackBufferTimer, attackBufferTime, autoReset: false);
        if (attackBufferTimer == 0) {
            attackBufferButton = false;
        }
        if (attackCompleted && attackBufferButton) {
            attackBufferTimer = 0;
            NormalAttack(currentComboData.Count > 0);
        }
    }

    private bool CanPerformNormalAttack() {
        attackBufferButton = true;
        if (AttackIsReady() && !IsAttacking) {
            return true;
        }
        else if (AttackIsReady() && characterAnimator.IsClipPlaying(currentAttackClip, ATTACK_CLIP_THRESHOLD)) {
            attackBufferTimer = attackBufferTime;
        }
        return false;
    }

    private bool AttackIsReady() {
        bool blockAttack = receivedAttackData.DisableCastOnWall && envQuery.WallInFront(receivedAttackData.WallCastDistance);
        bool onCooldown = Time.time < receivedAttackData.CurrentCooldownTime;
        return !blockAttack && !onCooldown;
    }

    private void UpdateAttackInformation(bool isPartOfCombo) {
        isAttacking = true;
        attackCompleted = false;
        currentAttackClip = currentAttackData.IsChargeableAttack
            ? currentAttackData.ChargeAnimation
            : currentAttackData.AttackAnimation;
        if (isPartOfCombo) {
            currentComboData.Add(currentAttackData);
        }
        else {
            if (!currentAttackData.IsChargeableAttack) {
                currentAttackData.CurrentCooldownTime = Time.time + currentAttackData.Cooldown;
            }
        }
    }

    public void PerformChargedAttack(AttackSO attackData, Transform projectileSpawPoint = null) {
        if (IsAttacking && !IsCharging) {
            return;
        }
        receivedAttackData = attackData;
        if (!receivedAttackData.IsChargeableAttack) {
            Debug.LogError("The attack is not set as chargeable. Call 'TryNormalAttack()' instead");
            return;
        }
        if (attackCompleted && AttackIsReady() && !IsCharging && !IsAttacking) {
            canReleaseChargedAttack = true;
            currentAttackData = attackData;
            UpdateAttackInformation(false);
            EnterAttackState(currentAttackData);
            isCharging = true;
            OnInitiateChargeAttack?.Invoke(this, new OnInitiateChargeAttackEventArgs {
                chargeClip = currentAttackData.ChargeAnimation
            });
        }
        if (IsCharging) {
            isAttacking = true;
            ChargeAttack(currentAttackData, projectileSpawPoint);
        }
    }

    /// <summary>
    /// Makes the character charge an attack
    /// </summary>
    /// <param name="attackData">The scriptable object that the data of the attack</param>
    /// <param name="chargeOvertime">Indicates when the character holded the attack for more than the allowed time</param>
    public void ChargeAttack(AttackSO attackData, Transform projectileSpawnPoint) {

        attackCharged = Utilities.TickTimer(ref chargeTimer, attackData.ChargeTime, false);

        if (canReleaseChargedAttack && attackCharged) {
            if (Utilities.TickTimer(ref holdAttackTimer, attackData.HoldChargeTime, false)) {
                attackCharged = false;
                if (currentAttackData.ChargeOverTime == ChargeOverTime.ForceRelease) {
                    canReleaseChargedAttack = false;
                    ReleaseChargedAttack(projectileSpawnPoint);
                }
                else if (currentAttackData.ChargeOverTime == ChargeOverTime.ForceCancel) {
                    canReleaseChargedAttack = false;
                    CancelChargedAttack();
                }
            }
        }
    }

    public void ReleaseChargedAttack(Transform projectileSpawnTransform = null) {
        if (currentAttackData == null || !currentAttackData.IsChargeableAttack) {
            return;
        }
        if (AttackIsReady()) {
            isAttacking = true;
            currentAttackData.CurrentCooldownTime = Time.time + currentAttackData.Cooldown;
            currentAttackClip = currentAttackData.AttackAnimation;
            OnReleaseChargeAttack?.Invoke(this, EventArgs.Empty);
            moveWhileCastingAttack = currentAttackData.CanMoveOnReleaseAttack;
            if (currentAttackData.ThrowsProjectile) {
                if (projectileSpawnTransform != null) {
                    StartCoroutine(WaitAnimationBeforeReleasing(projectileSpawnTransform));
                }
                else {
                    Debug.LogError(currentAttackData.name + ": The attack is configured to launch a projectile, but no `Transform` has been specified. " +
                        "If you intend for the attack to throw\n a projectile on release, ensure that you provide the necessary `Transform` to both the " +
                        "`PerformChargedAttack()` and `ReleaseChargedAttack()` functions.");
                }
            }
        }
    }

    public void CancelChargedAttack() {
        ExitAttackState(currentAttackData, false);
        currentAttackData.CurrentCooldownTime = Time.time + currentAttackData.CooldownIfOvertime;
        isCharging = false;
        isAttacking = false;
        attackCompleted = true;
        OnCancelChargeAttack?.Invoke(this, EventArgs.Empty);
    }

    private IEnumerator WaitAnimationBeforeReleasing(Transform spawnPoint) {
        AttackSO attackData = currentAttackData;
        yield return new WaitForEndOfFrame();
        while (characterAnimator.IsClipPlaying(attackData.ChargeAnimation, 1f)) {
            yield return null;
        }
        yield return new WaitForEndOfFrame();
        while (characterAnimator.IsClipPlaying(attackData.AttackAnimation, attackData.ThrowAtPercentage)) {
            yield return null;
        }
        StartCoroutine(ThrowProjectile(currentAttackData, spawnPoint));
    }

    private IEnumerator ThrowProjectile(AttackSO attackData, Transform spawnTransform) {
        yield return new WaitForSeconds(attackData.DelayProjectileThrow);
        GameObject projectile = Instantiate(attackData.ProjectilePrefab, spawnTransform.position, Quaternion.identity);
        OnProjectileThrown?.Invoke(this, EventArgs.Empty);
        ArrowProjectile arrow = projectile.GetComponent<ArrowProjectile>();
        Vector2 velocity = arrow.InitialVelocity;
        velocity.x *= transform.right.x;
        arrow.InitialVelocity = velocity;
        arrow.enabled = true;
    }

    /// <summary>
    /// Locks your character into 'Attack State' based on the scriptable object data
    /// </summary>
    /// <param name="attackData">The scriptable object that the data of the attack</param>
    public void EnterAttackState(AttackSO attackData) {
        if (!attackData.UseGravity) {
            playerRb.gravityScale = 0f;
        }
        if (attackData.AttackPushesCharacter && !attackData.CanMoveWhileAttacking && !attackData.CanMoveWhileCharging) {
            StartCoroutine(PushCharacter(attackData));
        }
        if (attackData.ResetVelocity) {
            playerRb.velocity = Vector2.zero;
        }
        if (attackData.IsChargeableAttack) {
            chargeTimer = attackData.ChargeTime;
            holdAttackTimer = attackData.HoldChargeTime;
            moveWhileCastingAttack = attackData.CanMoveWhileCharging;
        }
        else {
            moveWhileCastingAttack = attackData.CanMoveWhileAttacking;
        }
    }

    private IEnumerator PushCharacter(AttackSO attackData) {
        yield return new WaitForSeconds(attackData.DelayForceTime);
        Vector3 direction = transform.right;
        Vector3 force = attackData.Force;
        force.x *= direction.x;
        playerRb.AddForce(force, attackData.ForceMode);
        playerRb.drag = attackData.DragCoeficient;
    }

    /// <summary>
    /// Unlocks your character after the attack is completed
    /// </summary>
    public void ExitAttackState(AttackSO attackData, bool adjustPosition = true) {
        playerRb.drag = defaultLinearDrag;
        playerRb.gravityScale = defaultGravityScale;
        moveWhileCastingAttack = false;
        if (adjustPosition && attackData.AdjustPositionOnAttackEnd != Vector3.zero) {
            StartCoroutine(TeleportToPosition(attackData.AdjustPositionOnAttackEnd));
        }
    }

    private IEnumerator TeleportToPosition(Vector3 position) {
        yield return new WaitForEndOfFrame();
        position.x *= transform.right.x;
        transform.position += position;
    }

    /// <summary>
    /// You should call this function after calculating the horizontal velocity to allow the
    /// combat system to adjust it based on the values provided in the scriptable object
    /// </summary>
    /// <param name="attackData">The scriptable object that the data of the attack</param>
    /// <param name="horizontalVelocity">Current horizontal velocity</param>
    public void CanMoveWhileCastingAttack(ref Vector2 horizontalVelocity) {
        if (currentAttackData == null) {
            return;
        }
        if (moveWhileCastingAttack) {
            if (currentAttackData.CanMoveWhileCharging) {
                horizontalVelocity *= currentAttackData.ChargeMoveSpeedPercentage;
            }
            else if (currentAttackData.CanMoveWhileAttacking) {
                horizontalVelocity *= currentAttackData.AttackMoveSpeedPercentage;
            }
        }
        if (currentAttackData.IsChargeableAttack) {
            if (!moveWhileCastingAttack) {
                horizontalVelocity = Vector2.zero;
            }
        }
    }
}
