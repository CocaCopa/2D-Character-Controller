using UnityEngine;

[RequireComponent(typeof(Rigidbody2D))]
public class CharacterCombat : MonoBehaviour {

    public event System.EventHandler OnAttackEnded;

    [Tooltip("When the attack key is pressed an impulse force is applied to push the character forward")]
    [SerializeField] private float forceAmount = 1000f;
    [Tooltip("Linear drag coefficient to apply to the character's rigidbody for the duration of the attack. " +
        "After the attack is completed the coefficient resets back to the default rigidbody's value")]
    [SerializeField] private float linearDrag = 10f;
    [Tooltip("Whether or not the gravity scale of the rigidbody should be set to '0' for the duration of the attack")]
    [SerializeField] private bool zeroGravity = true;
    [Tooltip("If your animation moves the character, you may need to adjust the character's position after the animation has been completed")]
    [SerializeField] private Vector2 adjustPosition;

    private Rigidbody2D playerRb;
    private float defaultLinearDrag;
    private float defaultGravityScale;

    private void Awake() {
        playerRb = GetComponent<Rigidbody2D>();
        defaultLinearDrag = playerRb.drag;
        defaultGravityScale = playerRb.gravityScale;
    }

    public void EnterAttackState() {
        if (zeroGravity)
            playerRb.gravityScale = 0f;
        playerRb.drag = linearDrag;
        playerRb.velocity = Vector3.zero;
        Vector3 velocity = new(forceAmount * transform.right.x, 0f);
        playerRb.AddForce(velocity, ForceMode2D.Impulse);
    }

    public void ExitAttackState() {
        playerRb.drag = defaultLinearDrag;
        playerRb.gravityScale = defaultGravityScale;
    }
}
