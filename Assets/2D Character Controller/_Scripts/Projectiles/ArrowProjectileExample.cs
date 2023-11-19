using UnityEngine;

public class ArrowProjectileExample : MonoBehaviour {

    private CombatSystemProjectile csProjectile;
    private Rigidbody2D arrowRb;
    private bool canDamage = true;

    private void Awake() {
        csProjectile = GetComponent<CombatSystemProjectile>();
        arrowRb = GetComponent<Rigidbody2D>();
    }

    private void Update() {
        LookAtOnGoingDirection();
    }

    private void OnCollisionEnter2D(Collision2D collision) {
        if (csProjectile == null) {
            return;
        }
        if (canDamage == false) {
            // Ensures that if the arrow is frozen on a wall, it won't damage a damageable object upon collision.
            return;
        }

        if (csProjectile.DamageLayers == (csProjectile.DamageLayers | (1 << collision.gameObject.layer))) {
            csProjectile.DealDamageOnContact();
            Destroy(gameObject);
        }
        else {
            arrowRb.velocity = Vector2.zero;
            arrowRb.simulated = false;
            canDamage = false;
            enabled = false;
            Invoke(nameof(Fall), 3.5f);
        }
    }

    private void Fall() {
        arrowRb.simulated = true;
        Destroy(gameObject, 2f);
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
