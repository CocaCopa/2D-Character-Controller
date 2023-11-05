using UnityEditor;

[CustomEditor(typeof(AttackSO))]
public class AttackSOEditor : Editor {

    #region --- Serialized Properties ---
    SerializedProperty attackName;
    SerializedProperty icon;
    SerializedProperty attackAnimation;
    SerializedProperty whatIsDamageable;
    SerializedProperty canMoveWhileAttacking;
    SerializedProperty attackPushesCharacter;
    SerializedProperty resetVelocity;
    SerializedProperty damageAmount;
    SerializedProperty cooldown;
    SerializedProperty forceMode;
    SerializedProperty force;
    SerializedProperty delayForceTime;
    SerializedProperty useGravity;
    SerializedProperty dragCoeficient;
    SerializedProperty isChargeableAttack;
    SerializedProperty chargeAnimation;
    SerializedProperty cooldownIfOvertime;
    SerializedProperty chargeTime;
    SerializedProperty holdChargeTime;
    SerializedProperty canMoveWhileCharging;
    SerializedProperty moveSpeedPercentage;
    SerializedProperty throwsProjectile;
    SerializedProperty projectilePrefab;

    private void OnEnable() {
        attackName = serializedObject.FindProperty(nameof(attackName));
        icon = serializedObject.FindProperty(nameof(icon));
        attackAnimation = serializedObject.FindProperty(nameof(attackAnimation));
        whatIsDamageable = serializedObject.FindProperty(nameof(whatIsDamageable));
        canMoveWhileAttacking = serializedObject.FindProperty(nameof(canMoveWhileAttacking));
        attackPushesCharacter = serializedObject.FindProperty(nameof(attackPushesCharacter));
        resetVelocity = serializedObject.FindProperty(nameof(resetVelocity));
        damageAmount = serializedObject.FindProperty(nameof(damageAmount));
        cooldown = serializedObject.FindProperty(nameof(cooldown));
        forceMode = serializedObject.FindProperty(nameof(forceMode));
        force = serializedObject.FindProperty(nameof(force));
        delayForceTime = serializedObject.FindProperty(nameof(delayForceTime));
        useGravity = serializedObject.FindProperty(nameof(useGravity));
        dragCoeficient = serializedObject.FindProperty(nameof(dragCoeficient));
        isChargeableAttack = serializedObject.FindProperty(nameof(isChargeableAttack));
        chargeAnimation = serializedObject.FindProperty(nameof(chargeAnimation));
        cooldownIfOvertime = serializedObject.FindProperty(nameof(cooldownIfOvertime));
        chargeTime = serializedObject.FindProperty(nameof(chargeTime));
        holdChargeTime = serializedObject.FindProperty(nameof(holdChargeTime));
        canMoveWhileCharging = serializedObject.FindProperty(nameof(canMoveWhileCharging));
        moveSpeedPercentage = serializedObject.FindProperty(nameof(moveSpeedPercentage));
        throwsProjectile = serializedObject.FindProperty(nameof(throwsProjectile));
        projectilePrefab = serializedObject.FindProperty(nameof(projectilePrefab));
    }
    #endregion

    public override void OnInspectorGUI() {
        DrawCustomEditor();
    }

    private void DrawCustomEditor() {
        AttackSO attackSO = target as AttackSO;
        serializedObject.Update();
        EditorGUILayout.PropertyField(attackName);
        EditorGUILayout.PropertyField(icon);
        EditorGUILayout.PropertyField(attackAnimation);
        EditorGUILayout.PropertyField(whatIsDamageable);
        EditorGUILayout.PropertyField(canMoveWhileAttacking);
        EditorGUILayout.PropertyField(resetVelocity);
        EditorGUILayout.PropertyField(damageAmount);
        EditorGUILayout.PropertyField(cooldown);
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
            EditorGUILayout.PropertyField(chargeAnimation);
            EditorGUILayout.PropertyField(cooldownIfOvertime);
            EditorGUILayout.PropertyField(chargeTime);
            EditorGUILayout.PropertyField(holdChargeTime);
            EditorGUILayout.PropertyField(canMoveWhileCharging);
            if (attackSO.CanMoveWhileCharging) {
                EditorGUILayout.PropertyField(moveSpeedPercentage);
            }
            EditorGUILayout.Space(10);
        }
        if (attackSO.ThrowsProjectile && !attackSO.IsChargeableAttack) {
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
