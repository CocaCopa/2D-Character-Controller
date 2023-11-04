using UnityEngine;
using UnityEditor;

[CustomEditor(typeof(CharacterLedgeGrab))]
public class CharacterLedgeGrabEditor : Editor {

    SerializedProperty spriteHolderTransform;
    SerializedProperty colliderTransform;
    SerializedProperty ledgeClimbEndTransform;
    SerializedProperty isAnimationStill;
    SerializedProperty ledgeClimbSpeed;
    SerializedProperty offsetSprite;
    SerializedProperty offsetColliderHeight;
    SerializedProperty interpolateOffsets;
    SerializedProperty interpolateIn;
    SerializedProperty interpolateOut;

    private void OnEnable() {
        spriteHolderTransform = serializedObject.FindProperty(nameof(spriteHolderTransform));
        colliderTransform = serializedObject.FindProperty(nameof(colliderTransform));
        ledgeClimbEndTransform = serializedObject.FindProperty(nameof(ledgeClimbEndTransform));
        isAnimationStill = serializedObject.FindProperty(nameof(isAnimationStill));
        ledgeClimbSpeed = serializedObject.FindProperty(nameof(ledgeClimbSpeed));
        offsetSprite = serializedObject.FindProperty(nameof(offsetSprite));
        offsetColliderHeight = serializedObject.FindProperty(nameof(offsetColliderHeight));
        interpolateOffsets = serializedObject.FindProperty(nameof(interpolateOffsets));
        interpolateIn = serializedObject.FindProperty(nameof(interpolateIn));
        interpolateOut = serializedObject.FindProperty(nameof(interpolateOut));
    }

    public override void OnInspectorGUI() {
        DisplayScriptReference();
        CharacterLedgeGrab ledgeGrab = target as CharacterLedgeGrab;
        serializedObject.Update();
        EditorGUILayout.PropertyField(spriteHolderTransform);
        EditorGUILayout.PropertyField(colliderTransform);
        EditorGUILayout.PropertyField(ledgeClimbEndTransform);
        if (ledgeGrab.IsAnimationStill) {
            EditorGUILayout.Space(10);
        }
        EditorGUILayout.PropertyField(isAnimationStill);
        if (ledgeGrab.IsAnimationStill) {
            EditorGUILayout.PropertyField(ledgeClimbSpeed);
            EditorGUILayout.Space(10);
        }
        EditorGUILayout.PropertyField(offsetSprite);
        EditorGUILayout.PropertyField(offsetColliderHeight);
        if (ledgeGrab.InterpolateOffsets) {
            EditorGUILayout.Space(10);
        }
        EditorGUILayout.PropertyField(interpolateOffsets);
        if (ledgeGrab.InterpolateOffsets) {
            EditorGUILayout.PropertyField(interpolateIn);
            EditorGUILayout.PropertyField(interpolateOut);
            EditorGUILayout.Space(10);
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
