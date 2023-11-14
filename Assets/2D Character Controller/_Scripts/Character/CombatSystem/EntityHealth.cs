using UnityEngine;

public class EntityHealth : MonoBehaviour, IDamageable {

    public class OnTakeDamageEventArgs {
        public float amount;
    }
    public event System.EventHandler<OnTakeDamageEventArgs> OnTakeDamage;

    [Tooltip("Leave this field empty if your entity wears no armour.")]
    [SerializeField] private ArmourSO armourData;
    [Tooltip("Maximum health points of the entity.")]
    [SerializeField] private float maxHealthPoints;
    [Tooltip("Current health points of the entity.")]
    [SerializeField] private float currentHealthPoints;
    [Tooltip("Whether or not your character/object is able to regen missing health points.")]
    [SerializeField] private float canRegenHealth;
    [Tooltip("Time in seconds at which the health regeneration can take effect, if the current health points are not equal to max health points.")]
    [SerializeField] private float regenTriggerTime;
    [Tooltip("Time in seconds to regen back to full health points, once the health regeneration has been triggered.")]
    [SerializeField] private float regenToFullTime;

    public ArmourSO ArmourData { get => armourData; set => armourData = value; }
    public float MaxHealthPoints { get => maxHealthPoints; set => maxHealthPoints = value; }
    public float CurrentHealthPoints { get => currentHealthPoints; set => currentHealthPoints = value; }

    private void Awake() {
        currentHealthPoints = maxHealthPoints;
    }

    public void TakeDamage(float amount) {
        float damageReduction = armourData != null
            ? armourData.ArmourPoints / (armourData.ArmourPoints + armourData.ArmourEffectiveness)
            : 0;
        float finalDamage = amount * (1 - damageReduction);
        ReduceHealthPoints(finalDamage);
    }

    private void ReduceHealthPoints(float amount) {
        currentHealthPoints -= amount;
        if (currentHealthPoints <= 0) {
            Debug.Log("Character died");
        }
        OnTakeDamage?.Invoke(this, new OnTakeDamageEventArgs {
            amount = amount
        });
        Debug.Log(name + ": I received " + amount + " damage");
    }
}
