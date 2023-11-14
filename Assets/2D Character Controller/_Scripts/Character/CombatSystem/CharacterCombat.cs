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
    public class OnProjectileThrownEventArgs {
        public GameObject projectile;
    }
    public event EventHandler<OnInitiateNormalAttackEventArgs> OnInitiateNormalAttack;
    public event EventHandler<OnInitiateChargeAttackEventArgs> OnInitiateChargeAttack;
    public event EventHandler<OnProjectileThrownEventArgs> OnProjectileThrown;
    public event EventHandler OnReleaseChargeAttack;
    public event EventHandler OnCancelChargeAttack;

#if UNITY_EDITOR
    [Tooltip("Set to true, if to visualize the hitbox of your current attack")]
    [SerializeField] private bool visualizeAttackHitbox = true;
    [Tooltip("Choose the shape of the hitbox that is configured in the attack you are visualizing")]
    [SerializeField] HitboxShape shape;
    [SerializeField] private Color gizmosColor = Color.green;
#endif
    [Tooltip("The transform of the attack hitbox")]
    [SerializeField] private Transform attackHitboxTransform;
    [Tooltip("Determines the time window during which your character can initiate a follow-up attack after an initial attack.")]
    [SerializeField] private float attackBufferTime;
    [Tooltip("If a charged attack is being casted, this value will be set to true once the attack is fully charged.")]
    [SerializeField] private bool attackCharged = false;
    [Tooltip("Defines the duration required to fully charge an attack, based on the attack configuration.")]
    [SerializeField] private float chargeTimer;
    [Tooltip("Specifies the maximum duration an attack can be held before the character is compelled to release or cancel it, as per the attack configuration.")]
    [SerializeField] private float holdAttackTimer;

    private const float ATTACK_CLIP_THRESHOLD = 1f;

    private Rigidbody2D playerRb;
    private CharacterEnvironmentalQuery envQuery;
    private CharacterAnimator characterAnimator;
    private AnimationClip currentAttackClip;
    private AttackSO receivedAttackData;    // The attack in queque to play
    private AttackSO currentAttackData;     // Current attack playing
    private List<AttackSO> currentComboData = new();
    private Transform projectileSpawnTransform;

    private float defaultLinearDrag = 0f;
    private float defaultGravityScale = 0f;
    private float attackBufferTimer = 0f;

    private int attackCounter;

    private bool canDamage = true;
    private bool attackBufferButton = false;
    private bool moveWhileCastingAttack = false;
    private bool attackCompleted = true;
    private bool isAttacking = false;
    private bool isCharging = false;
    private bool canReleaseChargedAttack = false;

    public int AttackCounter => attackCounter;
    public bool IsAttacking => isAttacking;
    public bool IsCharging => isCharging;
    public bool CanMoveWhileAttacking => currentAttackData != null && (currentAttackData.CanMoveWhileAttacking || currentAttackData.CanMoveWhileCharging);
    public bool CanChangeDirections => currentAttackData == null || currentAttackData.CanChangeDirections;
#if UNITY_EDITOR
    public bool VisualizeAttackHitbox => visualizeAttackHitbox;
#endif

#if UNITY_EDITOR
    private void OnDrawGizmos() {
        if (visualizeAttackHitbox) {
            Gizmos.color = gizmosColor;
            if (shape == HitboxShape.Circle) {
                Vector3 center = attackHitboxTransform.position;
                float radius = (attackHitboxTransform.lossyScale.x / 2 + attackHitboxTransform.lossyScale.y / 2) / 2;
                if (attackHitboxTransform.gameObject.activeInHierarchy) {
                    Gizmos.DrawSphere(center, radius);
                }
                else {
                    Gizmos.DrawWireSphere(center, radius);
                }
            }
            else if (shape == HitboxShape.Box) {
                Vector3 center = attackHitboxTransform.position;
                Vector3 size = attackHitboxTransform.lossyScale;
                if (attackHitboxTransform.gameObject.activeInHierarchy) {
                    Gizmos.DrawCube(center, size);
                }
                else {
                    Gizmos.DrawWireCube(center, size);
                }
            }
        }
    }
