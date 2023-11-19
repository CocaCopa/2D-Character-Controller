using UnityEngine;
using UnityEngine.UI;

public class BowAttackUI : MonoBehaviour {

    [SerializeField] private AttackSO bowAttack;
    [SerializeField] private Image chargeImage;
    [SerializeField] private Gradient chargeBarColor;

    private CharacterCombat characterCombat;

    private void Awake() {
        characterCombat = transform.root.GetComponent<CharacterCombat>();
    }

    private void Update() {
        ImageGameObjectActive();
        BarFillAmount();
        BarGradientColor();
    }

    private void ImageGameObjectActive() {
        if (characterCombat.CurrentAttackData != bowAttack && chargeImage.gameObject.activeInHierarchy) {
            chargeImage.gameObject.SetActive(false);
        }
        else if (characterCombat.CurrentAttackData == bowAttack && !chargeImage.gameObject.activeInHierarchy) {
            chargeImage.gameObject.SetActive(true);
        }
    }

    private void BarFillAmount() {
        if (characterCombat.CurrentAttackData == bowAttack) {
            chargeImage.fillAmount = 1 - (characterCombat.ChargeTimer / bowAttack.ChargeTime);
        }
    }

    private void BarGradientColor() {
        chargeImage.color = chargeBarColor.Evaluate(chargeImage.fillAmount);
    }
}
