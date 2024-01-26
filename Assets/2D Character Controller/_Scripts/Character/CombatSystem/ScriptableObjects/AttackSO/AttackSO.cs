using UnityEngine;

[CreateAssetMenu(fileName = "NewAttack", menuName = "Create New Attack", order = 0)]
public class AttackSO : ScriptableObject {

    [SerializeField] private string attackName;
    [SerializeField] private Sprite attackIcon;

    [Tooltip("The animation clip of the attack.")]
    [SerializeField] private AnimationClip attackAnimation;
    [Tooltip("Define the shape of this attack's hitbox.")]
    [SerializeField] private HitboxShape hitboxShape;
    [Tooltip("Specify which layers can be damaged.")]
    [SerializeField] private LayerMask damageableLayers;
    [Tooltip("How much damage should be dealt by this attack.")]
    [SerializeField] private float damageAmount;
    [Tooltip("Determines if the attack damage should scale with the level of charge applied.")]
    [SerializeField] private bool scalableDamage;
    [Tooltip("Minimum damage of the attack.")]
    [SerializeField] private float minimumDamage;
    [Tooltip("Attack cooldown in seconds.")]
    [SerializeField] private float cooldown;

    [Tooltip("Wether or not your character should be able to cast the attack, if a wall is detected in front of them.")]
    [SerializeField] private bool disableCastOnWall = false;
    [Tooltip("Specify at what distance away from a wall your character must be, in order to allow them to cast the attack.")]
    [SerializeField] private float wallCastDistance;
    [Tooltip("True, sets your character's velocity to Vector3.zero when the attack is initiated. False, your character will continue moving at a constant speed based on their velocity before the attack.")]
    [SerializeField] private bool resetVelocity = true;
    [Tooltip("Determines wether or not your character can change directions while they're attacking.")]
    [SerializeField] private bool canChangeDirections = false;
    [Tooltip("If your animation offsets your character during the attack, you can adjust their position by setting an offset. After the attack, your character will teleport to that offseted position.")]
    [SerializeField] private Vector3 adjustPositionOnAttackEnd;
    [Tooltip("Determines if the character should be able to move while they cast an attack.")]
    [SerializeField] private bool canMoveWhileAttacking;
    [Tooltip("Adjusts the character's movement speed as a percentage of their maximum speed.")]
    [SerializeField, Range(0f,1f)] private float attackMoveSpeedPercentage;

    [Tooltip("If 'true,' a force will be applied to the character in the direction they are facing when the attack is initiated.")]
    [SerializeField] private bool attackPushesCharacter;
    [Tooltip("Choose when your character should be pushed. OnInitiate: Once the attack is initiated. OnRelease: Once the attack is released.")]
    [SerializeField] private PushMode attackPushMode;
    [Tooltip("Choose which Rigidbody.AddForce() force mode should be applied.")]
    [SerializeField] private ForceMode2D forceMode = ForceMode2D.Impulse;
    [Tooltip("How much force should be applied.")]
    [SerializeField] private Vector2 force;
    [Tooltip("Specifies the delay in seconds before applying force. The specified delay time should be less than the duration of the animation clip; otherwise, it will not be considered and the desired force will not be applied.")]
    [SerializeField] private float delayForceTime;
    [Tooltip("Whether the character should be affected by gravity for the duration of the attack.")]
    [SerializeField] private bool useGravity;
    [Tooltip("Change the drag coeficient of the Rigidbody2D for the duration of the attack.")]
    [SerializeField] private float dragCoefficient;
    [Tooltip("Choose which Rigidbody.AddForce() force mode should be applied.")]
    [SerializeField] private ForceMode2D m_ForceMode = ForceMode2D.Impulse;
    [Tooltip("How much force should be applied.")]
    [SerializeField] private Vector2 m_Force;
    [Tooltip("Specifies the delay in seconds before applying force. The specified delay time should be less than the duration of the animation clip; otherwise, it will not be considered and the desired force will not be applied.")]
    [SerializeField] private float m_DelayForceTime;
    [Tooltip("Whether the character should be affected by gravity for the duration of the attack.")]
    [SerializeField] private bool m_UseGravity;
    [Tooltip("Change the drag coeficient of the Rigidbody2D for the duration of the attack.")]
    [SerializeField] private float m_DragCoeficient;

    [Tooltip("Wether or not this attack can be charged.")]
    [SerializeField] private bool isChargeableAttack = false;
    [Tooltip("Animation to play when the attack is initiated. Can be null.")]
    [SerializeField] private AnimationClip initiateChargeAnimation;
    [Tooltip("Animation to play when the attack is charging.")]
    [SerializeField] private AnimationClip chargeAnimation;
    [Tooltip("How much time should it take for the attack to be fully charged in seconds.")]
    [SerializeField] private float chargeTime;
    [Tooltip("For how long should your character be able to hold their charged attack in seconds, before being forced to release/cancel it.")]
    [SerializeField] private float holdChargeTime;
    [Tooltip("Specify the action to be taken when the attack is held for longer than the designated hold charge time.")]
    [SerializeField] private ChargeOverTime chargeOverTime = ChargeOverTime.ForceRelease;
    [Tooltip("Cooldown to set the attack on, if is held more than 'holdChargeTime' or if the attack is canceled.")]
    [SerializeField] private float cooldownIfCanceled;
    [Tooltip("If 'true', your character will be allowed to move while they charge the attack.")]
    [SerializeField] private bool canMoveWhileCharging = false;
    [Tooltip("Adjusts the character's movement speed as a percentage of their maximum speed.")]
    [SerializeField, Range(0f,1f)] private float chargeMoveSpeedPercentage;
    [Tooltip("True if you want your character to be allowed to move as soon as the attack is released.")]
    [SerializeField] private bool canMoveOnReleaseAttack;

