using UnityEngine;
using UnityEngine.UI;

public class BowAttackUI : MonoBehaviour {

    [SerializeField] private AttackSO bowAttack;
    [SerializeField] private Image chargeImage;
    [SerializeField] private Gradient chargeBarColor;

    private CharacterCombat characterCombat;

    private void Awake() {
        characterCombat = transform.root.GetComponent<CharacterCombat>();
        characterCombat.OnInitiateChargeAttack += Combat_OnInitiateChargeAttack;
        chargeImage.fillAmount = 0;
        enabled = false;
    }

    private void Update() {
        ImageGameObjectActive();
        BarFillAmount();
        BarGradientColor();
    }

    private void ImageGameObjectActive() {
        if (characterCombat.CurrentAttackData != bowAttack && chargeImage.gameObject.activeInHierarchy) {
            chargeImage.fillAmount = 0;
            chargeImage.gameObject.SetActive(false);
            enabled = false;
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

    private void Combat_OnInitiateChargeAttack(object sender, CharacterCombat.CurrentAttackEventArgs e) {
        if (e.attackData == bowAttack) {
            chargeImage.gameObject.SetActive(true);
            enabled = true;
        }
    }
}
