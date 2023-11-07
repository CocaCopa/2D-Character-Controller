using UnityEngine;

[RequireComponent(typeof(Rigidbody2D))]
public class ArrowProjectile : MonoBehaviour {

    [SerializeField] private Vector2 initialVelocity;
    private Rigidbody2D arrowRb;

    public Vector2 InitialVelocity { get => initialVelocity; set => initialVelocity = value; }

    private void Awake() {
        arrowRb = GetComponent<Rigidbody2D>();
        enabled = false;
    }

    private void OnEnable() {
        arrowRb.velocity = initialVelocity;
    }

    private void Update() {
        LookAtOnGoingDirection();
    }

    private void LookAtOnGoingDirection() {
        Vector2 velocity = arrowRb.velocity;
        if (velocity != Vector2.zero) {
            float angle = Mathf.Atan2(velocity.y, velocity.x);
            float degrees = Mathf.Rad2Deg * angle;
            transform.rotation = Quaternion.Euler(0, 0, degrees);
        }
    }
}
