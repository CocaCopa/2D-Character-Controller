using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using CocaCopa;

[RequireComponent(typeof(Rigidbody2D))]
public class CharacterCombat : MonoBehaviour {
    public class OnProjectileThrownEventArgs {
        public AttackSO attackData;
        public GameObject projectile;
    }
    public class CurrentAttackEventArgs {
        public AttackSO attackData;
    }
    public event EventHandler<CurrentAttackEventArgs> OnInitiateNormalAttack;
    public event EventHandler<CurrentAttackEventArgs> OnInitiateChargeAttack;
    public event EventHandler<CurrentAttackEventArgs> OnChargedAttackFullyCharged;
    public event EventHandler<CurrentAttackEventArgs> OnReleaseChargeAttack;
    public event EventHandler<CurrentAttackEventArgs> OnCancelChargeAttack;
    public event EventHandler<CurrentAttackEventArgs> OnAttackDealtDamage;
    public event EventHandler<OnProjectileThrownEventArgs> OnProjectileThrown;

    [Tooltip("The transform of the attack hitbox. Projictiles have their own attack hitbox which can be assigned to their " +
        "attached CombatSystemProjectile script.")]
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

    private Rigidbody2D characterRb;
    private CharacterEnvironmentalQuery envQuery;
    private CharacterAnimator characterAnimator;
    private AnimationClip currentAttackClip;
    private AttackSO receivedAttackData;    // The attack in queque to play
    private AttackSO currentAttackData;     // Current attack playing
    private List<AttackSO> currentComboData = new();
    private Transform projectileSpawnTransform;
    private List<Coroutine> runningCoroutines = new();

    private float defaultLinearDrag = 0f;
    private float defaultGravityScale = 0f;
    private float attackBufferTimer = 0f;

    private int attackComboCounter;

    private bool canDamage = true;
    private bool attackBufferButton = false;
    private bool moveWhileCastingAttack = false;
    private bool attackCompleted = true;
    private bool isAttacking = false;
    private bool isCharging = false;
    private bool canReleaseChargedAttack = false;
    private bool isComboAttack = false;

    /// <summary>
    /// Get the data of the currently casted attack.
    /// </summary>
    public AttackSO CurrentAttackData => currentAttackData;
    public int AttackComboCounter => attackComboCounter;
    public bool IsAttacking => isAttacking;
    public bool AttackCompleted => attackCompleted;
    public bool IsCharging => isCharging;
    public float ChargeTimer => chargeTimer;
    public float HoldAttackTimer => holdAttackTimer;
    public bool CanMoveWhileAttacking => currentAttackData != null && (currentAttackData.CanMoveWhileAttacking || currentAttackData.CanMoveWhileCharging);
    public bool CanChangeDirections => currentAttackData == null || currentAttackData.CanChangeDirections;

