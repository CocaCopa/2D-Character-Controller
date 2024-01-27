using UnityEngine;

[RequireComponent(typeof(CombatSystemProjectile))]
public class BulletProjectileExample : MonoBehaviour {

    [Tooltip("Prefab to spawn once a collision has occured")]
    [SerializeField] private GameObject impactEffectPrefab;
    [Tooltip("Duration before the game object is automatically destroyed.")]
    [SerializeField] private float impactEffectTime;

    private CombatSystemProjectile csProjectile;

    private void Awake() {
        csProjectile = GetComponent<CombatSystemProjectile>();
    }

    private void OnCollisionEnter2D(Collision2D collision) {
        GameObject effect = Instantiate(impactEffectPrefab, collision.contacts[0].point, Quaternion.identity);
        effect.transform.right = -collision.contacts[0].normal;
        Destroy(effect, impactEffectTime);
        csProjectile.DealDamageOnContact();
        Destroy(gameObject);
    }
}
