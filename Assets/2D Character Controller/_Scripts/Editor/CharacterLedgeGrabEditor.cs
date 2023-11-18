using UnityEngine;
using UnityEditor;

[CustomEditor(typeof(CharacterLedgeGrab))]
public class CharacterLedgeGrabEditor : Editor {

    #region --- Serialized Properties ---
    SerializedProperty spriteHolderTransform;
    SerializedProperty colliderTransform;
    SerializedProperty ledgeClimbEndTransform;
    SerializedProperty isAnimationStill;
    SerializedProperty ledgeClimbSpeed;
    SerializedProperty offsetSprite;
    SerializedProperty offsetColliderHeight;

    private void OnEnable() {
        spriteHolderTransform = serializedObject.FindProperty(nameof(spriteHolderTransform));
        colliderTransform = serializedObject.FindProperty(nameof(colliderTransform));
        ledgeClimbEndTransform = serializedObject.FindProperty(nameof(ledgeClimbEndTransform));
        isAnimationStill = serializedObject.FindProperty(nameof(isAnimationStill));
        ledgeClimbSpeed = serializedObject.FindProperty(nameof(ledgeClimbSpeed));
        offsetSprite = serializedObject.FindProperty(nameof(offsetSprite));
        offsetColliderHeight = serializedObject.FindProperty(nameof(offsetColliderHeight));
    }
    #endregion

    private bool foldoutValue = false;

    public override void OnInspectorGUI() {
        DisplayScriptReference();
        CharacterLedgeGrab ledgeGrab = target as CharacterLedgeGrab;
        serializedObject.Update();
        foldoutValue = EditorGUILayout.BeginFoldoutHeaderGroup(foldoutValue, "--- References ---");
        if (foldoutValue) {
            EditorGUI.indentLevel++;
            EditorGUILayout.PropertyField(spriteHolderTransform);
            EditorGUILayout.PropertyField(colliderTransform);
            EditorGUILayout.PropertyField(ledgeClimbEndTransform);
            EditorGUI.indentLevel--;
        }
        EditorGUILayout.Space(5);
        EditorGUILayout.PropertyField(isAnimationStill);
        if (isAnimationStill.boolValue) {
            EditorGUILayout.PropertyField(ledgeClimbSpeed);
            EditorGUILayout.Space(10);
        }
        EditorGUILayout.PropertyField(offsetSprite);
        EditorGUILayout.PropertyField(offsetColliderHeight);
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
