using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(Collider2D), typeof(Rigidbody2D))]
public class CombatSystemProjectile : MonoBehaviour {

    [Tooltip("The initial velocity of the projectile when it's launched.")]
    [SerializeField] private Vector2 initialVelocity;
    [Tooltip("If the projectile is spawned by a chargeable attack, this is the minimum initial velocity, the projectile's velocity can be set to, based on the amount of charge")]
    [SerializeField] private Vector2 minimumInitialVelocity;
    [SerializeField] private Transform hitboxTransform;
    [SerializeField] private HitboxShape hitboxShape;
    [SerializeField] private float hitboxRadius;
    [SerializeField] private Vector2 hitboxSize;

    private GameObject lastGameObjectHit;
    private Rigidbody2D projectileRb;
    private List<Collider2D> colliders = new();
    private List<float> damageTimers = new();
    private float damageTimer = 0;

    /// <summary>
    /// The initial velocity of the projectile when it's launched.
    /// </summary>
    public Vector2 InitialVelocity {
        get => initialVelocity;
        set {
            if (value.sqrMagnitude < minimumInitialVelocity.sqrMagnitude) {
                value = minimumInitialVelocity;
            }
            initialVelocity = value;
        }
    }

    /// <summary>
    /// Indicates which layers can be damaged.
    /// </summary>
    public int DamageLayers { get; set; }
    /// <summary>
    /// Indicates the amount of damage the projectile will deal upon collision with a damageable object.
    /// </summary>
    public float DamageAmount { get; set; }
    /// <summary>
    /// Indicates the shape of the projectile's hitbox. It can be a cirle or a box.
    /// </summary>
    public HitboxShape HitboxShape => hitboxShape;

    private void Awake() {
        projectileRb = GetComponent<Rigidbody2D>();
        enabled = false;
    }

    private void OnEnable() {
        projectileRb.velocity = initialVelocity;
    }

    /// <summary>
    /// Deals damage to the first collider that enters the specified hit box.
    /// </summary>
    public void DealDamageOnContact() {
        Collider2D colliderHit = null;
        if (hitboxShape == HitboxShape.Circle) {
            colliderHit = Physics2D.OverlapCircle(hitboxTransform.position, hitboxRadius, DamageLayers);
        }
        else if (hitboxShape == HitboxShape.Box) {
            colliderHit = Physics2D.OverlapBox(hitboxTransform.position, hitboxSize, 0, DamageLayers);
        }
        if (colliderHit != null && colliderHit.transform.root.TryGetComponent<IDamageable>(out var damageableObject)) {
            if (lastGameObjectHit != colliderHit.gameObject) {
                lastGameObjectHit = colliderHit.gameObject;
                damageableObject.TakeDamage(DamageAmount);
            }
        }
    }

    /// <summary>
    /// Deals damage instantly to any new colliders that enter the specified hitbox and then deals damage again, to all targets, every 'x' amount of seconds.
    /// </summary>
    /// <param name="damageAgainInSeconds">Damage frequency.</param>
    public void ContinuousAreaOfEffectDamage(float damageAgainInSeconds) {
        Collider2D[] colliderHit = null;
        if (hitboxShape == HitboxShape.Circle) {
            colliderHit = Physics2D.OverlapCircleAll(hitboxTransform.position, hitboxRadius, DamageLayers);
        }
        else if (hitboxShape == HitboxShape.Box) {
            colliderHit = Physics2D.OverlapBoxAll(hitboxTransform.position, hitboxSize, 0, DamageLayers);
        }

        foreach (var collider in colliderHit) {
            if (collider.transform.root.TryGetComponent<IDamageable>(out var damageableObject)) {
                if (!colliders.Contains(collider)) {
                    colliders.Add(collider);
                    damageTimers.Add(damageAgainInSeconds);
                    damageableObject.TakeDamage(DamageAmount);
                }
            }
        }
        if (colliders.Count > 0) {
            for (int i = 0; i < colliders.Count; i++) {
                damageTimer = damageTimers[i];
                CocaCopa.Utilities.TickTimer(ref damageTimer, damageAgainInSeconds, false);
                damageTimers[i] = damageTimer;
                if (damageTimer == 0) {
                    colliders.Remove(colliders[i]);
                    damageTimers.RemoveAt(i);
                }
            }
        }
        if (colliderHit == null) {
            colliders.Clear();
        }
    }

    /// <summary>
    /// Deals damage to all colliders inside the specified hitbox and then destroys the projectile.
    /// </summary>
    public void ExplodeOnContact() {
        Collider2D[] collidersHit = null;
        if (hitboxShape == HitboxShape.Circle) {
            collidersHit = Physics2D.OverlapCircleAll(hitboxTransform.position, hitboxRadius, DamageLayers);
        }
        else if (hitboxShape == HitboxShape.Box) {
            collidersHit = Physics2D.OverlapBoxAll(hitboxTransform.position, hitboxSize, 0, DamageLayers);
        }

        if (collidersHit != null) {
            foreach (var collider in collidersHit) {
                if (collider.transform.root.TryGetComponent<IDamageable>(out var damageableObject)) {
                    damageableObject.TakeDamage(DamageAmount);
                }
            }
        }
        Destroy(gameObject);
    }

#if UNITY_EDITOR
    [Tooltip("Visualizes the shape of the hitbox shape and size in the scene.")]
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
