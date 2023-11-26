using UnityEngine;

public class CharacterEnvironmentalQuery : MonoBehaviour {

    #region --- Properties ---
    [Tooltip("Game object that specifies the transform of the ledge check GameObject")]
    [SerializeField] private Transform ledgeGrabTransform;
    [Tooltip("Game object that specifies the transform of the player's head")]
    [SerializeField] private Transform headRayTransform;
    [Tooltip("Game object that specifies the transform of the player's chest")]
    [SerializeField] private Transform chestRayTransform;
    [Tooltip("Game object that specifies the transform of the player's feet")]
    [SerializeField] private Transform feetRayTransform;

    [Tooltip("Check this box to exclude the layer assigned to the Character from all the Physics Casts.")]
    [SerializeField] private bool excludeCharacter = true;
    [Tooltip("Name of the player's assigned layer.")]
    [SerializeField] private string characterMask = "Player";
    [Tooltip("Check this box to assign specified layers for the Physics Casts to use.")]
    [SerializeField] private bool useSpecifiedLayer = false;
    [Tooltip("Specify which layers to be used in the Physics Casts.")]
    [SerializeField] private LayerMask specifiedLayer;

    [Tooltip("How far should the cast check for a platform below the character")]
    [SerializeField, Range(0, 2)] private float groundCheckDistance = 0.57f;
    [Tooltip("How far should the cast check for a platform in front of the character")]
    [SerializeField, Range(0, 2)] private float wallDetectionDistance = 0.06f;
    [Tooltip("How far should the cast check for a platform above the character")]
    [SerializeField, Range(0, 2)] private float wallAboveDetectionDistance = 1f;
    [Tooltip("")]
    [SerializeField, Range(0, 2)] private float ledgeGrabRadius = 0.14f;

    [Tooltip("Adjust the size Y of the capsule cast")]
    [SerializeField, Range(0, 1)] private float groundCheckSizeY = 50f / 100f;
    [Tooltip("Adjust the size Y of the box cast")]
    [SerializeField, Range(0, 1)] private float wallAboveSizeY = 72f / 100f;
    [Tooltip("Adjust the size Y of the box cast")]
    [SerializeField, Range(0, 1)] private float headCollisionSizeY = 15.4f / 100f;
    [Tooltip("Adjust the size Y of the box cast")]
    [SerializeField, Range(0, 1)] private float chestCollisionSizeY = 5f / 100f;
    [Tooltip("Adjust the size Y of the box cast")]
    [SerializeField, Range(0, 1)] private float feetCollisionSizeY = 15.4f / 100f;

    private Collider2D activeCollider;
    /// <summary>
    /// Sets the collider to be utilized by the class.
    /// </summary>
    /// <param name="value">The collider to set as active.</param>
    public void SetActiveCollider(Collider2D value) => activeCollider = value;
    #endregion

    #region --- Common ---
    private RaycastHit2D PhysicsBoxCast(Vector3 origin, Vector3 size, Vector3 direction, float maxDistance) {

        return PhysicsBoxCast(origin, size, out RaycastHit2D hitInfo, direction, maxDistance);
    }
    private RaycastHit2D PhysicsBoxCast(Vector3 origin, Vector3 size, out RaycastHit2D hitInfo, Vector3 direction, float maxDistance) {

        if (excludeCharacter)
            return hitInfo = Physics2D.BoxCast(origin, size, 0, direction, maxDistance, ~LayerMask.GetMask(characterMask));
        else if (useSpecifiedLayer)
            return hitInfo = Physics2D.BoxCast(origin, size, 0, direction, maxDistance, specifiedLayer);
        else
            return hitInfo = Physics2D.BoxCast(origin, size, 0, direction, maxDistance);
    }

