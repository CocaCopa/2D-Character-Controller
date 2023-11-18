using UnityEditor;
using UnityEngine;

[CustomEditor(typeof(EntityHealth))]
public class EntityHealthEditor : Editor {

    #region --- Serialized Properties ---
    SerializedProperty armourData;
    SerializedProperty maxHealthPoints;
    SerializedProperty currentHealthPoints;
    SerializedProperty canRegenHealth;
    SerializedProperty regenTriggerTime;
    SerializedProperty regenHealthPoints;

    private void OnEnable() {
        armourData = serializedObject.FindProperty(nameof(armourData));
        maxHealthPoints = serializedObject.FindProperty(nameof(maxHealthPoints));
        currentHealthPoints = serializedObject.FindProperty(nameof(currentHealthPoints));
        canRegenHealth = serializedObject.FindProperty(nameof(canRegenHealth));
        regenTriggerTime = serializedObject.FindProperty(nameof(regenTriggerTime));
        regenHealthPoints = serializedObject.FindProperty(nameof(regenHealthPoints));
    }
    #endregion

    public override void OnInspectorGUI() {
        DisplayScriptReference();
        DrawCustomInspector();
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
        serializedObject.Update();
        EditorGUILayout.PropertyField(armourData);
        EditorGUILayout.PropertyField(maxHealthPoints);
        EditorGUI.BeginDisabledGroup(true);
        EditorGUILayout.PropertyField(currentHealthPoints);
        EditorGUI.EndDisabledGroup();
        EditorGUILayout.PropertyField(canRegenHealth);
        if (canRegenHealth.boolValue) {
            EditorGUILayout.PropertyField(regenTriggerTime);
            EditorGUILayout.PropertyField(regenHealthPoints);
        }
        serializedObject.ApplyModifiedProperties();
    }
}
