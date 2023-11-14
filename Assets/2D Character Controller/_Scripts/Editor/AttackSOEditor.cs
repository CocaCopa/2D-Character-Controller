using UnityEditor;
using UnityEngine;

[CustomEditor(typeof(AttackSO))]
public class AttackSOEditor : Editor {

    #region --- Serialized Properties ---
    SerializedProperty attackName;
    SerializedProperty attackIcon;
    SerializedProperty attackAnimation;
    SerializedProperty hitboxShape;
    SerializedProperty whatIsDamageable;
    SerializedProperty damageAmount;
    SerializedProperty cooldown;
    SerializedProperty disableCastOnWall;
    SerializedProperty wallCastDistance;
    SerializedProperty resetVelocity;
    SerializedProperty canChangeDirections;
    SerializedProperty adjustPositionOnAttackEnd;
    SerializedProperty canMoveWhileAttacking;
    SerializedProperty attackMoveSpeedPercentage;
    SerializedProperty attackPushesCharacter;
    SerializedProperty forceMode;
    SerializedProperty force;
    SerializedProperty delayForceTime;
    SerializedProperty useGravity;
    SerializedProperty dragCoeficient;
    SerializedProperty isChargeableAttack;
    SerializedProperty chargeAnimation;
    SerializedProperty chargeTime;
    SerializedProperty holdChargeTime;
    SerializedProperty chargeOverTime;
    SerializedProperty cooldownIfCanceled;
    SerializedProperty canMoveWhileCharging;
    SerializedProperty chargeMoveSpeedPercentage;
    SerializedProperty canMoveOnReleaseAttack;
    SerializedProperty throwsProjectile;
    SerializedProperty chooseRandomFromList;
    SerializedProperty projectilePrefab;
    SerializedProperty projectilePrefabs;
    SerializedProperty throwAtPercentage;
    SerializedProperty delayProjectileThrow;

    private void OnEnable() {
        attackName = serializedObject.FindProperty(nameof(attackName));
        attackIcon = serializedObject.FindProperty(nameof(attackIcon));
        attackAnimation = serializedObject.FindProperty(nameof(attackAnimation));
        hitboxShape = serializedObject.FindProperty(nameof(hitboxShape));
        whatIsDamageable = serializedObject.FindProperty(nameof(whatIsDamageable));
        damageAmount = serializedObject.FindProperty(nameof(damageAmount));
        cooldown = serializedObject.FindProperty(nameof(cooldown));
        disableCastOnWall = serializedObject.FindProperty(nameof(disableCastOnWall));
        wallCastDistance = serializedObject.FindProperty(nameof(wallCastDistance));
        resetVelocity = serializedObject.FindProperty(nameof(resetVelocity));
        canChangeDirections = serializedObject.FindProperty(nameof(canChangeDirections));
        adjustPositionOnAttackEnd = serializedObject.FindProperty(nameof(adjustPositionOnAttackEnd));
        canMoveWhileAttacking = serializedObject.FindProperty(nameof(canMoveWhileAttacking));
        attackMoveSpeedPercentage = serializedObject.FindProperty(nameof(attackMoveSpeedPercentage));
        attackPushesCharacter = serializedObject.FindProperty(nameof(attackPushesCharacter));
        forceMode = serializedObject.FindProperty(nameof(forceMode));
        force = serializedObject.FindProperty(nameof(force));
        delayForceTime = serializedObject.FindProperty(nameof(delayForceTime));
        useGravity = serializedObject.FindProperty(nameof(useGravity));
        dragCoeficient = serializedObject.FindProperty(nameof(dragCoeficient));
        isChargeableAttack = serializedObject.FindProperty(nameof(isChargeableAttack));
        chargeAnimation = serializedObject.FindProperty(nameof(chargeAnimation));
        chargeTime = serializedObject.FindProperty(nameof(chargeTime));
        holdChargeTime = serializedObject.FindProperty(nameof(holdChargeTime));
        chargeOverTime = serializedObject.FindProperty(nameof(chargeOverTime));
        cooldownIfCanceled = serializedObject.FindProperty(nameof(cooldownIfCanceled));
        canMoveWhileCharging = serializedObject.FindProperty(nameof(canMoveWhileCharging));
        chargeMoveSpeedPercentage = serializedObject.FindProperty(nameof(chargeMoveSpeedPercentage));
        canMoveOnReleaseAttack = serializedObject.FindProperty(nameof(canMoveOnReleaseAttack));
        throwsProjectile = serializedObject.FindProperty(nameof(throwsProjectile));
        chooseRandomFromList = serializedObject.FindProperty(nameof(chooseRandomFromList));
        projectilePrefab = serializedObject.FindProperty(nameof(projectilePrefab));
        projectilePrefabs = serializedObject.FindProperty(nameof(projectilePrefabs));
        throwAtPercentage = serializedObject.FindProperty(nameof(throwAtPercentage));
        delayProjectileThrow = serializedObject.FindProperty(nameof(delayProjectileThrow));
    }
    #endregion