    private RaycastHit2D PhysicsCapsuleCast(Vector3 origin, Vector3 size, CapsuleDirection2D capsuleDirection, Vector3 castDirection, float distance) {

        return PhysicsCapsuleCast(origin, size, out RaycastHit2D hitInfo, capsuleDirection, castDirection, distance);
    }
    private RaycastHit2D PhysicsCapsuleCast(Vector3 origin, Vector3 size, out RaycastHit2D hitInfo, CapsuleDirection2D capsuleDirection, Vector3 castDirection, float distance) {

        if (excludeCharacter)
            return hitInfo = Physics2D.CapsuleCast(origin, size, capsuleDirection, 0, castDirection, distance, ~LayerMask.GetMask(characterMask));
        else if (useSpecifiedLayer)
            return hitInfo = Physics2D.CapsuleCast(origin, size, capsuleDirection, 0, castDirection, distance, specifiedLayer);
        else
            return hitInfo = Physics2D.CapsuleCast(origin, size, capsuleDirection, 0, castDirection, distance);
    }
    #endregion

    #region --- Collider Around Character ---
    /// <summary>
    /// Searches for any collider in the specified range around the character based on the character's collider.
    /// </summary>
    /// <param name="overlapBox">False, will cast an OverlapCapsule instead.</param>
    /// <param name="range">This value represents a percentage of the character's collider, to be used as the size of the cast.</param>
    /// <returns>True, if the overlap cast finds any colliders, otherwise false.</returns>
    public bool ColliderAroundCharacter(bool overlapBox, float range) {

        Vector2 origin = activeCollider.bounds.center;
        Vector2 size = activeCollider.bounds.size * (1 + range);
        float angle = 0;
        LayerMask excludePlayer = ~LayerMask.GetMask(characterMask);

        if (overlapBox) {
            return Physics2D.OverlapBox(origin, size, angle, excludePlayer);
        }
        else {
            CapsuleCollider2D collider = (CapsuleCollider2D)activeCollider;
            return Physics2D.OverlapCapsule(origin, size, collider.direction, angle, excludePlayer);
        }
    }
    #endregion

    #region --- Ground Check ---
    /// <summary>
    /// Checks if the character steps on a platform
    /// </summary>
    /// <returns></returns>
    public bool GroundCheck() {
        return GroundCheck(0, groundCheckDistance);
    }
    public bool GroundCheck(float originOffset, float castDistance) {
        return GroundCheck(originOffset, castDistance, out RaycastHit2D hitInfo);
    }
    /// <summary>
    /// Checks if the character steps on a platform
    /// </summary>
    /// <param name="originOffset">Offsets the origin of the cast away from the character's right direction.</param>
    /// <param name="castDistance">Overrides the default cast distance.</param>
    /// <param name="hitInfo">If true is returned, hitInfo will contain more information about where the closest collider was hit.</param>
    /// <returns></returns>
    public bool GroundCheck(float originOffset, float castDistance, out RaycastHit2D hitInfo) {

        CapsuleCollider2D capsuleCollider = (CapsuleCollider2D)activeCollider;
        Vector3 castDirection = Vector3.down;
        CapsuleDirection2D capsuleDirection = capsuleCollider.direction;
        Vector3 origin = activeCollider.bounds.center + transform.right * originOffset;
        Vector3 size = activeCollider.bounds.size;
        size.y *= groundCheckSizeY;
        size.x *= 0.99f;
        float maxDistance = castDistance;

        return PhysicsCapsuleCast(origin, size, out hitInfo, capsuleDirection, castDirection, maxDistance);
    }
    #endregion

    #region --- Wall Above ---
    /// <summary>
    /// Checks if there is a wall above of the character's head.
    /// </summary>
    /// <returns>True, if a collider is found, otherwise false.</returns>
    public bool WallAbove() {
        return WallAbove(wallAboveDetectionDistance);
    }
    /// <summary>
    /// Checks if there is a wall above of the character's head.
    /// </summary>
    /// <param name="distance">Overrides the default cast distance.</param>
    /// <returns>True, if a collider is found, otherwise false.</returns>
    public bool WallAbove(float distance) {

        Vector2 origin = activeCollider.bounds.center;
        Vector2 size = activeCollider.bounds.size * 0.99f;
        size.y *= wallAboveSizeY;
        Vector2 direction = Vector3.up;
        float maxDistance = distance;

        return PhysicsBoxCast(origin, size, direction, maxDistance);
    }
    #endregion

