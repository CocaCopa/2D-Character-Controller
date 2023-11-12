using UnityEngine;
using UnityEditor;

[CustomEditor(typeof(CombatSystemProjectile))]
public class CombatSystemProjectileEditor : Editor {

    #region --- Properties ---
    SerializedProperty initialVelocity;
    SerializedProperty minimumInitialVelocity;
    SerializedProperty lookAtOnGoingDirection;
    SerializedProperty OnCollisionEnter;
    SerializedProperty reactOnLayers;
    SerializedProperty onFirstContact;
    SerializedProperty constantMultiplier;
    SerializedProperty speedMultiplier;
    SerializedProperty allowBounces;
    SerializedProperty onRicochetEnd;

    private void OnEnable() {
        initialVelocity = serializedObject.FindProperty(nameof(initialVelocity));
        minimumInitialVelocity = serializedObject.FindProperty(nameof(minimumInitialVelocity));
        lookAtOnGoingDirection = serializedObject.FindProperty(nameof(lookAtOnGoingDirection));
        OnCollisionEnter = serializedObject.FindProperty(nameof(OnCollisionEnter));
        reactOnLayers = serializedObject.FindProperty(nameof(reactOnLayers));
        onFirstContact = serializedObject.FindProperty(nameof(onFirstContact));
        constantMultiplier = serializedObject.FindProperty(nameof(constantMultiplier));
        speedMultiplier = serializedObject.FindProperty(nameof(speedMultiplier));
        allowBounces = serializedObject.FindProperty(nameof(allowBounces));
        onRicochetEnd = serializedObject.FindProperty(nameof(onRicochetEnd));
    }
    #endregion

    private CombatSystemProjectile projectile;

    public override void OnInspectorGUI() {
        GetTargetReference();
        DisplayScriptReference();
        DrawCustomEditor();
    }

    private void GetTargetReference() {
        if (projectile == null) {
            projectile = target as CombatSystemProjectile;
        }
    }

    private void DrawCustomEditor() {
        serializedObject.Update();
        EditorGUILayout.PropertyField(initialVelocity);
        EditorGUILayout.PropertyField(minimumInitialVelocity);
        EditorGUILayout.PropertyField(lookAtOnGoingDirection);
        EditorGUILayout.PropertyField(OnCollisionEnter);
        EditorGUILayout.PropertyField(reactOnLayers);
        EditorGUILayout.PropertyField(onFirstContact);
        if (projectile.OnFirstContact == ProjectileContact.Ricochet) {
            EditorGUILayout.PropertyField(constantMultiplier);
            EditorGUILayout.PropertyField(speedMultiplier);
            EditorGUILayout.PropertyField(allowBounces);
            EditorGUILayout.PropertyField(onRicochetEnd);
        }
        serializedObject.ApplyModifiedProperties();
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
