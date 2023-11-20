using UnityEngine;
using UnityEngine.UI;

public class SingleAttackCooldownUI : MonoBehaviour {

    [SerializeField] private Transform targetCharacter;
    [SerializeField] private Image cooldownImage;
    [SerializeField] private AttackSO attackData;

    private CharacterCombat characterCombat;
    private float cooldown;

    private void Awake() {
        characterCombat = targetCharacter.GetComponent<CharacterCombat>();
        cooldownImage.fillAmount = 0;
    }

    private void Start() {
        if (attackData.IsChargeableAttack) {
            characterCombat.OnReleaseChargeAttack += EnableScript;
        }
        else {
            characterCombat.OnInitiateNormalAttack += EnableScript;
        }
    }

    private void Update() {
        float fillAmount = (attackData.CurrentCooldownTime - Time.time) / cooldown;
        fillAmount = Mathf.Clamp01(fillAmount);
        cooldownImage.fillAmount = fillAmount;
        if (cooldownImage.fillAmount == 0) {
            enabled = false;
            cooldownImage.gameObject.SetActive(false);
        }
    }

    private void EnableScript(object sender, CharacterCombat.CurrentAttackEventArgs e) {
        if (e.attackData == attackData) {
            cooldownImage.gameObject.SetActive(true);
            enabled = true;
            cooldownImage.fillAmount = 1;
            cooldown = characterCombat.CurrentAttackData.Cooldown;
        }
    }
}
