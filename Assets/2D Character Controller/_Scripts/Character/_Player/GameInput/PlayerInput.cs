using System;
using UnityEngine;

public class PlayerInput : MonoBehaviour
{
    public event EventHandler OnJumpPerformed;
    public event EventHandler OnSlideKeyUp;
    public event EventHandler OnSlideKeyDown;
    public event EventHandler OnDashPerformed;
    public event EventHandler OnCombatNormalAttacks;
    public event EventHandler OnChargedAttackReleased;
    private InputActions inputActions;

    private void Awake() {

        inputActions = new InputActions();
        inputActions.Player.Enable();

        inputActions.Player.Jump.performed += Jump_performed;
        inputActions.Player.Slide.performed += Slide_performed;
        inputActions.Player.Dash.performed += Dash_performed;
        inputActions.Player.ComboNormalAttacks.performed += ComboNormalAttacks_performed;
    }

    private void OnDisable() {

        inputActions.Player.Jump.performed -= Jump_performed;
        inputActions.Player.Slide.performed -= Slide_performed;
        inputActions.Player.Dash.performed -= Dash_performed;
        inputActions.Player.ComboNormalAttacks.performed -= ComboNormalAttacks_performed;
    }

    private void Update() {
        if (inputActions.FindAction("Slide").WasReleasedThisFrame()) {
            OnSlideKeyUp?.Invoke(this, EventArgs.Empty);
        }
        if (inputActions.FindAction("ChargedAttack").WasReleasedThisFrame()) {
            OnChargedAttackReleased?.Invoke(this, EventArgs.Empty);
        }
    }

    private void Jump_performed(UnityEngine.InputSystem.InputAction.CallbackContext obj) {
        OnJumpPerformed?.Invoke(this, EventArgs.Empty);
    }

    private void Slide_performed(UnityEngine.InputSystem.InputAction.CallbackContext obj) {
        OnSlideKeyDown?.Invoke(this, EventArgs.Empty);
    }

    private void Dash_performed(UnityEngine.InputSystem.InputAction.CallbackContext obj) {
        OnDashPerformed?.Invoke(this, EventArgs.Empty);
    }

    private void ComboNormalAttacks_performed(UnityEngine.InputSystem.InputAction.CallbackContext obj) {
        OnCombatNormalAttacks?.Invoke(this, EventArgs.Empty);
    }
    public bool OnChargedAttackContinuous() => inputActions.Player.ChargedAttack.ReadValue<float>() > 0f;

    public Vector2 GetMovementInput() => inputActions.Player.Movement.ReadValue<Vector2>();

    public bool OnSlideKeyContinuous() => inputActions.Player.Slide.ReadValue<float>() > 0f;
}
