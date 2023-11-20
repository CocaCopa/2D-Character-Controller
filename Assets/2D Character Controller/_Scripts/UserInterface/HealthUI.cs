using UnityEngine;
using UnityEngine.UI;

public class HealthUI : MonoBehaviour {

    [SerializeField] private Image healthImageLeft;
    [SerializeField] private Image healthImageRight;
    [SerializeField] private Gradient healthBarColor;

    private EntityHealth health;

    private void Awake() {
        health = transform.root.GetComponent<EntityHealth>();
        health.OnTakeDamage += EntityHealth_OnTakeDamage;
    }

    private void Update() {
        BarFillAmount();
        BarGradientColor();
    }

    private void BarFillAmount() {
        healthImageLeft.fillAmount =
        healthImageRight.fillAmount = health.CurrentHealthPoints / health.MaxHealthPoints;
        if (healthImageRight.fillAmount == 1) {
            enabled = false;
        }
    }

    private void BarGradientColor() {
        healthImageLeft.color =
        healthImageRight.color = healthBarColor.Evaluate(healthImageLeft.fillAmount);
    }

    private void EntityHealth_OnTakeDamage(object sender, EntityHealth.OnTakeDamageEventArgs e) {
        enabled = true;
    }
}