#endif

    private void Awake() {
        InitializeComponents();
        InitializeProperties();
    }

    private void Update() {
        NormalAttackInputBuffer();
        HandleAttackState();
        HandleAttackHitbox();
        ResetAttackData();
    }

    private void InitializeComponents() {
        playerRb = GetComponent<Rigidbody2D>();
        envQuery = GetComponent<CharacterEnvironmentalQuery>();
        characterAnimator = GetComponentInChildren<CharacterAnimator>();
    }

    private void InitializeProperties() {
        defaultLinearDrag = playerRb.drag;
        defaultGravityScale = playerRb.gravityScale;
        attackHitboxTransform.gameObject.SetActive(false);
    }

    private void NormalAttackInputBuffer() {
        Utilities.TickTimer(ref attackBufferTimer, attackBufferTime, autoReset: false);
        if (attackBufferTimer == 0) {
            attackBufferButton = false;
        }
        if (AttackIsReady() && attackCompleted && attackBufferButton) {
            attackBufferTimer = 0;
            NormalAttack(currentComboData.Count > 0);
        }
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
    
    private void HandleAttackHitbox() {
        if (!IsAttacking) {
            canDamage = true;
            return;
        }

        if (canDamage && attackHitboxTransform.gameObject.activeInHierarchy) {
            Collider2D collidersHit = null;
            if (currentAttackData.HitboxShape == HitboxShape.Circle) {
                Vector2 point = attackHitboxTransform.position;
                float radius = (attackHitboxTransform.lossyScale.x / 2 + attackHitboxTransform.lossyScale.y / 2) / 2;
                int layerMask = currentAttackData.WhatIsDamageable;
                collidersHit = Physics2D.OverlapCircle(point, radius, layerMask);
            }
            else if (currentAttackData.HitboxShape == HitboxShape.Box) {
                Vector2 point = attackHitboxTransform.position;
                Vector2 size = attackHitboxTransform.lossyScale;
                int layerMask = currentAttackData.WhatIsDamageable;
                collidersHit = Physics2D.OverlapBox(point, size, 0, layerMask);
            }

            if (collidersHit != null && collidersHit.transform.root.TryGetComponent<IDamageable>(out var damageableObject)) {
                damageableObject.TakeDamage(currentAttackData.DamageAmount);
                canDamage = false; // Ensures the attacker will only deal damage once. Resets when attackComplete = true;
            }
        }
    }

    private void ResetAttackData() {
        if (!IsAttacking && receivedAttackData != null) {
            receivedAttackData = null;
            currentAttackData = null;
            currentAttackClip = null;
        }
    }

    private IEnumerator CheckAttackAnimation() {
        yield return new WaitForEndOfFrame();
        attackCompleted = !characterAnimator.IsClipPlaying(currentAttackClip, ATTACK_CLIP_THRESHOLD);
        if (attackCompleted && IsAttacking) {
            ExitAttackState(currentAttackData);
            canDamage = true;
        }
    }

    public void PerformNormalAttack(AttackSO attackData, bool isPartOfCombo, Transform projectileSpawnTransform = null) {
        receivedAttackData = attackData;
        this.projectileSpawnTransform = projectileSpawnTransform;
        if (receivedAttackData.IsChargeableAttack) {
            Debug.LogError("The attack is configured as chargeable. Call 'PerformChargedAttack()' instead.");
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
        if (currentAttackData.ThrowsProjectile) {
            StartCoroutine(WaitAnimationBeforeReleasing(projectileSpawnTransform));
        }
        attackCounter++;
    }

    private bool CanPerformNormalAttack() {
        attackBufferButton = true;
        if (AttackIsReady() && !IsAttacking) {
            return true;
        }
        if (IsAttacking && AttackIsReady()) {
            attackBufferTimer = attackBufferTime;
        }
        return false;
    }

    private bool AttackIsReady() {
        bool blockAttack = receivedAttackData != null && receivedAttackData.DisableCastOnWall && envQuery.WallInFront(receivedAttackData.WallCastDistance);
        bool onCooldown = receivedAttackData != null && Time.time < receivedAttackData.CurrentCooldownTime;
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
                // Chargeable attack sets its cooldown when the attack is released or canceled
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
            Debug.LogError("The attack is not configured as chargeable. Call 'PerformNormalAttack()' instead.");
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
    private void ChargeAttack(AttackSO attackData, Transform projectileSpawnPoint) {

        attackCharged = Utilities.TickTimer(ref chargeTimer, attackData.ChargeTime, false);

        if (canReleaseChargedAttack && attackCharged) {
            if (Utilities.TickTimer(ref holdAttackTimer, attackData.HoldChargeTime, false)) {
                attackCharged = false;
                if (currentAttackData.ChargeOverTime == ChargeOverTime.ForceRelease) {
                    ReleaseChargedAttack(projectileSpawnPoint);
                }
                else if (currentAttackData.ChargeOverTime == ChargeOverTime.ForceCancel) {
                    CancelChargedAttack(currentAttackData);
                }
            }
        }
    }

    public void ReleaseChargedAttack(Transform projectileSpawnTransform = null) {
        if (currentAttackData == null || !currentAttackData.IsChargeableAttack) {
            return;
        }
        if (AttackIsReady()) {
            canReleaseChargedAttack = false;
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

    public void CancelChargedAttack(AttackSO attackData) {
        if (canReleaseChargedAttack) {
            attackData.CurrentCooldownTime = Time.time + attackData.CooldownIfCanceled;
        }
        canReleaseChargedAttack = false;
        isCharging = false;
        isAttacking = false;
        attackCompleted = true;
        ExitAttackState(attackData, false);
        OnCancelChargeAttack?.Invoke(this, EventArgs.Empty);
    }

    private IEnumerator WaitAnimationBeforeReleasing(Transform spawnPoint) {
        AttackSO attackData = currentAttackData;
        if (attackData.IsChargeableAttack) {
            yield return new WaitForEndOfFrame();
            while (characterAnimator.IsClipPlaying(attackData.ChargeAnimation, 1f)) {
                yield return null;
            }
        }
        yield return new WaitForEndOfFrame();
        while (characterAnimator.IsClipPlaying(attackData.AttackAnimation, attackData.ThrowAtPercentage)) {
            yield return null;
        }
        if (currentAttackData.ThrowsProjectile && (currentAttackData.ProjectilePrefab != null || currentAttackData.ProjectilePrefabs.Length > 0)) {
            StartCoroutine(ThrowProjectile(currentAttackData, spawnPoint));
        }
        else {
            Debug.LogError(currentAttackData.name + ": The attack is configured to launch a projectile, but no prefab(s) have been assigned.");
        }
    }

    private IEnumerator ThrowProjectile(AttackSO attackData, Transform spawnTransform) {
        yield return new WaitForSeconds(attackData.DelayProjectileThrow);
        GameObject spawnedProjectile;
        if (attackData.ChooseRandomFromList) {
            int randomIndex = UnityEngine.Random.Range(0, attackData.ProjectilePrefabs.Length);
            spawnedProjectile = Instantiate(attackData.ProjectilePrefabs[randomIndex], spawnTransform.position, Quaternion.Euler(transform.eulerAngles));
        }
        else {
            spawnedProjectile = Instantiate(attackData.ProjectilePrefab, spawnTransform.position, Quaternion.Euler(transform.eulerAngles));
        }
        OnProjectileThrown?.Invoke(this, new OnProjectileThrownEventArgs {
            projectile = spawnedProjectile
        });
        if (spawnedProjectile.TryGetComponent<CombatSystemProjectile>(out var projectile)) {
            if (attackData.IsChargeableAttack) {
                float speedMultiplier = chargeTimer / attackData.ChargeTime;
                projectile.Velocity *= 1 - speedMultiplier;
                projectile.Velocity *= transform.right.x;
                projectile.enabled = true;
            }
            else {
                projectile.Velocity *= transform.right.x;
                projectile.enabled = true;
            }
        }
        else {
            string projectileName = spawnedProjectile.name;
            if (spawnedProjectile.name.Contains("(Clone)")) {
                projectileName = projectileName.Replace("(Clone)", "");
            }
            Debug.LogError(attackData.name + ": The projectile prefab provided (prefab: " + projectileName + "), " +
                "is missing the 'CombatSystemProjectile' component.");
        }
    }

    /// <summary>
    /// Locks your character into 'Attack State' based on the scriptable object data
    /// </summary>
    /// <param name="attackData">The scriptable object that the data of the attack</param>
    private void EnterAttackState(AttackSO attackData) {
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
    private void ExitAttackState(AttackSO attackData, bool adjustPosition = true) {
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
