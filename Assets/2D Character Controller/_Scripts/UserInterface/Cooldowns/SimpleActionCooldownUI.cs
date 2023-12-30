using UnityEngine;
using UnityEngine.UI;

public class SimpleActionCooldownUI : MonoBehaviour {

    [SerializeField] private Transform targetCharacter;
    [SerializeField] private Image cooldownImage;

    private HumanoidController characterController;
    private float cooldown;

    private void Awake() {
        characterController = targetCharacter.GetComponent<HumanoidController>();
        cooldownImage.fillAmount = 0;
    }

    private void Start() {
        characterController.OnCharacterDash += EnableScript;
    }

    private void Update() {
        float fillAmount = (cooldown - Time.time) / characterController.DashCooldown;
        fillAmount = Mathf.Clamp01(fillAmount);
        cooldownImage.fillAmount = fillAmount;
        if (cooldownImage.fillAmount == 0) {
            enabled = false;
            cooldownImage.gameObject.SetActive(false);
        }
    }

    private void EnableScript(object sender, System.EventArgs e) {
        cooldownImage.gameObject.SetActive(true);
        enabled = true;
        cooldownImage.fillAmount = 1;
        cooldown = Time.time + characterController.DashCooldown;
    }
}
