using UnityEditor;
using UnityEngine;

[CustomEditor(typeof(CharacterCombat))]
public class CharacterCombatEditor : Editor {

    SerializedProperty attackBufferTime;
    SerializedProperty attackCharged;
    SerializedProperty chargeTimer;
    SerializedProperty holdAttackTimer;

    private void OnEnable() {
        attackBufferTime = serializedObject.FindProperty(nameof(attackBufferTime));
        attackCharged = serializedObject.FindProperty(nameof(attackCharged));
        chargeTimer = serializedObject.FindProperty(nameof(chargeTimer));
        holdAttackTimer = serializedObject.FindProperty(nameof(holdAttackTimer));
    }

    public override void OnInspectorGUI() {
        DisplayScriptReference();
        DisplayPropertiesAsDisabled();
    }

    private void DisplayPropertiesAsDisabled() {
        serializedObject.Update();
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
