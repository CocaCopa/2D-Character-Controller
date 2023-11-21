using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CustomControllerTemplate : HumanoidController {

    // This template is designed to expedite the development of your custom controller.
    // It includes essential functionalities, aiming to streamline your implementation process.
    // If you have any questions or require assistance, feel free to reach out to me.

    [Header("--- Attacks ---")]
    [SerializeField] private List<AttackSO> comboNormalAttacks;
    [SerializeField] private AttackSO singleNormalAttack;
    [SerializeField] private AttackSO singleChargeAttack;
    
    private EntityHealth entityHealth;

    protected override void Awake() {
        base.Awake();
        entityHealth = GetComponent<EntityHealth>();
        entityHealth.OnTakeDamage += EntityHealth_OnTakeDamage;
        entityHealth.OnEntityDeath += EntityHealth_OnEntityDeath;
    }

    protected override void Start() {
        base.Start();
    }

    protected override void Update() {
        base.Update();
    }

    protected override void OnDisable() {
        base.OnDisable();
        entityHealth.OnTakeDamage -= EntityHealth_OnTakeDamage;
        entityHealth.OnEntityDeath -= EntityHealth_OnEntityDeath;
    }

    // ----------------- //
    // vv Combat code vv //
    // ----------------- //
    private void EntityHealth_OnEntityDeath(object sender, System.EventArgs e) {
        // vv Your character dies. vv //
    }

    private void EntityHealth_OnTakeDamage(object sender, EntityHealth.OnTakeDamageEventArgs e) {
        // vv Your character takes damage. vv //
    }

    private void Respawn() {
        entityHealth.Alive();
        // vv Your 'respawn' code here vv //
    }

    // vv This is how attacks should get called vv //
    private void SingleNormalAttack() {
        if (Input.GetKeyDown(KeyCode.Z)) {
            characterCombat.PerformNormalAttack(singleChargeAttack, false);
        }
    }

    private void SingleChargeAttack() {
        if (Input.GetKey(KeyCode.X)) {
            characterCombat.PerformChargedAttack(singleChargeAttack);
        }
        else if (Input.GetKeyUp(KeyCode.X)) {
            characterCombat.TryReleaseChargedAttack();
        }
    }

    private void ComboNormalAttacks() {
        if (Input.GetKeyDown(KeyCode.C)) {
            if (characterCombat.AttackComboCounter < comboNormalAttacks.Count) {
                AttackSO currentAttack = comboNormalAttacks[characterCombat.AttackComboCounter];
                bool isPartOfCombo = true;
                characterCombat.PerformNormalAttack(currentAttack, isPartOfCombo);
            }
        }
    }
}
