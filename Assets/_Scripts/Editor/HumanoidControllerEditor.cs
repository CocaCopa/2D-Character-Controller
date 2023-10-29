using UnityEditor;

[CustomEditor(typeof(PlayerController))]
public class HumanoidControllerEditor : Editor {

    #region --- Serialized Properties ---
    SerializedProperty horizontalCollider;
    SerializedProperty verticalCollider;
    SerializedProperty smoothMovement;
    SerializedProperty coyoteTime;
    SerializedProperty numberOfAirJumps;
    SerializedProperty alwaysDecreaseJumpCounter;
    SerializedProperty meleeAttackCooldown;
    SerializedProperty meleeAttackBufferTime;
    SerializedProperty dashCooldown;
    SerializedProperty minimumDashDistance;
    SerializedProperty minimumFloorSlideSpeed;
    SerializedProperty ledgeJumpThreshold;
    SerializedProperty maxLedgeGrabTime;

    private void OnEnable() {
        horizontalCollider = serializedObject.FindProperty(nameof(horizontalCollider));
        verticalCollider = serializedObject.FindProperty(nameof(verticalCollider));
        smoothMovement = serializedObject.FindProperty(nameof(smoothMovement));
        coyoteTime = serializedObject.FindProperty(nameof(coyoteTime));
        numberOfAirJumps = serializedObject.FindProperty(nameof(numberOfAirJumps));
        alwaysDecreaseJumpCounter = serializedObject.FindProperty(nameof(alwaysDecreaseJumpCounter));
        meleeAttackCooldown = serializedObject.FindProperty(nameof(meleeAttackCooldown));
        meleeAttackBufferTime = serializedObject.FindProperty(nameof(meleeAttackBufferTime));
        dashCooldown = serializedObject.FindProperty(nameof(dashCooldown));
        minimumDashDistance = serializedObject.FindProperty(nameof(minimumDashDistance));
        minimumFloorSlideSpeed = serializedObject.FindProperty(nameof(minimumFloorSlideSpeed));
        ledgeJumpThreshold = serializedObject.FindProperty(nameof(ledgeJumpThreshold));
        maxLedgeGrabTime = serializedObject.FindProperty(nameof(maxLedgeGrabTime));
    }
    #endregion

    public override void OnInspectorGUI() {

        PlayerController controller = (PlayerController)target;

        serializedObject.Update();
        EditorGUILayout.PropertyField(horizontalCollider);
        EditorGUILayout.PropertyField(verticalCollider);
        EditorGUILayout.PropertyField(smoothMovement);
        EditorGUILayout.PropertyField(coyoteTime);
        EditorGUILayout.PropertyField(numberOfAirJumps);
        EditorGUILayout.PropertyField(alwaysDecreaseJumpCounter);

        if (controller.TryGetComponent<CharacterDash>(out _)) {
            EditorGUILayout.PropertyField(dashCooldown);
            EditorGUILayout.PropertyField(minimumDashDistance);
        }

        if (controller.TryGetComponent<CharacterSlide>(out _)) {
            EditorGUILayout.PropertyField(minimumFloorSlideSpeed);
        }

        if (controller.TryGetComponent<CharacterLedgeGrab>(out _)) {
            EditorGUILayout.PropertyField(ledgeJumpThreshold);
            EditorGUILayout.PropertyField(maxLedgeGrabTime);
        }

        if (controller.TryGetComponent<CharacterCombat>(out _)) {
            EditorGUILayout.PropertyField(meleeAttackCooldown);
            EditorGUILayout.PropertyField(meleeAttackBufferTime);
        }
        serializedObject.ApplyModifiedProperties();
    }
}
