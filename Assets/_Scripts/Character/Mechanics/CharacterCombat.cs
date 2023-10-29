using UnityEngine;

[RequireComponent(typeof(Rigidbody2D))]
public class CharacterCombat : MonoBehaviour {

    public class OnAttackInitiatedEventArgs {
        public Animation animationClip;
    }
    public event System.EventHandler<OnAttackInitiatedEventArgs> OnAttackInitiated;

    [Header("--- Melee Attack ---")]
    [Tooltip("When the attack key is pressed an impulse force is applied to push the character forward")]
    [SerializeField] private float forceAmount = 1000f;
    [Tooltip("Linear drag coefficient to apply to the character's rigidbody for the duration of the attack. " +
        "After the attack is completed the coefficient resets back to the default rigidbody's value")]
    [SerializeField] private float linearDrag = 10f;
    [Tooltip("Whether or not the gravity scale of the rigidbody should be set to '0' for the duration of the attack")]
    [SerializeField] private bool zeroGravity = true;
    [Tooltip("If your animation moves the character, you may need to adjust the character's position after the animation has been completed")]
    [SerializeField] private Vector2 adjustPosition;

    [Header("--- Ranged Attack ---")]
    [Tooltip("The transform from which the projectile should be launched.")]
    [SerializeField] private Transform projectileSpawnTransform;
    [Tooltip("Should your character be forced to stand still for as long as they're charging an attack?")]
    [SerializeField] private bool standWhileCharging = false;
    [Tooltip("How long should it take for an attack to be fully charged")]
    [SerializeField] private float chargeTime;
    [Tooltip("For how long should your character be able to hold their attack once charged.")]
    [SerializeField] private float holdAttackTime = 4f;

    private AttackSO attackData;
    private Rigidbody2D playerRb;
    private float defaultLinearDrag;
    private float defaultGravityScale;
    private float chargeTimer;
    private float holdAttackTimer;
    private bool rangedAttackCharged = false;

    private void Awake() {
        playerRb = GetComponent<Rigidbody2D>();
        defaultLinearDrag = playerRb.drag;
        defaultGravityScale = playerRb.gravityScale;
    }

    public void EnterAttackState(AttackSO attackData = null) {
        if (!attackData.UseGravity) {
            playerRb.gravityScale = 0f;
        }
        if (attackData.AttackPushesCharacter) {
            StartCoroutine(PushCharacter(attackData));
            playerRb.drag = attackData.DragCoeficient;
        }
        playerRb.velocity = Vector2.zero;

        if (attackData.IsChargeableAttack) {
            // todo: chargeable logic;
        }
    }

    private System.Collections.IEnumerator PushCharacter(AttackSO attackData) {
        yield return new WaitForSeconds(attackData.DelayForceTime);
        Vector3 direction = transform.right;
        Vector3 force = attackData.Force * direction.x;
        playerRb.AddForce(force, attackData.ForceMode);
    }

    public void ExitAttackState() {
        playerRb.drag = defaultLinearDrag;
        playerRb.gravityScale = defaultGravityScale;
    }

    public void ChargeAttack(AttackSO attackData) {
        if (!attackData.CanMoveWhileCharging) {
            playerRb.isKinematic = true;
            playerRb.velocity = Vector3.zero;
        }

        rangedAttackCharged = CocaCopa.Utilities.TickTimer(ref chargeTimer, attackData.ChargeTime, false);

        if (rangedAttackCharged) {
            if (CocaCopa.Utilities.TickTimer(ref holdAttackTimer, attackData.HoldChargeTime, false)) {
                rangedAttackCharged = false;
                ExitAttackState();
            }
        }
    }

    public void ReleaseChargedAttack(bool attackCanceled = false) {
        if (standWhileCharging) {
            playerRb.isKinematic = false;
        }
        if (!attackCanceled) {
            //Instantiate(projectileToShoot, projectileSpawnTransform.position, Quaternion.identity);
        }
    }
}