    #region --- Wall In Front ---
    /// <summary>
    /// Checks if there is a wall in front of the character.
    /// </summary>
    /// <returns></returns>
    public bool WallInFront() {
        return WallInFront(wallDetectionDistance);
    }
    /// <summary>
    /// Checks if there is a wall in front of the character.
    /// </summary>
    /// <param name="distance">Overrides the default cast distance.</param>
    /// <returns></returns>
    public bool WallInFront(float distance) {
        return WallInFront(out RaycastHit2D hitInfo, distance);
    }
    /// <summary>
    /// Checks if there is a wall in front of the character.
    /// </summary>
    /// <param name="hitInfo">Information of the collider that got hit.</param>
    /// <param name="distance">Overrides the default cast distance.</param>
    /// <returns></returns>
    public bool WallInFront(out RaycastHit2D hitInfo, float distance) {

        CapsuleCollider2D capsuleCollider = (CapsuleCollider2D)activeCollider;
        CapsuleDirection2D capsuleDirection = capsuleCollider.direction;
        Vector3 origin = activeCollider.bounds.center;
        Vector3 size = activeCollider.bounds.size * 0.99f;
        Vector2 castDirection = transform.right;
        float maxDistance = distance;

        return PhysicsCapsuleCast(origin, size, out hitInfo, capsuleDirection, castDirection, maxDistance);
    }
    #endregion

    #region --- Ledge Grab Check ---
    /// <summary>
    /// Checks if the character is able to grab onto a ledge.
    /// </summary>
    /// <param name="exitLedgeGrab">Indicates that the ability to perform ledge grab is available again.</param>
    /// <param name="fixedOffset">The position of the detected ledge.</param>
    /// <returns>True, if a ledge is detected, otherwise false.</returns>
    public bool LedgeGrabCheck(ref bool exitLedgeGrab, out Vector3 fixedOffset) {
        
        bool ledgeDetected = CanLedgeGrab() && InLedgeGrabRange();

        if (!ledgeDetected) {
            exitLedgeGrab = false;
        }

        fixedOffset.x = GetHorizontalLedgePosition();
        fixedOffset.y = GetVerticalLedgePosition();
        fixedOffset.z = 0;
        return ledgeDetected;
    }
    
    private bool InLedgeGrabRange() {

        Vector3 origin = activeCollider.bounds.center;
        origin.y += activeCollider.bounds.size.y / 4; // Raise the cast origin a bit up, in order to be able to detect thin platforms.
        Vector3 size = activeCollider.bounds.size;
        Vector2 castDirection = transform.right;
        float maxDistance = wallDetectionDistance;

        return PhysicsBoxCast(origin, size, castDirection, maxDistance);
    }

    private bool CanLedgeGrab() {

        Vector3 circleOrigin = ledgeGrabTransform.position;
        float circleRadius = ledgeGrabRadius;

        float placeBoxAboveCircle = 1.5f;
        Vector3 originOffset = circleRadius * placeBoxAboveCircle * Vector3.up;
        Vector3 boxOrigin = circleOrigin + originOffset;
        float sizeX = circleRadius * 4;
        float sizeY = circleRadius;
        Vector3 boxSize = new(sizeX, sizeY);

        bool boxCast;
        bool circleCast;

        if (excludeCharacter) {
            boxCast = Physics2D.OverlapBox(boxOrigin, boxSize, 0, ~LayerMask.GetMask(characterMask));
            circleCast = Physics2D.OverlapCircle(circleOrigin, circleRadius, ~LayerMask.GetMask(characterMask));
        }
        else if (useSpecifiedLayer) {
            boxCast = Physics2D.OverlapBox(boxOrigin, boxSize, 0, specifiedLayer);
            circleCast = Physics2D.OverlapCircle(circleOrigin, circleRadius, specifiedLayer);
        }
        else {
            boxCast = Physics2D.OverlapBox(boxOrigin, boxSize, 0);
            circleCast = Physics2D.OverlapCircle(circleOrigin, circleRadius);
        }
        return !boxCast && circleCast;
    }

