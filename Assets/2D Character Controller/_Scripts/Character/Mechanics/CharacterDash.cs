using UnityEngine;

[RequireComponent(typeof(Rigidbody2D), typeof(CharacterEnvironmentalQuery))]
public class CharacterDash : MonoBehaviour {

    [SerializeField] private GameObject afterImageObject;
    [Tooltip("How many after images should be spawned")]
    [SerializeField] private float afterImageCount = 9f;
    [Space(10)]
    [Tooltip("How fast should the character travel while dashing (m/s)")]
    [SerializeField] private float dashSpeed = 90f;
    [Tooltip("How much distance should the dash cover in meters")]
    [SerializeField] private float maxDashDistance = 6.5f;
    [Tooltip("How far from a wall should the character stop dashing")]
    [SerializeField] private float wallOffset = 0.2f;
    [Tooltip("If the character's feet or head are going to hit a wall during the dash, the position of the character " +
        "will be adjusted before the dash happens, to be on the platform's height, plus the given offset.")]
    [SerializeField, Range(0, 1)] private float adjustDashPosition = 20f / 100;

    public float DashSpeed { get => dashSpeed; set => dashSpeed = value; }
    public float MaximumDashDistance { get => maxDashDistance; set => maxDashDistance = value; }

    public event System.EventHandler<OnDasDistanceCoveredEventArgs> OnDashDistanceCovered;
    public class OnDasDistanceCoveredEventArgs {
        public Vector3 targetDashPosition;
    }

    private Rigidbody2D characterRb;
    private CharacterEnvironmentalQuery envQuery;

    private Vector3 initialDashPosition;
    private Vector3 targetDashPosition;
    private Vector3 nextAfterImagePosition;

    private float dashDistance;

    private void Awake() {

        envQuery = GetComponent<CharacterEnvironmentalQuery>();
        characterRb = GetComponent<Rigidbody2D>();
        enabled = false;
    }

    private void Update() {
        StopDashing();
        transform.position = characterRb.position;
    }

    /// <summary>
    /// Perform a Dash
    /// </summary>
    /// <param name="characterHeight">Height of the character</param>
    /// <param name="isGrounded">Whether the character is grounded</param>
    /// <param name="adjustPosition">Should the position of the character be adjusted if the character is about to hit on the head/feet?</param>
    public void Dash(float characterHeight, bool isGrounded, bool adjustPosition = true) {

        Vector3 positionBeforeDash = adjustPosition
            ? DashInitialPosition(characterHeight, isGrounded)
            : characterRb.position;

        characterRb.position = positionBeforeDash;
        dashDistance = DashDistanceCorrection();

        characterRb.isKinematic = true;
        initialDashPosition = positionBeforeDash;
        nextAfterImagePosition = initialDashPosition;
        enabled = true;

        characterRb.velocity = new(dashSpeed * transform.right.x, 0);
        
        if (afterImageObject)
            AfterImageEffect();
    }

    private float DashDistanceCorrection() {

        bool wallInfront = envQuery.WallInFront(out RaycastHit2D hit, maxDashDistance + 0.5f);
        float correctedDistance = (hit.point - characterRb.position).magnitude - (wallOffset + 0.5f);

        return wallInfront
            ? correctedDistance
            : maxDashDistance;
    }

    /// <summary>
    /// Get the position of the character, the first frame of the dash
    /// </summary>
    /// <param name="characterHeight"></param>
    /// <returns></returns>
    private Vector3 DashInitialPosition(float characterHeight, bool isGrounded) {

        if (!isGrounded && FoundColliderInFront(out RaycastHit2D hit, out bool downwardsOffset)) {

            GetColliderHitPoint(ref hit, characterHeight, downwardsOffset);
            characterRb.position = NewInitialDashPosition(hit.point.y, characterHeight, downwardsOffset);
        }
        return characterRb.position;
    }