    private AttackSO attackSO;

    public override void OnInspectorGUI() {
        GetTargetComponent();
        LogWarning();
        DrawCustomEditor();
    }

    private void GetTargetComponent() {
        if (attackSO == null)
            attackSO = target as AttackSO;
    }

    private void LogWarning() {
        if (attackSO.AttackAnimation != null && attackSO.AttackAnimation.isLooping) {
            Debug.LogWarning("The provided animation is configured with 'isLooping' set to 'true' " +
                "which might lead to potential visual issues when your character is attacking");
        }
    }

    private void DrawCustomEditor() {
        serializedObject.Update();
        DisplayScriptReference();
        CommonDetails();
        CommonStats();
        PushCharacter();
        ChargeableAttack();
        Projectile();
        serializedObject.ApplyModifiedProperties();
    }

    private void CommonDetails() {
        EditorGUILayout.PropertyField(attackName);
        EditorGUILayout.PropertyField(attackIcon);
        EditorGUILayout.Space(10);
    }

    private void CommonStats() {
        EditorGUILayout.PropertyField(attackAnimation);
        EditorGUILayout.PropertyField(hitboxShape);
        EditorGUILayout.PropertyField(whatIsDamageable);
        EditorGUILayout.PropertyField(damageAmount);
        EditorGUILayout.PropertyField(cooldown);
        EditorGUILayout.Space(10);
        EditorGUILayout.PropertyField(disableCastOnWall);
        if (attackSO.DisableCastOnWall) {
            EditorGUILayout.PropertyField(wallCastDistance);
        }
        EditorGUILayout.PropertyField(resetVelocity);
        EditorGUILayout.PropertyField(canChangeDirections);
        EditorGUILayout.PropertyField(adjustPositionOnAttackEnd);
        if (!attackSO.IsChargeableAttack) {
            EditorGUILayout.PropertyField(canMoveWhileAttacking);
            if (attackSO.CanMoveWhileAttacking) {
                EditorGUILayout.PropertyField(attackMoveSpeedPercentage);
            }
        }
        else {
            EditorGUILayout.PropertyField(canMoveWhileCharging);
            if (attackSO.CanMoveWhileCharging) {
                EditorGUILayout.PropertyField(chargeMoveSpeedPercentage);
                EditorGUILayout.PropertyField(canMoveOnReleaseAttack);
            }
        }
    }

    private void PushCharacter() {
        EditorGUILayout.Space(10);
        if (attackSO.CanMoveWhileCharging || attackSO.CanMoveWhileAttacking) {
            EditorGUI.BeginDisabledGroup(true);
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
        if (attackSO.CanMoveWhileCharging || attackSO.CanMoveWhileAttacking) {
            EditorGUI.EndDisabledGroup();
        }
        if (attackSO.IsChargeableAttack && !attackSO.AttackPushesCharacter) {
            EditorGUILayout.Space(10);
        }
    }

    private void ChargeableAttack() {
        EditorGUILayout.PropertyField(isChargeableAttack);
        if (attackSO.IsChargeableAttack) {
            EditorGUILayout.PropertyField(chargeAnimation);
            EditorGUILayout.PropertyField(chargeTime);
            EditorGUILayout.PropertyField(holdChargeTime);
            EditorGUILayout.PropertyField(chargeOverTime);
            EditorGUILayout.PropertyField(cooldownIfCanceled);
            EditorGUILayout.Space(10);
        }
    }

    private void Projectile() {
        if (attackSO.ThrowsProjectile && !attackSO.IsChargeableAttack) {
            EditorGUILayout.Space(10);
        }
        EditorGUILayout.PropertyField(throwsProjectile);
        if (attackSO.ThrowsProjectile) {
            EditorGUILayout.PropertyField(chooseRandomFromList);
            if (attackSO.ChooseRandomFromList) {
                EditorGUILayout.PropertyField(projectilePrefabs);
            }
            else {
                EditorGUILayout.PropertyField(projectilePrefab);
            }
            EditorGUILayout.PropertyField(throwAtPercentage);
            EditorGUILayout.PropertyField(delayProjectileThrow);
            EditorGUILayout.Space(10);
        }
    }

    private void DisplayScriptReference() {
        ScriptableObject scriptComponent = (ScriptableObject)target;
        SerializedObject m_serializedObject = new(scriptComponent);
        SerializedProperty scriptProperty = m_serializedObject.FindProperty("m_Script");
        EditorGUI.BeginDisabledGroup(true);
        EditorGUILayout.PropertyField(scriptProperty, true, new GUILayoutOption[0]);
        EditorGUI.EndDisabledGroup();
        serializedObject.ApplyModifiedProperties();
    }
}
