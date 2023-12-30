using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class ComboAttackCooldownUI : MonoBehaviour {

    [SerializeField] private Transform targetCharacter;
    [SerializeField] private Image cooldownImage;
    [SerializeField] private List<AttackSO> comboData;

    private CharacterCombat characterCombat;
    private float cooldown;

    private void Awake() {
        characterCombat = targetCharacter.GetComponent<CharacterCombat>();
        cooldownImage.fillAmount = 0f;
    }

    private void Start() {
        characterCombat.OnInitiateNormalAttack += EnableScript;
        enabled = false;
        cooldownImage.gameObject.SetActive(false);
    }

    private void Update() {
        float fillAmount = (comboData[0].CurrentCooldownTime - Time.time) / cooldown;
        fillAmount = Mathf.Clamp01(fillAmount);
        cooldownImage.fillAmount = fillAmount;
        StartCoroutine(DisableScript());
    }

    private void EnableScript(object sender, CharacterCombat.CurrentAttackEventArgs e) {
        foreach (var attack in comboData) {
            if (e.attackData == attack) {
                cooldownImage.fillAmount = 1f;
                cooldown = characterCombat.CurrentAttackData.Cooldown;
                cooldownImage.gameObject.SetActive(true);
                enabled = true;
                break;
            }
        }
    }

    private IEnumerator DisableScript() {
        while (cooldownImage.fillAmount > 0f) {
            yield return new WaitForEndOfFrame();
            if (cooldownImage.fillAmount == 0f) {
                enabled = false;
                cooldownImage.gameObject.SetActive(false);
            }
        }
    }
}
