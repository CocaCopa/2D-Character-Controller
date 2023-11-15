using UnityEditor;
using UnityEngine;

[CustomEditor(typeof(CharacterCombat))]
public class CharacterCombatEditor : Editor {

    #region --- Serialized Properties ---
    SerializedProperty visualizeAttackHitbox;
    SerializedProperty gizmosColor;
    SerializedProperty attackHitboxTransform;
    SerializedProperty attackBufferTime;
    SerializedProperty attackCharged;
    SerializedProperty chargeTimer;
    SerializedProperty holdAttackTimer;

    private void OnEnable() {
        visualizeAttackHitbox = serializedObject.FindProperty(nameof(visualizeAttackHitbox));
        gizmosColor = serializedObject.FindProperty(nameof(gizmosColor));
        attackHitboxTransform = serializedObject.FindProperty(nameof(attackHitboxTransform));
        attackBufferTime = serializedObject.FindProperty(nameof(attackBufferTime));
        attackCharged = serializedObject.FindProperty(nameof(attackCharged));
        chargeTimer = serializedObject.FindProperty(nameof(chargeTimer));
        holdAttackTimer = serializedObject.FindProperty(nameof(holdAttackTimer));
    }
    #endregion

    private CharacterCombat combat;

    public override void OnInspectorGUI() {
        GetTargetReference();
        DisplayScriptReference();
        DisplayPropertiesAsDisabled();
    }

    private void GetTargetReference() {
        if (combat == null) {
            combat = target as CharacterCombat;
        }
    }

    private void DisplayPropertiesAsDisabled() {
        serializedObject.Update();
        EditorGUILayout.PropertyField(visualizeAttackHitbox);
        if (combat.VisualizeAttackHitbox) {
            EditorGUILayout.PropertyField(gizmosColor);
            EditorGUILayout.Space(10);
        }
        EditorGUILayout.PropertyField(attackHitboxTransform);
        EditorGUILayout.PropertyField(attackBufferTime);
        serializedObject.ApplyModifiedProperties();
        EditorGUILayout.Space(10);
        EditorGUI.BeginDisabledGroup(true);
        EditorGUILayout.PropertyField(attackCharged);
        EditorGUILayout.PropertyField(chargeTimer);
        EditorGUILayout.PropertyField(holdAttackTimer);
        EditorGUI.EndDisabledGroup();
    }

    private void DisplayScriptReference() {
        MonoBehaviour scriptComponent = (MonoBehaviour)target;
        SerializedObject m_serializedObject = new(scriptComponent);
        SerializedProperty scriptProperty = m_serializedObject.FindProperty("m_Script");
        EditorGUI.BeginDisabledGroup(true);
        EditorGUILayout.PropertyField(scriptProperty, true, new GUILayoutOption[0]);
        EditorGUI.EndDisabledGroup();
        serializedObject.ApplyModifiedProperties();
    }
}
