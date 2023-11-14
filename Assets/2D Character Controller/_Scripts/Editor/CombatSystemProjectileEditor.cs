using UnityEngine;
using UnityEditor;

[CustomEditor(typeof(CombatSystemProjectile))]
public class CombatSystemProjectileEditor : Editor {

    #region --- Properties ---
    SerializedProperty initialVelocity;
    SerializedProperty minimumInitialVelocity;
    SerializedProperty useCustomBehaviour;
    SerializedProperty lookAtOnGoingDirection;
    SerializedProperty onCollisionEnter;
    SerializedProperty handleSameLayerCollision;
    SerializedProperty affectObject;
    SerializedProperty reactOnLayers;
    SerializedProperty onFirstContact;
    SerializedProperty constantMultiplier;
    SerializedProperty multiplierReductionRate;
    SerializedProperty speedMultiplier;
    SerializedProperty allowBounces;
    SerializedProperty onRicochetEnd;

    private void OnEnable() {
        initialVelocity = serializedObject.FindProperty(nameof(initialVelocity));
        minimumInitialVelocity = serializedObject.FindProperty(nameof(minimumInitialVelocity));
        useCustomBehaviour = serializedObject.FindProperty(nameof(useCustomBehaviour));
        lookAtOnGoingDirection = serializedObject.FindProperty(nameof(lookAtOnGoingDirection));
        onCollisionEnter = serializedObject.FindProperty(nameof(onCollisionEnter));
        handleSameLayerCollision = serializedObject.FindProperty(nameof(handleSameLayerCollision));
        affectObject = serializedObject.FindProperty(nameof(affectObject));
        reactOnLayers = serializedObject.FindProperty(nameof(reactOnLayers));
        onFirstContact = serializedObject.FindProperty(nameof(onFirstContact));
        constantMultiplier = serializedObject.FindProperty(nameof(constantMultiplier));
        multiplierReductionRate = serializedObject.FindProperty(nameof(multiplierReductionRate));
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
        EditorGUILayout.Space(10);
        EditorGUILayout.PropertyField(useCustomBehaviour);
        EditorGUILayout.Space(10);
        if (!projectile.UseCustomBehaviour) {
            EditorGUILayout.PropertyField(lookAtOnGoingDirection);
            EditorGUILayout.PropertyField(onCollisionEnter);
            EditorGUILayout.PropertyField(handleSameLayerCollision);
            if (projectile.HandleSameLayerCollision) {
                EnsureLayersAreSetUpCorrectly();
                EditorGUILayout.PropertyField(affectObject);
            }
            EditorGUILayout.Space(10);
            EditorGUILayout.PropertyField(reactOnLayers);
            EditorGUILayout.PropertyField(onFirstContact);
            if (projectile.OnFirstContact == ProjectileContact.Ricochet) {
                EditorGUILayout.PropertyField(constantMultiplier);
                if (!projectile.ConstantMultiplier) {
                    EditorGUILayout.PropertyField(multiplierReductionRate);
                }
                EditorGUILayout.PropertyField(speedMultiplier);
                EditorGUILayout.PropertyField(allowBounces);
                EditorGUILayout.PropertyField(onRicochetEnd);
                if (projectile.OnRicochetEnd == ProjectileContact.Ricochet) {
                    projectile.OnRicochetEnd = ProjectileContact.DefaultPhysics;
                    Debug.LogWarning("Unable to set 'Ricochet' to prevent a potential stack overflow.");
                }
            }
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

    private void EnsureLayersAreSetUpCorrectly() {
        if (!projectile.HandleSameLayerCollision) {
            return;
        }

        GameObject obj = projectile.gameObject;
        int objectLayer = obj.layer;

        LayerMask selectedMask = projectile.ReactOnLayers;

        int layerValue = 1 << objectLayer;

        if ((selectedMask.value & layerValue) != 0) {
            EditorGUILayout.HelpBox("To prevent collision issues, ensure the 'reactOnLayers' excludes " +
                "the layer assigned to this gameObject.", MessageType.Warning);
        }
    }
}
