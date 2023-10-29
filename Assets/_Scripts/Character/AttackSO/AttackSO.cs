using UnityEngine;

[CreateAssetMenu(fileName = "NewAttack", menuName = "Create New Attack", order = 0)]
public class AttackSO : ScriptableObject {

    [SerializeField] private string attackName;
    [SerializeField] private Sprite icon;
    [Tooltip("The animation clip of the attack")]
    [SerializeField] private AnimationClip attackAnimation;
    [Tooltip("Which layers can be damaged?")]
    [SerializeField] private LayerMask whatIsDamageable;
    [Tooltip("Should the character be pushed forward once the attack is initiated?")]
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
    [Tooltip("Can this attack be charged?")]
    [SerializeField] private bool isChargeableAttack = false;
    [Tooltip("How much time should it take for the attack to be charged in seconds")]
    [SerializeField] private float chargeTime;
    [Tooltip("For how long should your character be able to hold their charged attack in seconds, before being forced to cancel it")]
    [SerializeField] private float holdChargeTime;
    [Tooltip("Should your character be able to move while they charge this attack?")]
    [SerializeField] private bool canMoveWhileCharging = false;
    [Tooltip("How much damage should be dealt by this attack")]
    [SerializeField] private float damageAmount;
    [Tooltip("Attack cooldown in seconds")]
    [SerializeField] private float cooldown;
    [Tooltip("Does this attack throw a projectile?")]
    [SerializeField] private bool throwsProjectile = false;
    [Tooltip("Projectile to spawn")]
    [SerializeField] private GameObject projectilePrefab;

    public float ChargeTimer { get; set; }

    public string AttackName => attackName;
    public Sprite Icon => icon;
    public AnimationClip AttackAnimation => attackAnimation;
    public LayerMask WhatIsDamageable => whatIsDamageable;
    public bool AttackPushesCharacter => attackPushesCharacter;
    public ForceMode2D ForceMode => forceMode;
    public Vector3 Force => force;
    public float DelayForceTime => delayForceTime;
    public bool UseGravity => useGravity;
    public float DragCoeficient => dragCoeficient;
    public bool IsChargeableAttack => isChargeableAttack;
    public float ChargeTime => chargeTime;
    public float HoldChargeTime => holdChargeTime;
    public bool CanMoveWhileCharging => canMoveWhileCharging;
    public float DamageAmount => damageAmount;
    public float Cooldown => cooldown;
    public bool ThrowsProjectile => throwsProjectile;
    public GameObject ProjectilePrefab => projectilePrefab;
}
