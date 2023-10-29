using UnityEditor;

[CustomEditor(typeof(AttackSO))]
public class AttackSOEditor : Editor {

    SerializedProperty attackName;
    SerializedProperty icon;
    SerializedProperty attackAnimation;
    SerializedProperty whatIsDamageable;
    SerializedProperty attackPushesCharacter;
    SerializedProperty forceMode;
    SerializedProperty force;
    SerializedProperty delayForceTime;
    SerializedProperty useGravity;
    SerializedProperty dragCoeficient;
    SerializedProperty isChargeableAttack;
    SerializedProperty chargeTime;
    SerializedProperty holdChargeTime;
    SerializedProperty canMoveWhileCharging;
    SerializedProperty damageAmount;
    SerializedProperty cooldown;
    SerializedProperty throwsProjectile;
    SerializedProperty projectilePrefab;

    private void OnEnable() {
        attackName = serializedObject.FindProperty(nameof(attackName));
        icon = serializedObject.FindProperty(nameof(icon));
        attackAnimation = serializedObject.FindProperty(nameof(attackAnimation));
        whatIsDamageable = serializedObject.FindProperty(nameof(whatIsDamageable));
        attackPushesCharacter = serializedObject.FindProperty(nameof(attackPushesCharacter));
        forceMode = serializedObject.FindProperty(nameof(forceMode));
        force = serializedObject.FindProperty(nameof(force));
        delayForceTime = serializedObject.FindProperty(nameof(delayForceTime));
        useGravity = serializedObject.FindProperty(nameof(useGravity));
        dragCoeficient = serializedObject.FindProperty(nameof(dragCoeficient));
        isChargeableAttack = serializedObject.FindProperty(nameof(isChargeableAttack));
        chargeTime = serializedObject.FindProperty(nameof(chargeTime));
        holdChargeTime = serializedObject.FindProperty(nameof(holdChargeTime));
        canMoveWhileCharging = serializedObject.FindProperty(nameof(canMoveWhileCharging));
        damageAmount = serializedObject.FindProperty(nameof(damageAmount));
        cooldown = serializedObject.FindProperty(nameof(cooldown));
        throwsProjectile = serializedObject.FindProperty(nameof(throwsProjectile));
        projectilePrefab = serializedObject.FindProperty(nameof(projectilePrefab));
    }

    public override void OnInspectorGUI() {
        AttackSO attackSO = target as AttackSO;
        serializedObject.Update();
        EditorGUILayout.PropertyField(attackName);
        EditorGUILayout.PropertyField(icon);
        EditorGUILayout.PropertyField(attackAnimation);
        EditorGUILayout.PropertyField(whatIsDamageable);
        if (attackSO.AttackPushesCharacter) {
            EditorGUILayout.Space(10);
        }
        EditorGUILayout.PropertyField(attackPushesCharacter);
        if (attackSO.AttackPushesCharacter) {
            EditorGUILayout.PropertyField(forceMode);
            EditorGUILayout.PropertyField(force);
            EditorGUILayout.PropertyField(delayForceTime);
            EditorGUILayout.PropertyField(useGravity);
            EditorGUILayout.PropertyField(dragCoeficient);
            EditorGUILayout.Space(10);
        }
        if (attackSO.IsChargeableAttack && !attackSO.AttackPushesCharacter) {
            EditorGUILayout.Space(10);
        }
        EditorGUILayout.PropertyField(isChargeableAttack);
        if (attackSO.IsChargeableAttack) {
            EditorGUILayout.PropertyField(chargeTime);
            EditorGUILayout.PropertyField(holdChargeTime);
            EditorGUILayout.PropertyField(canMoveWhileCharging);
            EditorGUILayout.Space(10);
        }
        EditorGUILayout.PropertyField(damageAmount);
        EditorGUILayout.PropertyField(cooldown);
        if (attackSO.ThrowsProjectile) {
            EditorGUILayout.Space(10);
        }
        EditorGUILayout.PropertyField(throwsProjectile);
        if (attackSO.ThrowsProjectile) {
            EditorGUILayout.PropertyField(projectilePrefab);
            EditorGUILayout.Space(10);
        }
        serializedObject.ApplyModifiedProperties();
    }
}
