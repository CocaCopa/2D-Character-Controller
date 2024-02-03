using UnityEngine;

public class ExampleAIController : HumanoidController {

    [Header("--- Death ---")]
    [Tooltip("Time in seconds at which the character will disappear after death.")]
    [SerializeField] private float disappearTime;
    [Tooltip("Time in seconds at which the character will respawn after death.")]
    [SerializeField] private float respawnTime;

    [Header("--- Gun Normal Attack ---")]
    [SerializeField] private AttackSO gunFireAttack;
    [Tooltip("Position at which the projectile of this attack will be spawned.")]
    [SerializeField] private Transform bulletSpawnTransform;
    [Tooltip("Position at which the muzzle flash effect should be spawned.")]
    [SerializeField] private Transform muzzleEffectSpawnTransform;
    [Tooltip("Prefab to spawn as muzzle flash.")]
    [SerializeField] private GameObject muzzleEffectPrefab;
    [Tooltip("Duration before the game object is automatically destroyed.")]
    [SerializeField] private float destroyMuzzleEffectTime;

    private EntityHealth entityHealth;
    private Transform playerTransform;

    protected override void Awake() {
        base.Awake();
        entityHealth = GetComponent<EntityHealth>();
        playerTransform = FindObjectOfType<ExamplePlayerController>().transform;
    }

    protected override void Start() {
        base.Start();
        entityHealth.OnEntityDeath += EntityHealth_OnEntityDeath;
        characterCombat.OnProjectileThrown += Combat_ProjectileThrown;
    }

    protected override void Update() {
        base.Update();
        Controller();
        characterCombat.PerformNormalAttack(gunFireAttack, false, bulletSpawnTransform);
    }

    private void Combat_ProjectileThrown(object sender, CharacterCombat.OnProjectileThrownEventArgs e) {
        if (e.attackData == gunFireAttack) {
            GameObject effect = Instantiate(muzzleEffectPrefab, muzzleEffectSpawnTransform.position, Quaternion.identity);
            effect.transform.right = transform.right;
            Destroy(effect, destroyMuzzleEffectTime);
        }
    }

    private void Controller() {
        Vector3 lookDirection = (playerTransform.position - transform.position).normalized;
        Vector3 eulerAngles = lookDirection.x > 0 ? new(0, 0, 0) : new(0, 180, 0);
        transform.eulerAngles = eulerAngles;
    }

    private void EntityHealth_OnEntityDeath(object sender, System.EventArgs e) {
        enabled = false;
        characterRb.simulated = false;
        activeCollider.enabled = false;
        Invoke(nameof(Disable), disappearTime);
        Invoke(nameof(Respawn), respawnTime);
    }

    private void Disable() {
        GetComponentInChildren<SpriteRenderer>().enabled = false;
    }

    private void Respawn() {
        entityHealth.Alive();
        characterRb.simulated = true;
        activeCollider.enabled = true;
        GetComponentInChildren<SpriteRenderer>().enabled = true;
        enabled = true;
    }
}
