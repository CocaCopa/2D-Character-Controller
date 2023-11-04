using UnityEngine;

[RequireComponent(typeof(Rigidbody2D))]
public class CharacterCombat : MonoBehaviour {

    public class OnAttackInitiatedEventArgs {
        public Animation animationClip;
    }

    private AttackSO attackData;
    private Rigidbody2D playerRb;
    private float defaultLinearDrag;
    private float defaultGravityScale;
    
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
    /// <param name="attackData">The scriptable object holding the data of the attack</param>
    public void EnterAttackState(AttackSO attackData) {
        if (!attackData.UseGravity) {
            playerRb.gravityScale = 0f;
        }
        if (attackData.AttackPushesCharacter) {
            StartCoroutine(PushCharacter(attackData));
            playerRb.drag = attackData.DragCoeficient;
        }
        if (attackData.ResetVelocity) {
            playerRb.velocity = Vector2.zero;
        }
        if (attackData.IsChargeableAttack) {
            chargeTimer = attackData.ChargeTime;
            holdAttackTimer = attackData.HoldChargeTime;
        }
    }

    private System.Collections.IEnumerator PushCharacter(AttackSO attackData) {
        yield return new WaitForSeconds(attackData.DelayForceTime);
        Vector3 direction = transform.right;
        Vector3 force = attackData.Force;
        force.x *= direction.x;
        playerRb.AddForce(force, attackData.ForceMode);
    }

    /// <summary>
    /// Unlocks your character after the attack is completed
    /// </summary>
    public void ExitAttackState() {
        playerRb.drag = defaultLinearDrag;
        playerRb.gravityScale = defaultGravityScale;
    }

    /// <summary>
    /// Makes the character charge an attack
    /// </summary>
    /// <param name="attackData">The scriptable object holding the data of the attack</param>
    /// <param name="chargeOvertime">Indicates when the character holded the attack for more than the allowed time</param>
    public void ChargeAttack(AttackSO attackData, out bool chargeOvertime) {
        
        chargeOvertime = false;
        attackCharged = CocaCopa.Utilities.TickTimer(ref chargeTimer, attackData.ChargeTime, false);

        if (attackCharged) {
            if (CocaCopa.Utilities.TickTimer(ref holdAttackTimer, attackData.HoldChargeTime, false)) {
                chargeOvertime = true;
                attackCharged = false;
                ExitAttackState();
            }
        }
    }

    /// <summary>
    /// Releases a charged attack
    /// </summary>
    public void ReleaseChargedAttack() {
        
    }
}