    private float GetHorizontalLedgePosition() {
        Vector2 origin = new(transform.position.x, ledgeGrabTransform.position.y - ledgeGrabRadius - 0.05f);
        Vector2 direction = transform.right;
        float distance = activeCollider.bounds.size.x / 2 + activeCollider.bounds.size.x / 2 * 160f / 100;
        RaycastHit2D hit;
        if (excludeCharacter)
            hit = Physics2D.Raycast(origin, direction, distance, ~LayerMask.GetMask(characterMask));
        else if (useSpecifiedLayer)
            hit = Physics2D.Raycast(origin, direction, distance, specifiedLayer);
        else
            hit = Physics2D.Raycast(origin, direction, distance);

        return hit.point.x;
    }

    private float GetVerticalLedgePosition() {
        Vector3 originOffset = ledgeGrabRadius * 2 * Vector3.up;
        Vector3 circleOrigin = ledgeGrabTransform.position + originOffset;
        Vector3 direction = Vector3.down;
        float circleRadius = ledgeGrabRadius;
        float maxDistance = circleRadius * 4f;
        RaycastHit2D hit;
        if (excludeCharacter)
            hit = Physics2D.CircleCast(circleOrigin, circleRadius, direction, maxDistance, ~LayerMask.GetMask(characterMask));
        else if (useSpecifiedLayer)
            hit = Physics2D.CircleCast(circleOrigin, circleRadius, direction, maxDistance, specifiedLayer);
        else
            hit = Physics2D.CircleCast(circleOrigin, circleRadius, direction, maxDistance);

        return hit.point.y;
    }
    #endregion

    #region --- Body Part Rays ---
    /// <summary>
    /// Checks if there is a collider in front of the character's head, based on the transform asigned as the origin for the cast.
    /// </summary>
    /// <returns>True, if a collider was found, otherwise false.</returns>
    public bool HeadCollisionCheck() {
        return HeadCollisionCheck(wallDetectionDistance);
    }
    /// <summary>
    /// Checks if there is a collider in front of the character's head, based on the transform asigned as the origin for the cast.
    /// </summary>
    /// <returns>True, if a collider was found, otherwise false.</returns>
    /// <param name="distance">Override the default cast distance.</param>
    public bool HeadCollisionCheck(float distance) {
        return HeadCollisionCheck(out RaycastHit2D hitInfo, distance);
    }
    /// <summary>
    /// Checks if there is a collider in front of the character's head, based on the transform asigned as the origin for the cast.
    /// </summary>
    /// <param name="hitInfo">If true is returned, hitInfo will contain more information about where the closest collider was hit.</param>
    /// <param name="distance">Override the default cast distance.</param>
    /// <returns>True, if a collider was found, otherwise false.</returns>
    public bool HeadCollisionCheck(out RaycastHit2D hitInfo, float distance) {
        return BodyPartRay(headRayTransform.position, headCollisionSizeY, distance, out hitInfo);
    }

    /// <summary>
    /// Checks if there is a collider in front of the character's chest, based on the transform asigned as the origin for the cast.
    /// </summary>
    /// <returns>True, if a collider was found, otherwise false.</returns>
    public bool ChestCollisionCheck() {
        return ChestCollisionCheck(wallDetectionDistance);
    }
    public bool ChestCollisionCheck(float distance) {
        return ChestCollisionCheck(out RaycastHit2D hitInfo, distance);
    }
    /// <summary>
    /// Checks if there is a collider in front of the character's chest, based on the transform asigned as the origin for the cast.
    /// </summary>
    /// <param name="distance">Overrides the cast's default distance.</param>
    /// <returns>True, if a collider was found, otherwise false.</returns>
    public bool ChestCollisionCheck(out RaycastHit2D hitInfo, float distance) {
        return BodyPartRay(chestRayTransform.position, chestCollisionSizeY, distance, out hitInfo);
    }

