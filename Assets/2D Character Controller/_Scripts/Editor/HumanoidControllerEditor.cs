using System;
using System.Reflection;
using UnityEditor;
using UnityEngine;

[CustomEditor(typeof(HumanoidController), true)]
public class HumanoidControllerEditor : Editor {

    #region --- Serialized Properties ---
    SerializedProperty timeScale;
    SerializedProperty horizontalCollider;
    SerializedProperty verticalCollider;
    SerializedProperty smoothMovement;
    SerializedProperty coyoteTime;
    SerializedProperty numberOfAirJumps;
    SerializedProperty alwaysDecreaseJumpCounter;
    SerializedProperty dashCooldown;
    SerializedProperty minimumDashDistance;
    SerializedProperty minimumFloorSlideSpeed;
    SerializedProperty ledgeJumpThreshold;
    SerializedProperty maxLedgeGrabTime;

    private void OnEnable() {
        timeScale = serializedObject.FindProperty(nameof(timeScale));
        horizontalCollider = serializedObject.FindProperty(nameof(horizontalCollider));
        verticalCollider = serializedObject.FindProperty(nameof(verticalCollider));
        smoothMovement = serializedObject.FindProperty(nameof(smoothMovement));
        coyoteTime = serializedObject.FindProperty(nameof(coyoteTime));
        numberOfAirJumps = serializedObject.FindProperty(nameof(numberOfAirJumps));
        alwaysDecreaseJumpCounter = serializedObject.FindProperty(nameof(alwaysDecreaseJumpCounter));
        dashCooldown = serializedObject.FindProperty(nameof(dashCooldown));
        minimumDashDistance = serializedObject.FindProperty(nameof(minimumDashDistance));
        minimumFloorSlideSpeed = serializedObject.FindProperty(nameof(minimumFloorSlideSpeed));
        ledgeJumpThreshold = serializedObject.FindProperty(nameof(ledgeJumpThreshold));
        maxLedgeGrabTime = serializedObject.FindProperty(nameof(maxLedgeGrabTime));
    }
    #endregion

    private bool foldoutValue = true;

    public override void OnInspectorGUI() {
        DisplayScriptReference();
        DrawCustomInspector();
        EditorGUILayout.Space(1);
        DrawDefaultExcludingCustomFields();
    }

    private void DisplayScriptReference() {
        MonoBehaviour scriptComponent = target as MonoBehaviour;
        SerializedObject m_serializedObject = new(scriptComponent);
        SerializedProperty scriptProperty = m_serializedObject.FindProperty("m_Script");
        EditorGUI.BeginDisabledGroup(true);
        EditorGUILayout.PropertyField(scriptProperty, true, new GUILayoutOption[0]);
        EditorGUI.EndDisabledGroup();
        serializedObject.ApplyModifiedProperties();
    }

    private void DrawCustomInspector() {
        HumanoidController controller = target as HumanoidController;
        foldoutValue = EditorGUILayout.BeginFoldoutHeaderGroup(foldoutValue, "--- Controller Options ---");
        if (foldoutValue) {
            EditorGUI.indentLevel++;
            serializedObject.Update();
            EditorGUILayout.PropertyField(timeScale);
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
            serializedObject.ApplyModifiedProperties();
            EditorGUI.indentLevel--;
        }
        EditorGUILayout.EndFoldoutHeaderGroup();
    }

    private void DrawDefaultExcludingCustomFields() {
        string[] excludeFields = new string[13];
        excludeFields[0] = "m_Script";
        excludeFields[1] = nameof(timeScale);
        excludeFields[2] = nameof(horizontalCollider);
        excludeFields[3] = nameof(verticalCollider);
        excludeFields[4] = nameof(smoothMovement);
        excludeFields[5] = nameof(coyoteTime);
        excludeFields[6] = nameof(numberOfAirJumps);
        excludeFields[7] = nameof(alwaysDecreaseJumpCounter);
        excludeFields[8] = nameof(dashCooldown);
        excludeFields[9] = nameof(minimumDashDistance);
        excludeFields[10] = nameof(minimumFloorSlideSpeed);
        excludeFields[11] = nameof(ledgeJumpThreshold);
        excludeFields[12] = nameof(maxLedgeGrabTime);
        DrawPropertiesExcluding(serializedObject, excludeFields);
        serializedObject.ApplyModifiedProperties();
    }
}