    #region --- Debug ---
#if UNITY_EDITOR
    [Tooltip("Set to true, if to visualize the hitbox of your current attack")]
    [SerializeField] private bool visualizeAttackHitbox = true;
    [Tooltip("Choose the shape of the hitbox that is configured in the attack you are visualizing")]
    [SerializeField] private Color gizmosColor = Color.green;
    private HitboxShape shape;
    public bool VisualizeAttackHitbox => visualizeAttackHitbox;
    private void OnDrawGizmos() {
        if (visualizeAttackHitbox) {
            Gizmos.color = gizmosColor;
            if (shape == HitboxShape.Circle) {
                Vector3 center = attackHitboxTransform.position;
                float radius = (attackHitboxTransform.lossyScale.x / 2 + attackHitboxTransform.lossyScale.y / 2) / 2;
                if (attackHitboxTransform.gameObject.activeInHierarchy) {
                    //Gizmos.DrawSphere(center, radius);
                    DrawFilledArc(center, radius, 360f, 2000);
                }
                else {
                    //Gizmos.DrawWireSphere(center, radius);
                    DrawArc(center, radius, 360f, 20);
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
    void DrawArc(Vector3 center, float radius, float angle, int segments) {
        float angleStep = angle / segments;

        float currentAngle = 0f;

        Vector3 prevPoint = center + Quaternion.Euler(0, 0, -angle / 2f) * Vector3.right * radius;

        for (int i = 1; i <= segments; i++) {
            currentAngle = i * angleStep;

            Vector3 nextPoint = center + Quaternion.Euler(0, 0, -angle / 2f + currentAngle) * Vector3.right * radius;

            Gizmos.DrawLine(prevPoint, nextPoint);

            prevPoint = nextPoint;
        }
    }
    void DrawFilledArc(Vector3 center, float radius, float angle, int segments) {
        float angleStep = angle / segments;

        float currentAngle = 0f;

        Vector3 prevPoint = center + Quaternion.Euler(0, 0, -angle / 2f) * Vector3.right * radius;

        for (int i = 1; i <= segments; i++) {
            currentAngle = i * angleStep;

            Vector3 nextPoint = center + Quaternion.Euler(0, 0, -angle / 2f + currentAngle) * Vector3.right * radius;

            Gizmos.DrawLine(center, prevPoint);
            Gizmos.DrawLine(center, nextPoint);

            Gizmos.DrawRay(center, (prevPoint - center).normalized * radius);
            Gizmos.DrawRay(center, (nextPoint - center).normalized * radius);

            prevPoint = nextPoint;
        }
    }
#endif
    #endregion

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
        characterRb = GetComponent<Rigidbody2D>();
        envQuery = GetComponent<CharacterEnvironmentalQuery>();
        characterAnimator = GetComponentInChildren<CharacterAnimator>();
    }

    private void InitializeProperties() {
        defaultLinearDrag = characterRb.drag;
        defaultGravityScale = characterRb.gravityScale;
        attackHitboxTransform.gameObject.SetActive(false);
    }

    private void HandleAttackState() {
        if (IsAttacking && !IsCharging) {
            Coroutine newCoroutine = StartCoroutine(CheckAttackAnimation());
            runningCoroutines.Add(newCoroutine);
            if (attackCompleted && !attackBufferButton) {
                attackCompleted = false;
                isAttacking = false;
                attackComboCounter = 0;
                if (currentComboData.Count > 0) {
                    currentComboData[0].CurrentCooldownTime = Time.time + currentComboData[^1].Cooldown;
                    currentComboData.Clear();
                }

            }
        }

        if (attackComboCounter == 0) {
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
            Vector2 point = attackHitboxTransform.position;
            int layerMask = currentAttackData.DamageableLayers;
            if (currentAttackData.DamageableLayers.value == 0) {
                Debug.LogWarning(currentAttackData.name + ": The attack will not damage any objects (LayerMask is set to 'Nothing').");
                return;
            }
            if (currentAttackData.HitboxShape == HitboxShape.Circle) {
                #if UNITY_EDITOR
                shape = HitboxShape.Circle;
                #endif
                float radius = (attackHitboxTransform.lossyScale.x / 2 + attackHitboxTransform.lossyScale.y / 2) / 2;
                collidersHit = Physics2D.OverlapCircle(point, radius, layerMask);
            }
            else if (currentAttackData.HitboxShape == HitboxShape.Box) {
                #if UNITY_EDITOR
                shape = HitboxShape.Box;
                #endif
                Vector2 size = attackHitboxTransform.lossyScale;
                collidersHit = Physics2D.OverlapBox(point, size, 0, layerMask);
            }

            if (collidersHit != null && collidersHit.transform.root.TryGetComponent<IDamageable>(out var damageableObject)) {
                if (currentAttackData.IsChargeableAttack && currentAttackData.ScalableDamage) {
                    float damageAmount = currentAttackData.DamageAmount * (1 - chargeTimer / currentAttackData.ChargeTime);
                    if (damageAmount < currentAttackData.MinimumDamage) {
                        damageAmount = currentAttackData.MinimumDamage;
                    }
                    damageableObject.TakeDamage(damageAmount);
                }
                else if (!currentAttackData.IsChargeableAttack || (currentAttackData.IsChargeableAttack && !currentAttackData.ScalableDamage)) {
                    damageableObject.TakeDamage(currentAttackData.DamageAmount);
                }
                OnAttackDealtDamage?.Invoke(this, new CurrentAttackEventArgs {
                    attackData = currentAttackData
                });
                canDamage = false; // Ensures the attacker will only deal damage once. Resets when attackCompleted = true;
            }
        }
    }

    private void ResetAttackData() {
        if (!IsCharging && !IsAttacking && receivedAttackData != null) {
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
            StopSelectedCoroutines();
            canDamage = true;
        }
    }

    private void StopSelectedCoroutines() {
        foreach (var coroutine in runningCoroutines) {
            StopCoroutine(coroutine);
        }
        runningCoroutines.Clear();
    }

    public void PerformNormalAttack(AttackSO attackData, bool isPartOfCombo, Transform projectileSpawnTransform = null) {
        receivedAttackData = attackData;
        this.projectileSpawnTransform = projectileSpawnTransform;
        if (receivedAttackData.IsChargeableAttack) {
            Debug.LogError("The attack is configured as chargeable. Call 'PerformChargedAttack()' instead.");
            return;
        }
        isComboAttack = isPartOfCombo; // If the execution of NormalAttack() is a result of input buffering, 'isComboAttack' helps identify combo-related calls.
        if (CanPerformNormalAttack()) {
            NormalAttack(isPartOfCombo);
        }
    }

    // Gets called in the Update() method.
    private void NormalAttackInputBuffer() {
        Utilities.TickTimer(ref attackBufferTimer, attackBufferTime, autoReset: false);
        if (attackBufferTimer == 0) {
            attackBufferButton = false;
        }
        if (AttackIsReady() && attackCompleted && attackBufferButton) {
            attackBufferTimer = 0;
            NormalAttack(isComboAttack);
        }
    }

    private void NormalAttack(bool isPartOfCombo) {
        currentAttackData = receivedAttackData;
        UpdateAttackInformation(isPartOfCombo);
        EnterAttackState(currentAttackData);
        OnInitiateNormalAttack?.Invoke(this, new CurrentAttackEventArgs {
            attackData = currentAttackData
        });
        if (currentAttackData.ThrowsProjectile) {
            Coroutine newCoroutine = StartCoroutine(WaitAnimationBeforeReleasing(projectileSpawnTransform));
            runningCoroutines.Add(newCoroutine);
        }
        if (isPartOfCombo) {
            attackComboCounter++;
        }
        else {
            attackComboCounter = 0;
        }
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
            isAttacking = true;
            AnimationClip initiateChargeClip = currentAttackData.InitiateChargeAnimation != null
                ? currentAttackData.InitiateChargeAnimation
                : currentAttackData.ChargeAnimation;
            OnInitiateChargeAttack?.Invoke(this, new CurrentAttackEventArgs {
                attackData = currentAttackData
            });
        }

        if (IsCharging) {
            isAttacking = true;
            ChargeAttack(currentAttackData, projectileSpawPoint);
        }
    }

    private void ChargeAttack(AttackSO attackData, Transform projectileSpawnPoint) {
        Coroutine newCoroutine = StartCoroutine(CheckAttackCharged(attackData));
        runningCoroutines.Add(newCoroutine);
        if (canReleaseChargedAttack && attackCharged) {
            if (holdAttackTimer == attackData.HoldChargeTime) {
                OnChargedAttackFullyCharged?.Invoke(this, new CurrentAttackEventArgs {
                    attackData = currentAttackData
                });
            }
            if (Utilities.TickTimer(ref holdAttackTimer, attackData.HoldChargeTime, autoReset: false)) {
                attackCharged = false;
                if (currentAttackData.ChargeOverTime == ChargeOverTime.ForceRelease) {
                    TryReleaseChargedAttack(projectileSpawnPoint);
                }
                else if (currentAttackData.ChargeOverTime == ChargeOverTime.ForceCancel) {
                    CancelChargedAttack(currentAttackData);
                }
            }
        }
    }

    private IEnumerator CheckAttackCharged(AttackSO attackData) {
        yield return new WaitForEndOfFrame();
        attackCharged = Utilities.TickTimer(ref chargeTimer, attackData.ChargeTime, false);
        if (attackData.InitiateChargeAnimation != null && characterAnimator.IsClipPlaying(attackData.InitiateChargeAnimation, 1)) {
            chargeTimer = attackData.ChargeTime;
        }
    }

    public void TryReleaseChargedAttack(Transform projectileSpawnTransform = null) {
        if (currentAttackData == null || !currentAttackData.IsChargeableAttack) {
            return;
        }
        if (AttackIsReady()) {
            canReleaseChargedAttack = false;
            isAttacking = true;
            currentAttackData.CurrentCooldownTime = Time.time + currentAttackData.Cooldown;
            currentAttackClip = currentAttackData.AttackAnimation;
            OnReleaseChargeAttack?.Invoke(this, new CurrentAttackEventArgs {
                attackData = currentAttackData
            });
            moveWhileCastingAttack = currentAttackData.CanMoveOnReleaseAttack;
            if (currentAttackData.ThrowsProjectile) {
                if (projectileSpawnTransform != null) {
                    Coroutine newCoroutine = StartCoroutine(WaitAnimationBeforeReleasing(projectileSpawnTransform));
                    runningCoroutines.Add(newCoroutine);
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
        OnCancelChargeAttack?.Invoke(this, new CurrentAttackEventArgs {
            attackData = currentAttackData
        });
    }

    private IEnumerator WaitAnimationBeforeReleasing(Transform spawnPoint) {
        AttackSO attackData = currentAttackData;
        if (attackData.IsChargeableAttack) {
            yield return new WaitForEndOfFrame();
            AnimationClip chargeAnimation = attackData.InitiateChargeAnimation != null
                ? attackData.InitiateChargeAnimation
                : attackData.ChargeAnimation;
            while (characterAnimator.IsClipPlaying(chargeAnimation, 1f)) {
                yield return null;
            }
            if (attackData.IsChargeableAttack && attackData.AttackPushesCharacter) {
                if (attackData.AttackPushMode == PushMode.OnRelease) {
                    Coroutine newCoroutine = StartCoroutine(PushCharacter(currentAttackData));
                    runningCoroutines.Add(newCoroutine);
                }
                else if (attackData.AttackPushMode == PushMode.Both) {
                    Coroutine newCoroutine = StartCoroutine(PushCharacter(currentAttackData, useReleaseForces : true));
                    runningCoroutines.Add(newCoroutine);
                }
            }
        }
        yield return new WaitForEndOfFrame();
        while (characterAnimator.IsClipPlaying(attackData.AttackAnimation, attackData.ThrowAtPercentage)) {
            yield return null;
        }

        if (currentAttackData != null && currentAttackData.ThrowsProjectile) {
            bool unassignedProjectile = !currentAttackData.ChooseRandomFromList && currentAttackData.ProjectilePrefab == null;
            bool unassignedProjectiles = currentAttackData.ChooseRandomFromList && currentAttackData.ProjectilePrefabs.Length == 0;

            if (unassignedProjectile || unassignedProjectiles) {
                Debug.LogError(currentAttackData.name + ": The attack is configured to launch a projectile, but no prefab(s) have been assigned.");
            }
            else {
                Coroutine newCoroutine = StartCoroutine(ThrowProjectile(currentAttackData, spawnPoint));
                runningCoroutines.Add(newCoroutine);
            }
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
        SetUpProjectileGameObject(spawnedProjectile, attackData);
        OnProjectileThrown?.Invoke(this, new OnProjectileThrownEventArgs {
            attackData = attackData,
            projectile = spawnedProjectile
        });
    }

    private void SetUpProjectileGameObject(GameObject spawnedProjectile, AttackSO attackData) {
        if (spawnedProjectile.TryGetComponent<CombatSystemProjectile>(out var projectile)) {
            projectile.DamageAmount = attackData.ProjectileDamage;
            projectile.DamageLayers = attackData.DamageableLayers;
            if (attackData.IsChargeableAttack) {
                if (attackData.ScalableProjectileDamage) {
                    float projectileDamage = attackData.ProjectileDamage * (1 - chargeTimer / attackData.ChargeTime);
                    if (projectileDamage < attackData.MinimumProjectileDamage) {
                        projectileDamage = attackData.MinimumProjectileDamage;
                    }
                    projectile.DamageAmount = projectileDamage;
                }
                float speedMultiplier = chargeTimer / attackData.ChargeTime;
                projectile.InitialVelocity *= 1 - speedMultiplier;
                projectile.InitialVelocity *= transform.right.x;
                projectile.enabled = true;
            }
            else {
                projectile.InitialVelocity *= transform.right.x;
                projectile.enabled = true;
            }
        }
        else {
            string projectileName = spawnedProjectile.name;
            if (spawnedProjectile.name.Contains("(Clone)")) {
                projectileName = projectileName.Replace("(Clone)", "");
            }
            Debug.LogError(attackData.name + ": The projectile prefab provided (prefab: " + projectileName + "), " +
                "is missing the 'CombatSystemProjectile' component. \nMake sure, said component is attached to the root gameObject.");
        }
    }

    /// <summary>
    /// Locks your character into 'Attack State' based on the scriptable object data
    /// </summary>
    /// <param name="attackData">The scriptable object that the data of the attack</param>
    private void EnterAttackState(AttackSO attackData) {
        if (!attackData.UseGravity) {
            characterRb.gravityScale = 0f;
        }
        if (attackData.AttackPushesCharacter && !attackData.CanMoveWhileAttacking && !attackData.CanMoveWhileCharging) {
            if (attackData.IsChargeableAttack && (attackData.AttackPushMode == PushMode.OnInitiate || attackData.AttackPushMode == PushMode.Both)) {
                Coroutine newCoroutine = StartCoroutine(PushCharacter(attackData));
                runningCoroutines.Add(newCoroutine);
            }
            else if (!attackData.IsChargeableAttack) {
                Coroutine newCoroutine = StartCoroutine(PushCharacter(attackData));
                runningCoroutines.Add(newCoroutine);
            }
        }
        if (attackData.ResetVelocity) {
            characterRb.velocity = Vector2.zero;
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

    private IEnumerator PushCharacter(AttackSO attackData, bool useReleaseForces = false) {
        if (!useReleaseForces) {
            yield return new WaitForSeconds(attackData.DelayForceTime);
            Vector3 direction = transform.right;
            Vector3 force = attackData.Force;
            force.x *= direction.x;
            characterRb.AddForce(force, attackData.ForceMode);
            characterRb.drag = attackData.DragCoeficient;
        }
        else {
            yield return new WaitForSeconds(attackData.ReleaseDelayForceTime);
            Vector3 direction = transform.right;
            Vector3 force = attackData.ReleaseFoce;
            force.x *= direction.x;
            characterRb.AddForce(force, attackData.ReleaseForceMode);
            characterRb.drag = attackData.ReleaseDragCoeficient;
        }
    }

    
    private void ExitAttackState(AttackSO attackData, bool adjustPosition = true) {
        if (adjustPosition && attackData.AdjustPositionOnAttackEnd != Vector3.zero) {
            StartCoroutine(TeleportToPosition(attackData.AdjustPositionOnAttackEnd));
        }
        characterRb.drag = defaultLinearDrag;
        characterRb.gravityScale = defaultGravityScale;
        moveWhileCastingAttack = false;
    }

    private IEnumerator TeleportToPosition(Vector3 position) {
        position.x *= transform.right.x;
        yield return new WaitForEndOfFrame();
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
