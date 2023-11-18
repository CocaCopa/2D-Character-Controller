using UnityEditor;
using UnityEngine;

[CustomEditor(typeof(CharacterEnvironmentalQuery))]
public class CharacterEnvironmentalQueryEditor : Editor {

    #region --- Serialized Properties ---
    SerializedProperty debugMode;
    SerializedProperty debugColor;
    SerializedProperty ledgeGrabTransform;
    SerializedProperty headRayTransform;
    SerializedProperty chestRayTransform;
    SerializedProperty feetRayTransform;

    SerializedProperty excludeCharacter;
    SerializedProperty characterMask;
    SerializedProperty useSpecifiedLayer;
    SerializedProperty specifiedLayer;

    SerializedProperty groundCheckDistance;
    SerializedProperty wallDetectionDistance;
    SerializedProperty wallAboveDetectionDistance;
    SerializedProperty ledgeGrabRadius;

    SerializedProperty groundCheckSizeY;
    SerializedProperty wallAboveSizeY;
    SerializedProperty headCollisionSizeY;
    SerializedProperty chestCollisionSizeY;
    SerializedProperty feetCollisionSizeY;

    private void OnEnable() {
        debugMode = serializedObject.FindProperty(nameof(debugMode));
        debugColor = serializedObject.FindProperty(nameof(debugColor));

        ledgeGrabTransform = serializedObject.FindProperty(nameof(ledgeGrabTransform));
        headRayTransform = serializedObject.FindProperty(nameof(headRayTransform));
        chestRayTransform = serializedObject.FindProperty(nameof(chestRayTransform));
        feetRayTransform = serializedObject.FindProperty(nameof(feetRayTransform));

        excludeCharacter = serializedObject.FindProperty(nameof(excludeCharacter));
        characterMask = serializedObject.FindProperty(nameof(characterMask));
        useSpecifiedLayer = serializedObject.FindProperty(nameof(useSpecifiedLayer));
        specifiedLayer = serializedObject.FindProperty(nameof(specifiedLayer));
        
        groundCheckDistance = serializedObject.FindProperty(nameof(groundCheckDistance));
        wallDetectionDistance = serializedObject.FindProperty(nameof(wallDetectionDistance));
        wallAboveDetectionDistance = serializedObject.FindProperty(nameof(wallAboveDetectionDistance));
        ledgeGrabRadius = serializedObject.FindProperty(nameof(ledgeGrabRadius));

        groundCheckSizeY = serializedObject.FindProperty(nameof(groundCheckSizeY));
        wallAboveSizeY = serializedObject.FindProperty(nameof(wallAboveSizeY));
        headCollisionSizeY = serializedObject.FindProperty(nameof(headCollisionSizeY));
        chestCollisionSizeY = serializedObject.FindProperty(nameof(chestCollisionSizeY));
        feetCollisionSizeY = serializedObject.FindProperty(nameof(feetCollisionSizeY));
    }
    #endregion

    private bool changeLayerOptionsFlag = true;
    private bool debugFoldoutValue = false;
    private bool referencesFoldoutValue = false;
    private bool rayLayerMaskFoldoutValue = true;
    private bool rayDistancesFoldoutValue = true;
    private bool raySizesFoloutValue = true;

    public override void OnInspectorGUI() {
        DisplayScriptReference();
        serializedObject.Update();
        DrawDebug();
        EditorGUILayout.Space(5);
        DrawReferences();
        EditorGUILayout.Space(5);
        DrawRaycastLayerMask();
        ChangeLayerOptions();
        EditorGUILayout.Space(5);
        DrawRaycastDistances();
        EditorGUILayout.Space(5);
        DrawRaycastSizes();
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

    private void DrawDebug() {
        debugFoldoutValue = EditorGUILayout.BeginFoldoutHeaderGroup(debugFoldoutValue, "--- Debug ---");
        if (debugFoldoutValue) {
            EditorGUI.indentLevel++;
            EditorGUILayout.PropertyField(debugMode);
            EditorGUILayout.PropertyField(debugColor);
            EditorGUI.indentLevel--;
        }
        EditorGUILayout.EndFoldoutHeaderGroup();
    }

    private void DrawReferences() {
        referencesFoldoutValue = EditorGUILayout.BeginFoldoutHeaderGroup(referencesFoldoutValue, "--- Raycast Transform References ---");
        if (referencesFoldoutValue) {
            EditorGUI.indentLevel++;
            EditorGUILayout.PropertyField(ledgeGrabTransform);
            EditorGUILayout.PropertyField(headRayTransform);
            EditorGUILayout.PropertyField(chestRayTransform);
            EditorGUILayout.PropertyField(feetRayTransform);
            EditorGUI.indentLevel--;
        }
        EditorGUILayout.EndFoldoutHeaderGroup() ;
    }

    private void DrawRaycastLayerMask() {
        rayLayerMaskFoldoutValue = EditorGUILayout.BeginFoldoutHeaderGroup(rayLayerMaskFoldoutValue, "--- Raycast LayerMask ---");
        if (rayLayerMaskFoldoutValue) {
            EditorGUI.indentLevel++;
            EditorGUILayout.PropertyField(excludeCharacter);
            EditorGUILayout.PropertyField(useSpecifiedLayer);
            EditorGUILayout.Space(5);
            if (excludeCharacter.boolValue) {
                EditorGUILayout.PropertyField(characterMask);
            }
            if (useSpecifiedLayer.boolValue) {
                EditorGUILayout.PropertyField(specifiedLayer);
            }
            EditorGUI.indentLevel--;
        }
        EditorGUILayout.EndFoldoutHeaderGroup();
    }

    private void DrawRaycastDistances() {
        rayDistancesFoldoutValue = EditorGUILayout.BeginFoldoutHeaderGroup(rayDistancesFoldoutValue, "--- Raycast Distances ---");
        if (rayDistancesFoldoutValue) {
            EditorGUI.indentLevel++;
            EditorGUILayout.PropertyField(groundCheckDistance);
            EditorGUILayout.PropertyField(wallDetectionDistance);
            EditorGUILayout.PropertyField(wallAboveDetectionDistance);
            EditorGUILayout.PropertyField(ledgeGrabRadius);
            EditorGUI.indentLevel--;
        }
        EditorGUILayout.EndFoldoutHeaderGroup();
    }

    private void DrawRaycastSizes() {
        raySizesFoloutValue = EditorGUILayout.BeginFoldoutHeaderGroup(raySizesFoloutValue, "--- Raycast Sizes ---");
        if (raySizesFoloutValue) {
            EditorGUI.indentLevel++;
            EditorGUILayout.PropertyField(groundCheckSizeY);
            EditorGUILayout.PropertyField(wallAboveSizeY);
            EditorGUILayout.PropertyField(headCollisionSizeY);
            EditorGUILayout.PropertyField(chestCollisionSizeY);
            EditorGUILayout.PropertyField(feetCollisionSizeY);
            EditorGUI.indentLevel--;
        }
        EditorGUILayout.EndFoldoutHeaderGroup();
    }

    private void ChangeLayerOptions() {
        CharacterEnvironmentalQuery envQuery = target as CharacterEnvironmentalQuery;
        if (envQuery.ExcludeCharacter && changeLayerOptionsFlag) {
            if (envQuery.UseSpecifiedLayer) {

                changeLayerOptionsFlag = false;
                envQuery.ExcludeCharacter = false;
            }
        }
        else if (envQuery.UseSpecifiedLayer) {
            if (envQuery.ExcludeCharacter) {

                changeLayerOptionsFlag = true;
                envQuery.UseSpecifiedLayer = false;
            }
        }
    }
}
