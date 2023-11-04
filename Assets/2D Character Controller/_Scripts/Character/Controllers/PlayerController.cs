using System.Collections.Generic;
using UnityEngine;

public class PlayerController : HumanoidController {

    [SerializeField] private List<AttackSO> meleeCombo_1 = new();
    [SerializeField] private AttackSO singleChargeAttack;

    private PlayerInput input;
    private bool canLedgeClimb = false;

    protected override void Awake() {
        base.Awake();
        input = FindObjectOfType<PlayerInput>();
    }

    protected override void Start() {
        base.Start();
        SubscribeToEvents();
    }

    protected override void OnDisable() {
        base.OnDisable();
        UnsubscribeFromEvents();
    }

    protected override void Update() {
        base.Update();
        Controller();

        if (Input.GetKey(KeyCode.Z)) {
            TryChargeAttack(singleChargeAttack);
        }
        else {
            ReleaseChargeAttack();
        }
    }

    private void SubscribeToEvents() {
        input.OnJumpPerformed += Input_OnJumpPerformed;
        input.OnDashPerformed += Input_OnDashPerformed;
        input.OnAttackPerformed += Input_OnAttackPerformed;
    }

    private void UnsubscribeFromEvents() {
        input.OnJumpPerformed -= Input_OnJumpPerformed;
        input.OnDashPerformed -= Input_OnDashPerformed;
        input.OnAttackPerformed -= Input_OnAttackPerformed;
    }

    private void Input_OnAttackPerformed(object sender, System.EventArgs e) {
        if (AttackCounter < meleeCombo_1.Count) {
            TryNormalAttack(meleeCombo_1[AttackCounter], true);
        }
    }

    private void Input_OnDashPerformed(object sender, System.EventArgs e) {
        TryDashing();
    }

    private void Input_OnJumpPerformed(object sender, System.EventArgs e) {
        if (IsLedgeGrabbing) {
            canLedgeClimb = true;
        }
        else {
            TryJumping();
        }
    }

    private void Controller() {
        if (!IsLedgeClimbing) {
            FlipCharacter(input.GetMovementInput().x);
        }
        ChangeHorizontalVelocity(input.GetMovementInput());
        TryFloorSlide(input.OnSlideKeyContinuous());
        WallSlide(input.GetMovementInput().x * transform.right.x > 0);
        DebugLedgeClimb();
        if (!IsLedgeClimbing && canLedgeClimb) {
            canLedgeClimb = false;
        }
    }

    [HideInInspector] private enum DebugLedgeAction { Normal, NoInput, WithInput }
    [HideInInspector] private DebugLedgeAction debugLedgeAction;
    [HideInInspector] private TMPro.TextMeshProUGUI ledgeModeText;
    private void DebugLedgeClimb() {

        if (ledgeModeText == null) {
            ledgeModeText = GameObject.Find("LedgeMode").GetComponent<TMPro.TextMeshProUGUI>();
        }

        switch (debugLedgeAction) {
            case DebugLedgeAction.Normal:
            LedgeGrab(/*input.GetMovementInput().x * transform.right.x > 0*/);
            LedgeClimb(canLedgeClimb);
            ledgeModeText.text = "Ledge grab + Ledge climb";
            break;
            case DebugLedgeAction.NoInput:
            LedgeClimb();
            ledgeModeText.text = "Ledge climb only - No input required";
            break;
            case DebugLedgeAction.WithInput:
            LedgeClimb(input.GetMovementInput().x * transform.right.x > 0);
            ledgeModeText.text = "Ledge climb only - Input required";
            break;
            default:
            break;
        }

        if (Input.GetKey(KeyCode.Y)) {
            debugLedgeAction = DebugLedgeAction.Normal;
        }
        if (Input.GetKey(KeyCode.U)) {
            debugLedgeAction = DebugLedgeAction.NoInput;
        }
        if (Input.GetKey(KeyCode.I)) {
            debugLedgeAction = DebugLedgeAction.WithInput;
        }
    }
}
