using UnityEngine;
using UnityEditor;

[CustomEditor(typeof(CombatSystemProjectile))]
public class CombatSystemProjectileEditor : Editor {

    #region --- Properties ---
    SerializedProperty initialVelocity;
    SerializedProperty minimumInitialVelocity;
    SerializedProperty hitboxTransform;
    SerializedProperty visualizeHitbox;
    SerializedProperty projectileType;
    SerializedProperty hitboxShape;
    SerializedProperty hitboxRadius;
    SerializedProperty hitboxSize;


    private void OnEnable() {
        initialVelocity = serializedObject.FindProperty(nameof(initialVelocity));
        minimumInitialVelocity = serializedObject.FindProperty(nameof(minimumInitialVelocity));
        hitboxTransform = serializedObject.FindProperty(nameof(hitboxTransform));
        visualizeHitbox = serializedObject.FindProperty(nameof(visualizeHitbox));
        projectileType = serializedObject.FindProperty(nameof(projectileType));
        hitboxShape = serializedObject.FindProperty(nameof(hitboxShape));
        hitboxRadius = serializedObject.FindProperty(nameof(hitboxRadius));
        hitboxSize = serializedObject.FindProperty(nameof(hitboxSize));
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
        EditorGUILayout.PropertyField(hitboxTransform);
        EditorGUILayout.PropertyField(visualizeHitbox);
        EditorGUILayout.PropertyField(projectileType);
        if (projectile.ProjectileType == ProjectileType.Normal) {
            EditorGUILayout.PropertyField(hitboxShape);
            if (projectile.HitboxShape == HitboxShape.Circle) {
                EditorGUILayout.PropertyField(hitboxRadius);
            }
            else if (projectile.HitboxShape == HitboxShape.Box) {
                EditorGUILayout.PropertyField(hitboxSize);
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
}
