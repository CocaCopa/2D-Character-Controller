using UnityEngine;

public enum ChargeOverTime {
    ForceCancel,
    ForceRelease
}

[CreateAssetMenu(fileName = "NewAttack", menuName = "Create New Attack", order = 0)]
public class AttackSO : ScriptableObject {

    [SerializeField] private string attackName;
    [SerializeField] private Sprite attackIcon;
    [Tooltip("The animation clip of the attack")]
    [SerializeField] private AnimationClip attackAnimation;
    [Tooltip("Specify which layers can be damaged")]
    [SerializeField] private LayerMask whatIsDamageable;
    [Tooltip("How much damage should be dealt by this attack")]
    [SerializeField] private float damageAmount;
    [Tooltip("Attack cooldown in seconds")]
    [SerializeField] private float cooldown;
    [Tooltip("Wether or not your character should be able to cast the attack, if a wall is detected in front of them")]
    [SerializeField] private bool disableCastOnWall = false;
    [Tooltip("Specify at what distance away from a wall your character must be, in order to allow them to cast the attack")]
    [SerializeField] private float wallCastDistance;
    [Tooltip("True, sets your character's velocity to Vector3.zero when the attack is initiated. " +
        "False, your character will continue moving at a constant speed based on their velocity before the attack.")]
    [SerializeField] private bool resetVelocity = true;
    [Tooltip("Determines wether or not your character can change directions while they're attacking")]
    [SerializeField] private bool canChangeDirections = false;
    [Tooltip("If your animation offsets your character during the attack, you can adjust their position by setting an offset. " +
        "After the attack, your character will teleport to that offseted position")]
    [SerializeField] private Vector3 adjustPositionOnAttackEnd;
    [Tooltip("Determines if the character should be able to move while they cast an attack")]
    [SerializeField] private bool canMoveWhileAttacking;
    [Tooltip("Adjusts the character's movement speed as a percentage of their maximum speed")]
    [SerializeField, Range(0,1)] private float attackMoveSpeedPercentage;
    [Tooltip("If 'true,' a force will be applied to the character in the direction they are facing when the attack is initiated")]
    [SerializeField] private bool attackPushesCharacter;
    [Tooltip("Choose which Rigidbody.AddForce() force mode should be applied")]
    [SerializeField] private ForceMode2D forceMode = ForceMode2D.Impulse;
    [Tooltip("How much force should be applied")]
    [SerializeField] private Vector2 force;
    [Tooltip("Delay force application in seconds")]
    [SerializeField] private float delayForceTime;
    [Tooltip("Whether the character should be affected by gravity for the duration of the attack")]
    [SerializeField] private bool useGravity;
    [Tooltip("Change the drag coeficient of the Rigidbody2D for the duration of the attack")]
    [SerializeField] private float dragCoeficient;
    [Tooltip("Wether or not this attack can be charged")]
    [SerializeField] private bool isChargeableAttack = false;
    [Tooltip("Animation to play when the charge attack is initiated")]
    [SerializeField] private AnimationClip chargeAnimation;
    [Tooltip("How much time should it take for the attack to be charged in seconds")]
    [SerializeField] private float chargeTime;
    [Tooltip("For how long should your character be able to hold their charged attack in seconds, before being forced to cancel it")]
    [SerializeField] private float holdChargeTime;
    [Tooltip("Specify the action to be taken when the attack is held for longer than the designated hold charge time")]
    [SerializeField] private ChargeOverTime chargeOverTime = ChargeOverTime.ForceRelease;
    [Tooltip("Cooldown to set the attack on, if is held more than 'holdChargeTime' or if the attack is canceled")]
    [SerializeField] private float cooldownIfCanceled;
    [Tooltip("If 'true', your character will be allowed to move while they charge the attack")]
    [SerializeField] private bool canMoveWhileCharging = false;
    [Tooltip("Adjusts the character's movement speed as a percentage of their maximum speed")]
    [SerializeField, Range(0,1)] private float chargeMoveSpeedPercentage;
    [Tooltip("True if you want your character to be allowed to move as soon as the attack is released")]
    [SerializeField] private bool canMoveOnReleaseAttack;
    [Tooltip("Does this attack throw a projectile?")]
    [SerializeField] private bool throwsProjectile = false;
    [Tooltip("Projectile to spawn")]
    [SerializeField] private GameObject projectilePrefab;
    [Tooltip("The projectile will be thrown when the 'AttackAnimation' has played the specified percentage")]
    [SerializeField, Range(0, 1)] private float throwAtPercentage;
    [Tooltip("Specifies the delay, in seconds, before the projectile is thrown.")]
    [SerializeField] private float delayProjectileThrow;

    private void OnEnable() => CurrentCooldownTime = 0;

    public float CurrentCooldownTime { get; set; }
    public float ChargeTimer { get; set; }
    public string AttackName => attackName;
    public Sprite AttackIcon => attackIcon;
    public AnimationClip AttackAnimation => attackAnimation;
    public LayerMask WhatIsDamageable => whatIsDamageable;
    public float DamageAmount => damageAmount;
    public float Cooldown => cooldown;
    public bool DisableCastOnWall => disableCastOnWall;
    public float WallCastDistance => wallCastDistance;
    public bool ResetVelocity => resetVelocity;
    public bool CanChangeDirections => canChangeDirections;
    public Vector3 AdjustPositionOnAttackEnd => adjustPositionOnAttackEnd;
    public bool CanMoveWhileAttacking => !IsChargeableAttack && canMoveWhileAttacking;
    public float AttackMoveSpeedPercentage => attackMoveSpeedPercentage;
    public bool AttackPushesCharacter => attackPushesCharacter;
    public ForceMode2D ForceMode => forceMode;
    public Vector3 Force => force;
    public float DelayForceTime => delayForceTime;
    public bool UseGravity => useGravity;
    public float DragCoeficient => dragCoeficient;
    public bool IsChargeableAttack => isChargeableAttack;
    public AnimationClip ChargeAnimation => chargeAnimation;
    public float ChargeTime => chargeTime;
    public float HoldChargeTime => holdChargeTime;
    public ChargeOverTime ChargeOverTime => chargeOverTime;
    public float CooldownIfCanceled => cooldownIfCanceled;
    public bool CanMoveWhileCharging => IsChargeableAttack && canMoveWhileCharging;
    public float ChargeMoveSpeedPercentage => chargeMoveSpeedPercentage;
    public bool CanMoveOnReleaseAttack => canMoveOnReleaseAttack;
    public bool ThrowsProjectile => throwsProjectile;
    public GameObject ProjectilePrefab => projectilePrefab;
    public float ThrowAtPercentage => throwAtPercentage;
    public float DelayProjectileThrow => delayProjectileThrow;
}
