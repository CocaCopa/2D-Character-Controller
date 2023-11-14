using UnityEngine;

[RequireComponent(typeof(Rigidbody2D), typeof(Collider2D))]
public class CombatSystemProjectile : MonoBehaviour {

    [Tooltip("The initial velocity of the projectile when it's launched.")]
    [SerializeField] private Vector2 initialVelocity;
    [Tooltip("If the projectile is spawned by a chargeable attack, this is the minimum initial velocity, the projectile's velocity " +
        "can be set to, based on the amount of charge")]
    [SerializeField] private Vector2 minimumInitialVelocity;
    [SerializeField] private Transform hitboxTransform;
    [SerializeField] private ProjectileType projectileType;
    [SerializeField] private HitboxShape hitboxShape;
    [SerializeField] private float hitboxRadius;
    [SerializeField] private Vector2 hitboxSize;


    /*[SerializeField] private bool isExplosive = false;
    [SerializeField] private ParticleSystem explodeParticles;
    [SerializeField] private float damageRadius;*/

    private Rigidbody2D projectileRb;

    public Vector2 InitialVelocity {
        get => initialVelocity;
        set {
            if (value.sqrMagnitude < minimumInitialVelocity.sqrMagnitude) {
                value = minimumInitialVelocity;
            }
            initialVelocity = value;
        }
    }

    public int DamageLayers { get; set; }
    public float DamageAmount { get; set; }

    public ProjectileType ProjectileType => projectileType;
    public HitboxShape HitboxShape => hitboxShape;

    private void Awake() {
        projectileRb = GetComponent<Rigidbody2D>();
        enabled = false;
    }

    private void OnEnable() {
        projectileRb.velocity = initialVelocity;
    }
    private bool canDamage = true;
    private void Update() {
        Collider2D colliderHit = null;
        if (hitboxShape == HitboxShape.Circle) {
            colliderHit = Physics2D.OverlapCircle(hitboxTransform.position, hitboxRadius, DamageLayers);
        }
        else if (hitboxShape == HitboxShape.Box) {
            colliderHit = Physics2D.OverlapBox(hitboxTransform.position, hitboxSize, 0, DamageLayers);
        }
        if (colliderHit != null && colliderHit.transform.root.TryGetComponent<IDamageable>(out var damageableObject)) {
            if (canDamage) {
                canDamage = false;
                damageableObject.TakeDamage(DamageAmount);
            }
        }
        canDamage = !colliderHit;
    }

#if UNITY_EDITOR
    [SerializeField] private bool visualizeHitbox;
    private void OnDrawGizmos() {
        if (!visualizeHitbox) {
            return;
        }

        if (hitboxShape == HitboxShape.Circle) {
            Gizmos.DrawWireSphere(hitboxTransform.position, hitboxRadius);
        }
        else if (hitboxShape == HitboxShape.Box) {
            Gizmos.DrawWireCube(hitboxTransform.position, hitboxSize);
        }
    }
#endif
}
