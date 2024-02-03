using System;
using UnityEngine;
using CocaCopa;

public class EntityHealth : MonoBehaviour, IDamageable {

    public class OnTakeDamageEventArgs {
        public float damageAmount;
    }
    public event EventHandler<OnTakeDamageEventArgs> OnTakeDamage;
    public event EventHandler OnEntityDeath;
    public event EventHandler OnEntityAlive;

    [Tooltip("Leave this field empty if your entity wears no armour.")]
    [SerializeField] private ArmourSO armourData;
    [Tooltip("Maximum health points of the entity.")]
    [SerializeField] private float maxHealthPoints;
    [Tooltip("Current health points of the entity.")]
    [SerializeField] private float currentHealthPoints;
    [Tooltip("Whether or not your entity is able to regen missing health points.")]
    [SerializeField] private bool canRegenHealth;
    [Tooltip("Choose whether or not your entity should stop regenerating health, when they take damage.")]
    [SerializeField] private bool interruptWhenDamaged = true;
    [Tooltip("Time in seconds at which the health regeneration can take effect, if the current health points are not equal to max health points.")]
    [SerializeField] private float regenTriggerTime;
    [Tooltip("How many health points per second the entity should gain, once the regen effect has been triggered.")]
    [SerializeField] private float regenHealthPoints;

    /// <summary>
    /// Indicates whether the entity is alive.
    /// </summary>
    public bool IsAlive => currentHealthPoints > 0;
    public ArmourSO ArmourData { get => armourData; set => armourData = value; }
    /// <summary>
    /// Maximum health points of the entity.
    /// </summary>
    public float MaxHealthPoints { get => maxHealthPoints; set => maxHealthPoints = value; }
    /// <summary>
    /// The current health points of the entity.
    /// </summary>
    public float CurrentHealthPoints { get => currentHealthPoints; set => currentHealthPoints = value; }
    /// <summary>
    /// Whether or not your entity is able to regen missing health points.
    /// </summary>
    public bool CanRegenHealth { get => canRegenHealth; set => canRegenHealth = value; }
    /// <summary>
    /// Choose whether or not your entity should stop regenerating health, when they take damage.
    /// </summary>
    public bool InterruptHealthRegenWhenDamaged { get => interruptWhenDamaged; set => interruptWhenDamaged = value; }
    /// <summary>
    /// The amount of health points the entity should regenrate, once the health regeneration effect has been triggered.
    /// </summary>
    public float RegenHealthPoints { get => regenHealthPoints; set => regenHealthPoints = value; }
    /// <summary>
    /// Time in seconds before health regeneration triggers.
    /// </summary>
    public float RegenTriggerTime { get => regenTriggerTime; set => regenTriggerTime = value; }
    /// <summary>
    /// Triggers health regeneration when this value reaches 0.
    /// </summary>
    public float RegenTriggerTimer { get => regenTriggerTimer; set => regenTriggerTimer = value; }
    /// <summary>
    /// Indicates whether the entity is regenerating health points.
    /// </summary>
    public bool IsRegeneratingHealth => isRegenerating;
    /// <summary>
    /// Starts health regeneration.
    /// </summary>
    /// <param name="immediate">Determines if the regeneration should happen immediately or wait for the 'RegenTriggerTimer' value first.</param>
    public void StartHealthRegeneration(bool immediate) {
        if (immediate) {
            regenTriggerTimer = 0;
        }
        canRegenHealth = true;
        enabled = true;
    }

    private bool defaultCanRegenHealth = false;
    private bool isRegenerating = false;
    private float regenTriggerTimer;

    private void Awake() {
        defaultCanRegenHealth = canRegenHealth;
        currentHealthPoints = maxHealthPoints;
        regenTriggerTimer = regenTriggerTime;
        enabled = false;
    }

    private void Update() {
        if (canRegenHealth) {
            RegenerateHealth();
        }
        else {
            enabled = false;
        }
    }

    private void RegenerateHealth() {
        if (Utilities.TickTimer(ref regenTriggerTimer, regenTriggerTime, autoReset: false)) {
            isRegenerating = true;
            currentHealthPoints += regenHealthPoints * Time.deltaTime;
            currentHealthPoints = Mathf.Clamp(currentHealthPoints, 0, maxHealthPoints);
        }
        if (currentHealthPoints == maxHealthPoints) {
            isRegenerating = false;
            regenTriggerTimer = regenTriggerTime;
            enabled = false;
        }
    }

    public void TakeDamage(float amount) {
        float damageReduction = armourData != null
            ? armourData.ArmourPoints / (armourData.ArmourPoints + armourData.ArmourEffectiveness)
            : 0;
        float finalDamage = amount * (1 - damageReduction);
        ManageHealthRegenOnDamage();
        ReduceHealthPoints(finalDamage);
    }

    private void ManageHealthRegenOnDamage() {
        if (canRegenHealth) {
            if (isRegenerating && interruptWhenDamaged) {
                regenTriggerTimer = regenTriggerTime;
            }
            else if (!isRegenerating) {
                regenTriggerTimer = regenTriggerTime;
            }
            enabled = true;
        }
    }

    private void ReduceHealthPoints(float amount) {
        currentHealthPoints -= amount;
        currentHealthPoints = Mathf.Clamp(currentHealthPoints, 0, maxHealthPoints);
        OnTakeDamage?.Invoke(this, new OnTakeDamageEventArgs {
            damageAmount = amount
        });

        if (currentHealthPoints <= 0) {
            KillEntity();
        }
    }

    /// <summary>
    /// Kills the entity.
    /// </summary>
    public void KillEntity() {
        canRegenHealth = false;
        enabled = false;
        OnEntityDeath?.Invoke(this, EventArgs.Empty);
    }

    /// <summary>
    /// Respawns the entity.
    /// </summary>
    public void Alive() {
        regenTriggerTimer = regenTriggerTime;
        canRegenHealth = defaultCanRegenHealth;
        currentHealthPoints = maxHealthPoints;
        enabled = true;
        OnEntityAlive?.Invoke(this, EventArgs.Empty);
    }
}
