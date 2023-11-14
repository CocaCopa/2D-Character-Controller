using UnityEngine;

public class ArrowProjectile : MonoBehaviour {

    [Tooltip("Determines whether the projectile object should continuously orient itself along its ongoing direction.")]
    [SerializeField] private bool lookAtOnGoingDirection = true;
    [Tooltip("Event that can be used to trigger actions upon collision. It's serialized to allow external configuration in the Unity Editor.")]
    [SerializeField] private UnityEngine.Events.UnityEvent onCollisionEnter;
    [Tooltip("When true, the code will manage collision behaviour specific to objects sharing the same layer.")]
    [SerializeField] private bool handleSameLayerCollision;
    [Tooltip("Choose which object should be affected by the collision")]
    [SerializeField] private AffectObject affectObject;
    [Tooltip("Defines which layers the projectile should react to upon collision.")]
    [SerializeField] private LayerMask reactOnLayers;
    [Tooltip("Determines the action to be taken upon the projectile's first collision.")]
    [SerializeField] private ProjectileContact onFirstContact;
    [Tooltip("Defines if the speed multiplier applied during ricochet remains constant or decreases.")]
    [SerializeField] private bool constantMultiplier;
    [Tooltip("Reduces the 'speedMultiplier' by a percentage upon each collision between the projectile and an object.")]
    [SerializeField, Range(0,1)] private float multiplierReductionRate;
    [Tooltip("Specifies the resultant speed of the projectile after bouncing off an object. Percentage of the 'initialVelocity' value.")]
    [SerializeField, Range(0,1)] private float speedMultiplier;
    [Tooltip("Specifies the maximum number of bounces allowed for the projectile.")]
    [SerializeField] private int allowBounces;
    [Tooltip("Defines the action to be taken when the projectile reaches the maximum allowed bounces during ricochet.")]
    [SerializeField] private ProjectileContact onRicochetEnd;

    private Rigidbody2D projectileRb;
    private CombatSystemProjectile combatSystemProjectile;
    private bool objectCollided = false;
    private int numberOfBounces = 0;
    private Vector2 initialVelocity;

    public ProjectileContact OnFirstContact => onFirstContact;
    public ProjectileContact OnRicochetEnd { get => onRicochetEnd; set => onRicochetEnd = value; }
    public bool HandleSameLayerCollision => handleSameLayerCollision;
    public LayerMask ReactOnLayers => reactOnLayers;
    public bool ConstantMultiplier => constantMultiplier;

    private void Awake() {
        projectileRb = GetComponent<Rigidbody2D>();
        combatSystemProjectile = GetComponent<CombatSystemProjectile>();
    }

    private void Start() {
        initialVelocity = combatSystemProjectile.InitialVelocity;
    }

    private void OnCollisionEnter2D(Collision2D collision) {
        objectCollided = true;
        if (handleSameLayerCollision) {
            CollisionWithSameLayer(collision);
        }
        CollisionWithSpecifiedLayers(collision, onFirstContact);
    }

    private void CollisionWithSameLayer(Collision2D collision) {
        if ((gameObject.layer & collision.gameObject.layer) != 0) {
            float mySpeed = projectileRb.velocity.sqrMagnitude;
            float otherObjectSpeed = collision.transform.GetComponent<Rigidbody2D>().velocity.sqrMagnitude;

            if (affectObject == AffectObject.Both) {
                Destroy(gameObject);
                Destroy(collision.gameObject);
            }
            else if (affectObject == AffectObject.Slowest) {
                if (mySpeed > otherObjectSpeed)
                    Destroy(collision.gameObject);
                else if (mySpeed < otherObjectSpeed)
                    Destroy(gameObject);
            }
            else if (affectObject == AffectObject.Fastest) {
                if (mySpeed > otherObjectSpeed)
                    Destroy(gameObject);
                else if (mySpeed < otherObjectSpeed)
                    Destroy(collision.gameObject);
            }
        }
    }

    private void CollisionWithSpecifiedLayers(Collision2D collision, ProjectileContact onContact) {
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
        if (initialVelocity.magnitude * speedMultiplier < 1f) {
            numberOfBounces = allowBounces;
        }
        if (numberOfBounces < allowBounces) {
            Vector3 currentDirection = projectileRb.velocity.normalized;
            Vector3 perpendicularDirection = transform.right.x * Vector3.Cross(currentDirection, Vector3.forward);
            perpendicularDirection.Normalize();
            if (Physics2D.Raycast(collision.contacts[0].point, Vector2.down, 0.1f, reactOnLayers)) {
                perpendicularDirection *= -1f;
            }
            projectileRb.velocity = initialVelocity.magnitude * speedMultiplier * perpendicularDirection;
            if (!constantMultiplier) {
                speedMultiplier *= 1 - multiplierReductionRate;
            }
            numberOfBounces++;
            objectCollided = false;
        }
        else {
            CollisionWithSpecifiedLayers(collision, onRicochetEnd);
        }
    }
}
