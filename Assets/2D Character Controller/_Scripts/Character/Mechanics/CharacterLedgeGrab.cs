#if LEDGE_GRAB_COMPONENT
using CocaCopa;
using UnityEngine;

[RequireComponent(typeof(Rigidbody2D))]
public class CharacterLedgeGrab : MonoBehaviour {

    [Tooltip("Transform of the parent that holds the gameObject with 'Sprite Renderer' attached")]
    [SerializeField] private Transform spriteHolderTransform;
    [Tooltip("Transform of the character's vertical collider")]
    [SerializeField] private Transform colliderTransform;
    [Tooltip("A raycast will be fired downward to determine the precise location where the character should be positioned after performing the ledge climb")]
    [SerializeField] private Transform ledgeClimbEndTransform;
    [Tooltip("True, if all frames of the Ledge Climb animation are at the same position. False, if the frames of the Ledge Climb animation vary in position.")]
    [SerializeField] private bool isAnimationStill = false;
    [Tooltip("How fast should the character climb off a ledge. (Works if 'isAnimationStill' is set to true)")]
    [SerializeField] private float ledgeClimbSpeed;
    [Tooltip("Your character is expected to be offseted. Adjust this 'Vector2' value to set it back to the intended position")]
    [SerializeField] private Vector2 offsetSprite = new (-0.25f, -1.64f);
    [Tooltip("Offset the character's collider if it doesn't correspond to the player's position.")]
    [SerializeField] private float offsetColliderHeight = 0;

    public bool IsAnimationStill => isAnimationStill;

    private float climbAnimationPoints;
    private readonly AnimationCurve climbCurve = AnimationCurve.EaseInOut(0, 0, 1, 1);

    private Rigidbody2D playerRb;
    private CharacterAnimator characterAnimator;

    private void Awake() {
        playerRb = GetComponent<Rigidbody2D>();
        characterAnimator = GetComponentInChildren<CharacterAnimator>();
        if (spriteHolderTransform.TryGetComponent<SpriteRenderer>(out _)) {
            Debug.LogError("'spriteHolderTransform' should not have a sprite renderer attached. " +
                "Please create a new gameObject and set it as the parent of the one with the Sprite Renderer component");
        }
        
    }

    /// <summary>
    /// Sets character to Ledge Grab state
    /// </summary>
    /// <param name="ledgePosition">The ledge position detected by your raycast</param>
    public void EnterLedgeGrabState(Vector3 ledgePosition) {
        enabled = true;
        playerRb.isKinematic = true;
        playerRb.velocity = Vector2.zero;
        OffsetPositions(ledgePosition);
    }

    /// <summary>
    /// Exit from Ledge Grab state
    /// </summary>
    public void ExitLedgeGrabState() {
        ResetOffsets();
        playerRb.isKinematic = false;
    }

    private void OffsetPositions(Vector3 ledgePosition) {
        Vector3 spriteOffset = offsetSprite;
        spriteOffset.x = transform.right.x > 0
            ? offsetSprite.x
            : -offsetSprite.x;

        Vector3 offsetPosition = ledgePosition + spriteOffset;

        if (spriteHolderTransform != null)
            spriteHolderTransform.position = offsetPosition;
        
        if (colliderTransform != null) {
            Vector2 position = colliderTransform.localPosition;
            position.y = offsetColliderHeight;
            colliderTransform.localPosition = position;
        }
    }

    private void ResetOffsets() {
        if (spriteHolderTransform != null)
            spriteHolderTransform.localPosition = Vector3.zero;

        if (colliderTransform != null)
            colliderTransform.localPosition = Vector3.zero;
    }

    /// <summary>
    /// Makes the character climb at the top of a ledge
    /// </summary>
    /// <param name="isLedgeClimbing">Indicates when the climb is finished</param>
    /// <param name="endPosition">The position of the sprite holder once the ledge climb is finished</param>
    public void LedgeClimb(Vector3 ledgePosition, out bool isLedgeClimbing, out Vector3 endPosition) {
        isLedgeClimbing = true;
        playerRb.velocity = Vector2.zero;
        playerRb.isKinematic = true;
        endPosition = GetEndPosition();

        if (isAnimationStill) {
            float lerpTime = Utilities.EvaluateAnimationCurve(climbCurve, ref climbAnimationPoints, ledgeClimbSpeed);
            Vector3 localEndPosition = transform.InverseTransformPoint(endPosition);
            spriteHolderTransform.localPosition = Vector3.Lerp(Vector3.zero, localEndPosition, lerpTime);
            if (climbAnimationPoints == 1) {
                climbAnimationPoints = 0;
                playerRb.isKinematic = false;
                isLedgeClimbing = false;
                spriteHolderTransform.localPosition = Vector3.zero;
            }
        }
        else {
            if (isLedgeClimbing) {
                OffsetPositions(ledgePosition);
            }
            if (characterAnimator.CheckStatePercentage(HumanoidStateName.LedgeClimb, 1f)) {
                isLedgeClimbing = false;
            }
        }
    }

    private Vector3 GetEndPosition() {
        RaycastHit2D hit = Physics2D.Raycast(ledgeClimbEndTransform.position, Vector2.down, 25f, ~LayerMask.GetMask("Player"));
        return hit.point;
    }
}
#endif
