using UnityEngine;
using UnityEditor;

[CustomEditor(typeof(ArrowProjectile))]
public class ArrowProjectileEditor : Editor {

    #region --- Serialized Properties ---
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

    private ArrowProjectile arrow;

    public override void OnInspectorGUI() {
        GetTargetReference();
        DisplayScriptReference();
        DrawCustomEditor();
    }

    private void GetTargetReference() {
        if (arrow == null) {
            arrow = target as ArrowProjectile;
        }
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

    private void DrawCustomEditor() {
        serializedObject.Update();
        EditorGUILayout.PropertyField(lookAtOnGoingDirection);
        EditorGUILayout.PropertyField(onCollisionEnter);
        EditorGUILayout.PropertyField(handleSameLayerCollision);
        if (arrow.HandleSameLayerCollision) {
            EnsureLayersAreSetUpCorrectly();
            EditorGUILayout.PropertyField(affectObject);
        }
        EditorGUILayout.Space(10);
        EditorGUILayout.PropertyField(reactOnLayers);
        EditorGUILayout.PropertyField(onFirstContact);
        if (arrow.OnFirstContact == ProjectileContact.Ricochet) {
            EditorGUILayout.PropertyField(constantMultiplier);
            if (!arrow.ConstantMultiplier) {
                EditorGUILayout.PropertyField(multiplierReductionRate);
            }
            EditorGUILayout.PropertyField(speedMultiplier);
            EditorGUILayout.PropertyField(allowBounces);
            EditorGUILayout.PropertyField(onRicochetEnd);
            if (arrow.OnRicochetEnd == ProjectileContact.Ricochet) {
                arrow.OnRicochetEnd = ProjectileContact.DefaultPhysics;
                Debug.LogWarning("Unable to set 'Ricochet' to prevent a potential stack overflow.");
            }
        }
        serializedObject.ApplyModifiedProperties();
    }

    private void EnsureLayersAreSetUpCorrectly() {
        if (!arrow.HandleSameLayerCollision) {
            return;
        }

        GameObject obj = arrow.gameObject;
        int objectLayer = obj.layer;

        LayerMask selectedMask = arrow.ReactOnLayers;

        int layerValue = 1 << objectLayer;

        if ((selectedMask.value & layerValue) != 0) {
            EditorGUILayout.HelpBox("To prevent collision issues, ensure the 'reactOnLayers' excludes " +
                "the layer assigned to this gameObject.", MessageType.Warning);
        }
    }
}
