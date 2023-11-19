using System;
using UnityEngine;
using CocaCopa;

public class EntityHealth : MonoBehaviour, IDamageable {

    public class OnTakeDamageEventArgs {
        public float damageAmount;
    }
    public event EventHandler<OnTakeDamageEventArgs> OnTakeDamage;
    public event EventHandler OnEntityDeath;

    [Tooltip("Leave this field empty if your entity wears no armour.")]
    [SerializeField] private ArmourSO armourData;
    [Tooltip("Maximum health points of the entity.")]
    [SerializeField] private float maxHealthPoints;
    [Tooltip("Current health points of the entity.")]
    [SerializeField] private float currentHealthPoints;
    [Tooltip("Whether or not your character/object is able to regen missing health points.")]
    [SerializeField] private bool canRegenHealth;
    [SerializeField] private bool interruptWhenDamaged = true;
    [Tooltip("Time in seconds at which the health regeneration can take effect, if the current health points are not equal to max health points.")]
    [SerializeField] private float regenTriggerTime;
    [Tooltip("How many health points per second the entity should gain, once the regen effect has been triggered.")]
    [SerializeField] private float regenHealthPoints;

    public ArmourSO ArmourData { get => armourData; set => armourData = value; }
    public float MaxHealthPoints { get => maxHealthPoints; set => maxHealthPoints = value; }
    public float CurrentHealthPoints { get => currentHealthPoints; set => currentHealthPoints = value; }
    public bool CanRegenHealth { get => canRegenHealth; set => canRegenHealth = value; }
    public bool InterruptHealthRegenWhenDamaged { get => interruptWhenDamaged; set => interruptWhenDamaged = value; }
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
    /// Starts health regeneration.
    /// </summary>
    /// <param name="immediate">Determines if the regeneration should happen immediately or wait for the 'RegenTriggerTimer' value first.</param>
    public void StartHealthRegeneration(bool immediate) {
        if (immediate) {
            regenTriggerTimer = 0;
        }
        enabled = true;
    }

    private bool isRegenerating = false;
    private float regenTriggerTimer;

    private void Awake() {
        currentHealthPoints = maxHealthPoints;
        regenTriggerTimer = regenTriggerTime;
        enabled = false;
    }

    private void Update() {
        RegenerateHealth();
    }

    private void RegenerateHealth() {
        if (canRegenHealth) {
            if (Utilities.TickTimer(ref regenTriggerTimer, regenTriggerTime, autoReset: false)) {
                isRegenerating = true;
                currentHealthPoints += regenHealthPoints * Time.deltaTime;
                currentHealthPoints = Mathf.Clamp(currentHealthPoints, 0, maxHealthPoints);
                if (currentHealthPoints == maxHealthPoints) {
                    isRegenerating = false;
                    regenTriggerTimer = regenTriggerTime;
                    enabled = false;
                }
            }
        }
    }

    public void TakeDamage(float amount) {
        float damageReduction = armourData != null
            ? armourData.ArmourPoints / (armourData.ArmourPoints + armourData.ArmourEffectiveness)
            : 0;
        float finalDamage = amount * (1 - damageReduction);
        ReduceHealthPoints(finalDamage);
        ManageHealthRegenOnDamage();
    }

    private void ManageHealthRegenOnDamage() {
        if (canRegenHealth) {
            if (isRegenerating && interruptWhenDamaged) {
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
            Death();
        }
        Debug.Log(name + ": I received " + amount + " damage");
    }

    public void Death() {
        OnEntityDeath?.Invoke(this, EventArgs.Empty);
        Debug.Log("Character died");
    }
}
