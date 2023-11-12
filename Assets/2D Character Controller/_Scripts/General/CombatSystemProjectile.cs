using UnityEngine;

public enum ProjectileContact {
    DefaultPhysics,
    Destroy,
    FreezePosition,
    Ricochet
}

[RequireComponent(typeof(Rigidbody2D), typeof(Collider2D))]
public class CombatSystemProjectile : MonoBehaviour {

    [Tooltip("The initial velocity of the projectile when it's launched.")]
    [SerializeField] private Vector2 initialVelocity;
    [Tooltip("If the projectile is spawned by a chargeable attack, this is the minimum initial velocity, the projectile's velocity " +
        "can be set to, based on the amount of charge")]
    [SerializeField] private Vector2 minimumInitialVelocity;
    [Tooltip("Enable to create custom behavior entirely from scratch.")]
    [SerializeField] private bool useCustomBehaviour = false;
    [Tooltip("Determines whether the projectile object should continuously orient itself along its ongoing direction.")]
    [SerializeField] private bool lookAtOnGoingDirection = true;
    [Tooltip("Event that can be used to trigger actions upon collision. It's serialized to allow external configuration in the Unity Editor.")]
    [SerializeField] private UnityEngine.Events.UnityEvent OnCollisionEnter;
    [Tooltip("Defines which layers the projectile should react to upon collision.")]
    [SerializeField] private LayerMask reactOnLayers;
    [Tooltip("Determines the action to be taken upon the projectile's first collision.")]
    [SerializeField] private ProjectileContact onFirstContact;
    [Tooltip("Defines if the speed multiplier applied during ricochet remains constant or decreases.")]
    [SerializeField] private bool constantMultiplier;
    [Tooltip("A multiplier applied to the ricochet speed.")]
    [SerializeField, Range(0,1)] private float speedMultiplier;
    [Tooltip("Specifies the maximum number of bounces allowed for the projectile.")]
    [SerializeField] private int allowBounces;
    [Tooltip("Defines the action to be taken when the projectile reaches the maximum allowed bounces during ricochet.")]
    [SerializeField] private ProjectileContact onRicochetEnd;

    private Rigidbody2D projectileRb;
    private bool objectCollided = false;
    private int numberOfBounces = 0;

    public Vector2 Velocity {
        get => initialVelocity;
        set {
            if (value.sqrMagnitude < minimumInitialVelocity.sqrMagnitude) {
                value = minimumInitialVelocity;
            }
            initialVelocity = value;
        }
    }
    public ProjectileContact OnFirstContact => onFirstContact;
    public bool UseCustomBehaviour => useCustomBehaviour;

    private void Awake() {
        projectileRb = GetComponent<Rigidbody2D>();
        enabled = false;
    }

    private void OnEnable() {
        projectileRb.velocity = initialVelocity;
    }

    private void OnCollisionEnter2D(Collision2D collision) {
        if (useCustomBehaviour) {
            return;
        }
        objectCollided = true;
        HandleCollision(collision, onFirstContact);
    }

    private void HandleCollision(Collision2D collision, ProjectileContact onContact) {
        if (reactOnLayers == (reactOnLayers | (1 << collision.gameObject.layer))) {
            switch (onContact) {
                case ProjectileContact.DefaultPhysics:
                break;
                case ProjectileContact.Destroy:
                Destroy(gameObject);
                break;
                case ProjectileContact.FreezePosition:
                FreezePosition();
                break;
                case ProjectileContact.Ricochet:
                Ricochet(collision);
                break;
            }
        }
    }

    private void Update() {
        if (useCustomBehaviour) {
            enabled = false;
        }
        if (lookAtOnGoingDirection && !objectCollided) {
            LookAtOnGoingDirection();
        }
    }

    private void LookAtOnGoingDirection() {
        Vector2 velocity = projectileRb.velocity;
        if (velocity != Vector2.zero) {
            float angle = Mathf.Atan2(velocity.y, velocity.x);
            float degrees = Mathf.Rad2Deg * angle;
            transform.rotation = Quaternion.Euler(0, 0, degrees);
        }
    }

    private void FreezePosition() {
        projectileRb.isKinematic = true;
        projectileRb.velocity = Vector2.zero;
        projectileRb.angularVelocity = 0f;
    }

    private void Ricochet(Collision2D collision) {
        if (numberOfBounces < allowBounces) {
            Vector3 currentDirection = projectileRb.velocity.normalized;
            Vector3 perpendicularDirection = -Vector3.Cross(currentDirection, Vector3.forward).normalized;
            
            projectileRb.velocity = perpendicularDirection * initialVelocity.magnitude * speedMultiplier;
            if (!constantMultiplier) {
                speedMultiplier /= 2f;
            }
            numberOfBounces++;
        }
        else {
            HandleCollision(collision, onRicochetEnd);
        }
    }
}