    /// <summary>
    /// Checks if there is a collider in front of the character's feet, based on the transform asigned as the origin for the cast.
    /// </summary>
    /// <returns>True, if a collider was found, otherwise false.</returns>
    public bool FeetCollisionCheck() {
        return FeetCollisionCheck(wallDetectionDistance);
    }
    public bool FeetCollisionCheck(float distance) {
        return FeetCollisionCheck(out RaycastHit2D hitInfo, distance);
    }
    /// <summary>
    /// Checks if there is a collider in front of the character's feet, based on the transform asigned as the origin for the cast.
    /// </summary>
    /// <param name="distance">Overrides the cast's default distance</param>
    /// <returns>True, if a collider was found, otherwise false.</returns>
    public bool FeetCollisionCheck(out RaycastHit2D hitInfo, float distance) {
        return BodyPartRay(feetRayTransform.position, feetCollisionSizeY, distance, out hitInfo);
    }

    private RaycastHit2D BodyPartRay(Vector3 origin, float sizeMultiplier, float distance, out RaycastHit2D hitInfo) {

        Vector3 size = activeCollider.bounds.size;
        size.y *= sizeMultiplier;
        size.x *= 50f / 100f;
        origin += transform.right * size.x * 50f / 100f;
        Vector3 direction = transform.right;
        float maxDistance = distance;

        return hitInfo = PhysicsBoxCast(origin, size, direction, maxDistance);
    }
    #endregion

    #region --- Debug ---
#if UNITY_EDITOR
    public enum DebugMode { None, GroundCheck, WallAbove, WallInFront, LedgeGrab, AllSpecificCastPoints, HeadCollision, ChestCollision, FeetCollision }
    [Tooltip("When in play mode, select which Physics Cast you wish to debug")]
    [SerializeField] private DebugMode debugMode = DebugMode.None;
    [Tooltip("Specify the color of the drawn wire boxes")]
    [SerializeField] private Color debugColor = Color.white;
    public bool ExcludeCharacter { get => excludeCharacter; set => excludeCharacter = value; }
    public bool UseSpecifiedLayer { get => useSpecifiedLayer; set => useSpecifiedLayer = value; }

    private void Update() {
        if (!activeCollider) {
            Debug.LogError("PlayerEnvironmentalQuery: No collider has been provided. " +
                "Please make sure the script that calls the checks, also sets the 'activeCollider' variable.");
        }
        enabled = false;
    }

    private void OnDrawGizmos() {
        if (!activeCollider && debugMode != DebugMode.None) {
            activeCollider = GameObject.Find("Vertical").GetComponent<CapsuleCollider2D>();
            if (!activeCollider) {
                debugMode = DebugMode.None;
                Debug.LogError("PlayerEnvironmentalQuery: Could not find character's collider. Please make sure that the 'Character' prefab child objects were not altered in any way");
                return;
            }
        }
        DebugPhysicsCasts(out Vector3 origin, out Vector3 size);
        Gizmos.color = debugColor;
        Gizmos.DrawWireCube(origin, size);
    }