    [Tooltip("Set to 'true' if your attack should throw a projectile.")]
    [SerializeField] private bool throwsProjectile = false;
    [Tooltip("Determines if the damage of the projectile should scale with the level of charge applied.")]
    [SerializeField] private bool scalableProjectileDamage;
    [Tooltip("Determines if the velocity of the projectile should scale with the level of charge applied.")]
    [SerializeField] private bool scalableProjectileVelocity;
    [Tooltip("Enabling this option allows multiple prefabs to be assigned as projectiles. Upon attacking, a random prefab will be chosen.")]
    [SerializeField] private bool chooseRandomFromList = false;
    [Tooltip("Projectile to spawn.\n\nEnsure that the provided prefab has the 'CombatSystemProjectile' component attached for correct functionality.")]
    [SerializeField] private GameObject projectilePrefab;
    [Tooltip("Projectiles to spawn. A random prefab will be selected from the list once the attack spawns the projectile.\n\nEnsure that the provided prefabs have the 'CombatSystemProjectile' component attached for correct functionality.")]
    [SerializeField] private GameObject[] projectilePrefabs;
    [Tooltip("Indicates the exact timing for launching the projectile during the 'AttackAnimation', based on a specified percentage.")]
    [SerializeField, Range(0f, 0.98f)] private float throwAtPercentage;
    [Tooltip("Specifies the delay, in seconds, before the projectile is thrown.\nIn most cases this value will be 0.")]
    [SerializeField] private float delayProjectileThrow;

    private void OnEnable() {
        CurrentCooldownTime = 0;
    }

    /// <summary>
    /// This value is configured by the combat system to regulate the cooldown of the attack. It is recommended not to modify
    /// this value in your code unless you intend to reset the attack cooldown, as altering it may lead to unintended bugs.
    /// </summary>
    public float CurrentCooldownTime { get; set; }
    public string AttackName => attackName;
    public Sprite AttackIcon => attackIcon;
    public AnimationClip AttackAnimation => attackAnimation;
    public HitboxShape HitboxShape => hitboxShape;
    public LayerMask DamageableLayers => damageableLayers;
    public float DamageAmount => damageAmount;
    public bool ScalableDamage => scalableDamage;
    public float MinimumDamage => minimumDamage;
    public float Cooldown => cooldown;

    public bool DisableCastOnWall => disableCastOnWall;
    public float WallCastDistance => wallCastDistance;
    public bool ResetVelocity => resetVelocity;
    public bool CanChangeDirections => canChangeDirections;
    public Vector3 AdjustPositionOnAttackEnd => adjustPositionOnAttackEnd;
    public bool CanMoveWhileAttacking => !IsChargeableAttack && canMoveWhileAttacking;
    public float AttackMoveSpeedPercentage => attackMoveSpeedPercentage;

    public bool AttackPushesCharacter => attackPushesCharacter;
    public PushMode AttackPushMode => attackPushMode;
    public ForceMode2D ForceMode => forceMode;
    public Vector3 Force => force;
    public float DelayForceTime => delayForceTime;
    public bool UseGravity => useGravity;
    public float DragCoefficient => dragCoefficient;
    public ForceMode2D ReleaseForceMode => m_ForceMode;
    public Vector3 ReleaseFoce => m_Force;
    public float ReleaseDelayForceTime => m_DelayForceTime;
    public bool ReleaseUseGravity => m_UseGravity;
    public float ReleaseDragCoeficient => m_DragCoeficient;

    public bool IsChargeableAttack => isChargeableAttack;
    public AnimationClip InitiateChargeAnimation => initiateChargeAnimation;
    public AnimationClip ChargeAnimation => chargeAnimation;
    public float ChargeTime => chargeTime;
    public float HoldChargeTime => holdChargeTime;
    public ChargeOverTime ChargeOverTime => chargeOverTime;
    public float CooldownIfCanceled => cooldownIfCanceled;
    public bool CanMoveWhileCharging => IsChargeableAttack && canMoveWhileCharging;
    public float ChargeMoveSpeedPercentage => chargeMoveSpeedPercentage;
    public bool CanMoveOnReleaseAttack => canMoveOnReleaseAttack;

    public bool ThrowsProjectile => throwsProjectile;
    public bool ScalableProjectileDamage => scalableProjectileDamage;
    public bool ScalableProjectileVelocity => scalableProjectileVelocity;
    public bool ChooseRandomFromList => chooseRandomFromList;
    public GameObject ProjectilePrefab => projectilePrefab;
    public GameObject[] ProjectilePrefabs => projectilePrefabs;
    public float ThrowAtPercentage => throwAtPercentage;
    public float DelayProjectileThrow => delayProjectileThrow;
}
