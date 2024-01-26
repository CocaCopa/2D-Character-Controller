using UnityEditor;
using UnityEngine;

[CustomEditor(typeof(AttackSO))]
public class AttackSOEditor : Editor {

    #region --- Serialized Properties ---
    SerializedProperty attackName;
    SerializedProperty attackIcon;

    SerializedProperty attackAnimation;
    SerializedProperty hitboxShape;
    SerializedProperty damageableLayers;
    SerializedProperty damageAmount;
    SerializedProperty scalableDamage;
    SerializedProperty minimumDamage;
    SerializedProperty cooldown;

    SerializedProperty disableCastOnWall;
    SerializedProperty wallCastDistance;
    SerializedProperty resetVelocity;
    SerializedProperty canChangeDirections;
    SerializedProperty adjustPositionOnAttackEnd;
    SerializedProperty canMoveWhileAttacking;
    SerializedProperty attackMoveSpeedPercentage;

    SerializedProperty attackPushesCharacter;
    SerializedProperty attackPushMode;
    SerializedProperty forceMode;
    SerializedProperty force;
    SerializedProperty delayForceTime;
    SerializedProperty useGravity;
    SerializedProperty dragCoefficient;
    SerializedProperty m_ForceMode;
    SerializedProperty m_Force;
    SerializedProperty m_DelayForceTime;
    SerializedProperty m_UseGravity;
    SerializedProperty m_DragCoeficient;

    SerializedProperty isChargeableAttack;
    SerializedProperty initiateChargeAnimation;
    SerializedProperty chargeAnimation;
    SerializedProperty chargeTime;
    SerializedProperty holdChargeTime;
    SerializedProperty chargeOverTime;
    SerializedProperty cooldownIfCanceled;
    SerializedProperty canMoveWhileCharging;
    SerializedProperty chargeMoveSpeedPercentage;
    SerializedProperty canMoveOnReleaseAttack;

    SerializedProperty throwsProjectile;
    SerializedProperty scalableProjectileDamage;
    SerializedProperty scalableProjectileVelocity;
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
        damageableLayers = serializedObject.FindProperty(nameof(damageableLayers));
        damageAmount = serializedObject.FindProperty(nameof(damageAmount));
        scalableDamage = serializedObject.FindProperty(nameof(scalableDamage));
        minimumDamage = serializedObject.FindProperty(nameof(minimumDamage));
        cooldown = serializedObject.FindProperty(nameof(cooldown));

        disableCastOnWall = serializedObject.FindProperty(nameof(disableCastOnWall));
        wallCastDistance = serializedObject.FindProperty(nameof(wallCastDistance));
        resetVelocity = serializedObject.FindProperty(nameof(resetVelocity));
        canChangeDirections = serializedObject.FindProperty(nameof(canChangeDirections));
        adjustPositionOnAttackEnd = serializedObject.FindProperty(nameof(adjustPositionOnAttackEnd));
        canMoveWhileAttacking = serializedObject.FindProperty(nameof(canMoveWhileAttacking));
        attackMoveSpeedPercentage = serializedObject.FindProperty(nameof(attackMoveSpeedPercentage));

        attackPushesCharacter = serializedObject.FindProperty(nameof(attackPushesCharacter));
        attackPushMode = serializedObject.FindProperty(nameof(attackPushMode));
        forceMode = serializedObject.FindProperty(nameof(forceMode));
        force = serializedObject.FindProperty(nameof(force));
        delayForceTime = serializedObject.FindProperty(nameof(delayForceTime));
        useGravity = serializedObject.FindProperty(nameof(useGravity));
        dragCoefficient = serializedObject.FindProperty(nameof(dragCoefficient));
        m_ForceMode = serializedObject.FindProperty(nameof(m_ForceMode));
        m_Force = serializedObject.FindProperty(nameof(m_Force));
        m_DelayForceTime = serializedObject.FindProperty(nameof(m_DelayForceTime));
        m_UseGravity = serializedObject.FindProperty(nameof(m_UseGravity));
        m_DragCoeficient = serializedObject.FindProperty(nameof(m_DragCoeficient));

        isChargeableAttack = serializedObject.FindProperty(nameof(isChargeableAttack));
        initiateChargeAnimation = serializedObject.FindProperty(nameof(initiateChargeAnimation));
        chargeAnimation = serializedObject.FindProperty(nameof(chargeAnimation));
        chargeTime = serializedObject.FindProperty(nameof(chargeTime));
        holdChargeTime = serializedObject.FindProperty(nameof(holdChargeTime));
        chargeOverTime = serializedObject.FindProperty(nameof(chargeOverTime));
        cooldownIfCanceled = serializedObject.FindProperty(nameof(cooldownIfCanceled));
        canMoveWhileCharging = serializedObject.FindProperty(nameof(canMoveWhileCharging));
        chargeMoveSpeedPercentage = serializedObject.FindProperty(nameof(chargeMoveSpeedPercentage));
        canMoveOnReleaseAttack = serializedObject.FindProperty(nameof(canMoveOnReleaseAttack));

