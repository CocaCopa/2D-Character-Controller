using UnityEngine;

public enum ProjectileContact {
    None,
    Destroy,
    FreezePosition
}

[RequireComponent(typeof(Rigidbody2D), typeof(Collider2D))]
public class CombatSystemProjectile : MonoBehaviour {

    [SerializeField] private Vector2 velocity;
    [SerializeField] private Vector2 minimumVelocity;
    [SerializeField] private bool lookAtOnGoingDirection = true;
    [SerializeField] private LayerMask allowedLayers;
    [SerializeField] private ProjectileContact onContact;
    //[SerializeField] private UnityEngine.Events.UnityEvent customEvent;

    private Rigidbody2D projectileRb;
    private bool objectCollided = false;

    public Vector2 Velocity {
        get => velocity;
        set {
            if (value.sqrMagnitude >= minimumVelocity.sqrMagnitude) {
                velocity = value;
            }
        }
    }

    private void Awake() {
        projectileRb = GetComponent<Rigidbody2D>();
        enabled = false;
    }

    private void OnEnable() {
        projectileRb.velocity = velocity;
    }

    private void OnCollisionEnter2D(Collision2D collision) {
        objectCollided = true;
        if (allowedLayers == (allowedLayers | (1 << collision.gameObject.layer))) {
            if (onContact == ProjectileContact.None) {
                return;
            }

            if (onContact == ProjectileContact.Destroy) {
                Destroy(gameObject);
            }
            else if (onContact == ProjectileContact.FreezePosition) {
                projectileRb.isKinematic = true;
                projectileRb.velocity = Vector2.zero;
                projectileRb.angularVelocity = 0;
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
}
