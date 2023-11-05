using UnityEngine;
using UnityEngine.UIElements;

[RequireComponent(typeof(Rigidbody2D))]
public class CharacterCombat : MonoBehaviour {

    public class OnAttackInitiatedEventArgs {
        public Animation animationClip;
    }

    private Rigidbody2D playerRb;
    private float defaultLinearDrag;
    private float defaultGravityScale;
    private bool moveWhileCastingAttack;
    
    [SerializeField] private bool attackCharged = false;
    [SerializeField] private float chargeTimer;
    [SerializeField] private float holdAttackTimer;

    private void Awake() {
        playerRb = GetComponent<Rigidbody2D>();
        defaultLinearDrag = playerRb.drag;
        defaultGravityScale = playerRb.gravityScale;
    }

    /// <summary>
    /// Locks your character into 'Attack State' based on the scriptable object data
    /// </summary>
    /// <param name="attackData">The scriptable object that the data of the attack</param>
    public void EnterAttackState(AttackSO attackData) {
        if (!attackData.UseGravity) {
            playerRb.gravityScale = 0f;
        }
        if (attackData.AttackPushesCharacter && !attackData.CanMoveWhileAttacking && !attackData.CanMoveWhileCharging) {
            StartCoroutine(PushCharacter(attackData));
        }
        if (attackData.ResetVelocity) {
            playerRb.velocity = Vector2.zero;
        }
        if (attackData.IsChargeableAttack) {
            chargeTimer = attackData.ChargeTime;
            holdAttackTimer = attackData.HoldChargeTime;
            moveWhileCastingAttack = attackData.CanMoveWhileCharging;
        }
        else {
            moveWhileCastingAttack = attackData.CanMoveWhileAttacking;
        }
    }

    private System.Collections.IEnumerator PushCharacter(AttackSO attackData) {
        yield return new WaitForSeconds(attackData.DelayForceTime);
        Vector3 direction = transform.right;
        Vector3 force = attackData.Force;
        force.x *= direction.x;
        playerRb.AddForce(force, attackData.ForceMode);
        playerRb.drag = attackData.DragCoeficient;
    }

    /// <summary>
    /// Unlocks your character after the attack is completed
    /// </summary>
    public void ExitAttackState(AttackSO attackData, bool adjustPosition = true) {
        playerRb.drag = defaultLinearDrag;
        playerRb.gravityScale = defaultGravityScale;
        moveWhileCastingAttack = false;
        if (adjustPosition && attackData.AdjustPositionOnAttackEnd != Vector3.zero) {
            StartCoroutine(TeleportToPosition(attackData.AdjustPositionOnAttackEnd));
            Vector3 position = attackData.AdjustPositionOnAttackEnd;
        }
    }

    private System.Collections.IEnumerator TeleportToPosition(Vector3 position) {
        yield return new WaitForEndOfFrame();
        position.x *= transform.right.x;
        transform.position += position;
    }

    /// <summary>
    /// Makes the character charge an attack
    /// </summary>
    /// <param name="attackData">The scriptable object that the data of the attack</param>
    /// <param name="chargeOvertime">Indicates when the character holded the attack for more than the allowed time</param>
    public void ChargeAttack(AttackSO attackData, out bool chargeOvertime) {
        
        chargeOvertime = false;
        attackCharged = CocaCopa.Utilities.TickTimer(ref chargeTimer, attackData.ChargeTime, false);

        if (attackCharged) {
            if (CocaCopa.Utilities.TickTimer(ref holdAttackTimer, attackData.HoldChargeTime, false)) {
                chargeOvertime = true;
                attackCharged = false;
            }
        }
    }

    /// <summary>
    /// Releases a charged attack. This function will not 'ExitAttackState()'
    /// </summary>
    /// <param name="attackData">The scriptable object that the data of the attack</param>
    public void ReleaseChargedAttack(AttackSO attackData) {
        moveWhileCastingAttack = attackData.CanMoveOnReleaseAttack;
    }

    /// <summary>
    /// If your character can move during an attack, this function will adjust their horizontal velocity based on the provided attack data.
    /// Should be called after your horizontal velocity calculation.
    /// </summary>
    /// <param name="attackData">The scriptable object that the data of the attack</param>
    /// <param name="horizontalVelocity">Current horizontal velocity</param>
    public void CanMoveWhileCastingAttack(AttackSO attackData, ref Vector2 horizontalVelocity) {
        if (moveWhileCastingAttack) {
            if (attackData.CanMoveWhileCharging) {
                horizontalVelocity *= attackData.ChargeMoveSpeedPercentage;
            }
            else if (attackData.CanMoveWhileAttacking) {
                horizontalVelocity *= attackData.AttackMoveSpeedPercentage;
            }
        }
        else if (attackData != null && attackData.IsChargeableAttack) {
            if (!moveWhileCastingAttack) {
                horizontalVelocity = Vector2.zero;
            }
        }
    }
}