    /// <summary>
    /// Check if there is a collider between the maximum dash distance and the character
    /// </summary>
    /// <param name="hit">Info of the collider hit</param>
    /// <param name="downwards">Informs if the collider found is above or below the character</param>
    /// <returns></returns>
    private bool FoundColliderInFront(out RaycastHit2D hit, out bool downwards) {

        bool feetCollision = envQuery.FeetCollisionCheck(out hit, maxDashDistance); // Assume that a collider will be found in front of the character's feet.
        bool headCollision = false; // Assume that a collider will NOT be found in front of the character's head.
        if (!feetCollision) // If the assumption is incorrect, a head check will be initiated.
            headCollision = envQuery.HeadCollisionCheck(out hit, maxDashDistance);
        bool chestCollision = envQuery.ChestCollisionCheck(maxDashDistance); // Check for colliders in front of the character's chest.

        downwards = headCollision; // Position will be adjusted downwards if a collider is found in front of the character's head, or upwards if not.
        return (feetCollision || headCollision) && !chestCollision; // Position will be adjusted only if a collider is found only in front of the character's head or feet.
    }

    /// <summary>
    /// Casts a check on the position the collider found, to get the platforms edge point
    /// </summary>
    /// <param name="refHit">Cast based on the given hit info and recalculate hit based on the casts results</param>
    /// <param name="characterHeight">Size Y of the character's collider</param>
    /// <param name="downwardsCast">Should cast downward or upward</param>
    private void GetColliderHitPoint(ref RaycastHit2D refHit, float characterHeight, bool downwardsCast) {

        float offsetDefaultOrigin = transform.right.x == 1
            ? refHit.point.x - characterRb.position.x
            : characterRb.position.x - refHit.point.x;

        float overrideCastDistance = downwardsCast
            ? -(characterHeight + adjustDashPosition)
            : characterHeight + adjustDashPosition;

        envQuery.GroundCheck(offsetDefaultOrigin, overrideCastDistance, out refHit);
    }

    /// <summary>
    /// Calculates the offseted position
    /// </summary>
    /// <param name="colliderGroundHeight">Edge point of the platform found</param>
    /// <param name="characterHeight">Size Y of the characters collider</param>
    /// <param name="downwardsOffset">Should offset the character upwards or downwards</param>
    /// <returns></returns>
    private Vector3 NewInitialDashPosition(float colliderGroundHeight, float characterHeight, bool downwardsOffset) {

        float characterPivotPosition = characterRb.position.y;
        float distanceFromColliderToHitPoint = colliderGroundHeight - characterPivotPosition;
        float pivotToHitPointDistance = characterHeight - distanceFromColliderToHitPoint;

        float adjustDownwards = characterPivotPosition - pivotToHitPointDistance - adjustDashPosition;
        float adjustUpwards = colliderGroundHeight + adjustDashPosition;

        Vector3 newPosition = characterRb.position;
        newPosition.y = downwardsOffset
            ? adjustDownwards
            : adjustUpwards;

        return newPosition;
    }

    private void StopDashing() {

        if (DashDistanceCovered()) {

            if (dashDistance != maxDashDistance) {
                characterRb.velocity = Vector2.zero;
            }
            OnDashDistanceCovered?.Invoke(this, new OnDasDistanceCoveredEventArgs {
                targetDashPosition = targetDashPosition
            });

            characterRb.isKinematic = false;
            enabled = false;
        }
    }

    private bool DashDistanceCovered() {

        if (transform.right.x > 0)
            return characterRb.position.x >= initialDashPosition.x + (dashDistance);
        else
            return characterRb.position.x <= initialDashPosition.x - (dashDistance);
    }

    /// <summary>
    /// Spawns the object given as the "After Image Object" based on the assigned values
    /// </summary>
    private void AfterImageEffect() {

        Vector3 initialPoint = initialDashPosition;
        Vector3 endPoint;
        AfterImageBasedOnDirection(out endPoint);
        float distanceDivisions = (endPoint.x - initialPoint.x) / afterImageCount;

        for (int i = 0; i < afterImageCount; i++) {

            Instantiate(afterImageObject, nextAfterImagePosition, transform.rotation);
            nextAfterImagePosition.x += distanceDivisions;
        }
    }

    private void AfterImageBasedOnDirection(out Vector3 endPoint) {

        endPoint = transform.right.x == 1
            ? initialDashPosition + new Vector3(dashDistance, 0)
            : initialDashPosition - new Vector3(dashDistance, 0);

        targetDashPosition = endPoint;
    }
}
