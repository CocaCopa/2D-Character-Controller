using UnityEngine;

public class AIController : HumanoidController {

    [Header("--- Player Character ---")]
    [SerializeField] private Transform playerTransform;

    [Header("--- Death ---")]
    [Tooltip("Time in seconds at which the character will disappear after death.")]
    [SerializeField] private float disappearTime;
    [Tooltip("Time in seconds at which the character will respawn after death.")]
    [SerializeField] private float respawnTime;

    private EntityHealth entityHealth;

    protected override void Awake() {
        base.Awake();
        entityHealth = GetComponent<EntityHealth>();
    }

    protected override void Start() {
        base.Start();
        entityHealth.OnEntityDeath += EntityHealth_OnEntityDeath;
    }

    private void Disable() {
        GetComponentInChildren<SpriteRenderer>().enabled = false;
    }

    protected override void Update() {
        base.Update();
        Controller();
    }

    private void Controller() {
        Vector3 lookDirection = (playerTransform.position - transform.position).normalized;
        Vector3 eulerAngles = lookDirection.x > 0 ? new(0, 0, 0) : new(0, 180, 0);
        transform.eulerAngles = eulerAngles;
    }

    private void Respawn() {
        entityHealth.Alive();
        characterRb.simulated = true;
        activeCollider.enabled = true;
        GetComponentInChildren<SpriteRenderer>().enabled = true;
        enabled = true;
    }

    private void EntityHealth_OnEntityDeath(object sender, System.EventArgs e) {
        enabled = false;
        characterRb.simulated = false;
        activeCollider.enabled = false;
        Invoke(nameof(Disable), disappearTime);
        Invoke(nameof(Respawn), respawnTime);
    }
}