    private void DebugPhysicsCasts(out Vector3 origin, out Vector3 size) {

        origin = Vector3.zero;
        size = Vector3.zero;

        switch (debugMode) {
            case DebugMode.GroundCheck:
            GroundCheckDebug(out origin, out size);
            break;
            case DebugMode.WallAbove:
            WallAboveDebug(out origin, out size);
            break;
            case DebugMode.WallInFront:
            WallInFrontDebug(out origin, out size);
            break;
            case DebugMode.LedgeGrab:
            LedgeGrabDebug(out Vector3 circleOrigin, out float circleRadius, out Vector3 boxOrigin, out Vector3 boxSize);
            Gizmos.DrawWireSphere(circleOrigin, circleRadius);
            Gizmos.DrawWireCube(boxOrigin, boxSize);
            break;
            case DebugMode.AllSpecificCastPoints:
            HeadCollisionDebug(out origin, out size);
            Gizmos.color = debugColor;
            Gizmos.DrawWireCube(origin, size);

            ChestCollisionDebug(out origin, out size);
            Gizmos.color = debugColor;
            Gizmos.DrawWireCube(origin, size);

            FeetCollisionDebug(out origin, out size);
            Gizmos.color = debugColor;
            Gizmos.DrawWireCube(origin, size);

            LedgeGrabDebug(out circleOrigin, out circleRadius, out boxOrigin, out boxSize);
            Gizmos.DrawWireSphere(circleOrigin, circleRadius);
            Gizmos.DrawWireCube(boxOrigin, boxSize);
            break;
            case DebugMode.HeadCollision:
            HeadCollisionDebug(out origin, out size);
            break;
            case DebugMode.ChestCollision:
            ChestCollisionDebug(out origin, out size);
            break;
            case DebugMode.FeetCollision:
            FeetCollisionDebug(out origin, out size);
            break;
        }
    }

    private void GroundCheckDebug(out Vector3 origin, out Vector3 size) {

        Vector3 castDirection = Vector3.down;
        origin = activeCollider.bounds.center;
        size = activeCollider.bounds.size;
        float maxDistance = groundCheckDistance;

        size.y *= groundCheckSizeY;
        origin += castDirection * maxDistance;
    }

    private void WallAboveDebug(out Vector3 origin, out Vector3 size) {

        Vector3 castDirection = Vector3.up;
        float maxDistance = wallAboveDetectionDistance;
        origin = activeCollider.bounds.center;
        size = activeCollider.bounds.size;

        size.y *= wallAboveSizeY;
        origin += castDirection * maxDistance;
    }

    private void WallInFrontDebug(out Vector3 origin, out Vector3 size) {

        Vector3 castDirection = transform.right;
        origin = activeCollider.bounds.center;
        size = activeCollider.bounds.size * 0.99f;
        float maxDistance = wallDetectionDistance;

        origin += castDirection * maxDistance;
    }

    private void LedgeGrabDebug(out Vector3 circleOrigin, out float circleRadius, out Vector3 boxOrigin, out Vector3 boxSize) {

        circleOrigin = ledgeGrabTransform.position;
        circleRadius = ledgeGrabRadius;

        boxOrigin = circleOrigin + Vector3.up * circleRadius * 1.5f;
        boxSize = new Vector2(circleRadius * 4, circleRadius);
    }

    private void HeadCollisionDebug(out Vector3 origin, out Vector3 size) {

        BodyPartRayDebug(headRayTransform, headCollisionSizeY, out origin, out size);
    }

    private void ChestCollisionDebug(out Vector3 origin, out Vector3 size) {

        BodyPartRayDebug(chestRayTransform, chestCollisionSizeY, out origin, out size);
    }

    private void FeetCollisionDebug(out Vector3 origin, out Vector3 size) {

        BodyPartRayDebug(feetRayTransform, feetCollisionSizeY, out origin, out size);
    }

    private void BodyPartRayDebug(Transform bodyPartTransform, float sizeMultiplier, out Vector3 origin, out Vector3 size) {

        origin = bodyPartTransform.position;
        size = activeCollider.bounds.size;
        size.y *= sizeMultiplier;
        size.x *= 50f / 100f;
        Vector3 direction = transform.right;
        float maxDistance = wallDetectionDistance;

        origin += direction * maxDistance + transform.right * size.x * 50f / 100f;
    }
#endif
    #endregion
}