        throwsProjectile = serializedObject.FindProperty(nameof(throwsProjectile));
        scalableProjectileDamage = serializedObject.FindProperty(nameof(scalableProjectileDamage));
        scalableProjectileVelocity = serializedObject.FindProperty(nameof(scalableProjectileVelocity));
        chooseRandomFromList = serializedObject.FindProperty(nameof(chooseRandomFromList));
        projectilePrefab = serializedObject.FindProperty(nameof(projectilePrefab));
        projectilePrefabs = serializedObject.FindProperty(nameof(projectilePrefabs));
        throwAtPercentage = serializedObject.FindProperty(nameof(throwAtPercentage));
        delayProjectileThrow = serializedObject.FindProperty(nameof(delayProjectileThrow));
    }
    #endregion

    private bool foldoutValue = false;
    private bool foldoutValue_2 = false;

    public override void OnInspectorGUI() {
        LogWarning();
        DrawCustomEditor();
    }

    private void LogWarning() {
        AttackSO attackSO = target as AttackSO;
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
        EditorGUILayout.PropertyField(damageableLayers);
        EditorGUILayout.PropertyField(damageAmount);
        if (isChargeableAttack.boolValue) {
            EditorGUILayout.PropertyField(scalableDamage);
            if (scalableDamage.boolValue) {
                EditorGUILayout.PropertyField(minimumDamage);
            }
        }
        EditorGUILayout.PropertyField(cooldown);
        EditorGUILayout.Space(10);
        EditorGUILayout.PropertyField(disableCastOnWall);
        if (disableCastOnWall.boolValue) {
            EditorGUILayout.PropertyField(wallCastDistance);
        }
        EditorGUILayout.PropertyField(resetVelocity);
        EditorGUILayout.PropertyField(canChangeDirections);
        EditorGUILayout.PropertyField(adjustPositionOnAttackEnd);
        if (!isChargeableAttack.boolValue) {
            EditorGUILayout.PropertyField(canMoveWhileAttacking);
            if (canMoveWhileAttacking.boolValue) {
                EditorGUILayout.PropertyField(attackMoveSpeedPercentage);
                attackPushesCharacter.boolValue = false;
            }
        }
        else {
            EditorGUILayout.PropertyField(canMoveWhileCharging);
            if (canMoveWhileCharging.boolValue) {
                EditorGUILayout.PropertyField(chargeMoveSpeedPercentage);
                EditorGUILayout.PropertyField(canMoveOnReleaseAttack);
                attackPushesCharacter.boolValue = false;
            }
        }
    }

    private void PushCharacter() {
        EditorGUILayout.Space(10);
        if (canMoveWhileCharging.boolValue || canMoveWhileAttacking.boolValue) {
            EditorGUI.BeginDisabledGroup(true);
        }
        EditorGUILayout.PropertyField(attackPushesCharacter);
        if (attackPushesCharacter.boolValue) {
            if (isChargeableAttack.boolValue) {
                EditorGUILayout.PropertyField(attackPushMode);
            }
            if (!isChargeableAttack.boolValue || attackPushMode.enumValueIndex == 0 || attackPushMode.enumValueIndex == 1) {
                EditorGUILayout.PropertyField(forceMode);
                EditorGUILayout.PropertyField(force);
                EditorGUILayout.PropertyField(delayForceTime);
                EditorGUILayout.PropertyField(useGravity);
                EditorGUILayout.PropertyField(dragCoefficient);
                EditorGUILayout.Space(10);

            }
            else if (attackPushMode.enumValueIndex == 2) {
                foldoutValue = EditorGUILayout.BeginFoldoutHeaderGroup(foldoutValue, "Initiate Forces");
                if (foldoutValue) {
                    EditorGUI.indentLevel++;
                    EditorGUILayout.PropertyField(forceMode);
                    EditorGUILayout.PropertyField(force);
                    EditorGUILayout.PropertyField(delayForceTime);
                    EditorGUILayout.PropertyField(useGravity);
                    EditorGUILayout.PropertyField(dragCoefficient);
                    EditorGUILayout.Space(10);
                    EditorGUI.indentLevel--;
                }
                EditorGUILayout.EndFoldoutHeaderGroup();

                foldoutValue_2 = EditorGUILayout.BeginFoldoutHeaderGroup(foldoutValue_2, "Release Forces");
                if (foldoutValue_2) {
                    EditorGUI.indentLevel++;
                    EditorGUILayout.PropertyField(m_ForceMode);
                    EditorGUILayout.PropertyField(m_Force);
                    EditorGUILayout.PropertyField(m_DelayForceTime);
                    EditorGUILayout.PropertyField(m_UseGravity);
                    EditorGUILayout.PropertyField(m_DragCoeficient);
                    EditorGUI.indentLevel--;
                }
                EditorGUILayout.EndFoldoutHeaderGroup();
                EditorGUILayout.Space(10);
            }
        }

        if (canMoveWhileCharging.boolValue || canMoveWhileAttacking.boolValue) {
            EditorGUI.EndDisabledGroup();
        }
        if (isChargeableAttack.boolValue && !attackPushesCharacter.boolValue) {
            EditorGUILayout.Space(10);
        }
    }
    
    private void ChargeableAttack() {
        EditorGUILayout.PropertyField(isChargeableAttack);
        if (isChargeableAttack.boolValue) {
            EditorGUILayout.PropertyField(initiateChargeAnimation);
            EditorGUILayout.PropertyField(chargeAnimation);
            EditorGUILayout.PropertyField(chargeTime);
            EditorGUILayout.PropertyField(holdChargeTime);
            EditorGUILayout.PropertyField(chargeOverTime);
            EditorGUILayout.PropertyField(cooldownIfCanceled);
            EditorGUILayout.Space(10);
        }
    }

    private void Projectile() {
        if (throwsProjectile.boolValue && !isChargeableAttack.boolValue) {
            EditorGUILayout.Space(10);
        }
        EditorGUILayout.PropertyField(throwsProjectile);
        if (throwsProjectile.boolValue) {
            if (isChargeableAttack.boolValue) {
                EditorGUILayout.PropertyField(scalableProjectileDamage);
                EditorGUILayout.PropertyField(scalableProjectileVelocity);
            }
            EditorGUILayout.PropertyField(chooseRandomFromList);
            if (chooseRandomFromList.boolValue) {
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
