using UnityEngine;

public class CharacterMovement : MonoBehaviour {

    //------------------------------------------------------------------------------------//
    //-------------------------------------- *NOTE* --------------------------------------//
    //--- PlayerMovement has no way of knowing if the speed of your character changed. ---//
    //--- It calculates speed independently. Thus, if a sudden change in velocity --------//
    //--- happens to your character outside of this script, you need to set the ----------//
    //--- PlayerMovement.CurrentSpeed property accordingly. ------------------------------//
    //------------------------------------------------------------------------------------//
    //------------------------------------------------------------------------------------//

    [Header("--- Grounded ---")]
    [Tooltip("Curve of the character's acceleration")]
    [SerializeField] private AnimationCurve accelerationCurve;
    [Tooltip("Curve of the character's desceleration")]
    [SerializeField] private AnimationCurve descelerationCurve;
    [Tooltip("How fast should the acceleration curves be evaluated")]
    [SerializeField] private float curveEvaluationSpeed = 4;
    [Tooltip("Character's top speed (m/s)")]
    [SerializeField] private float moveSpeed = 12f;

    [Header("--- On Air ---")]
    [Tooltip("Curve of the character's on air acceleration")]
    [SerializeField] private AnimationCurve airAccelerationCurve;
    [Tooltip("Curve of the character's on air desceleration")]
    [SerializeField] private AnimationCurve airDescelerationCurve;
    [Tooltip("How fast should the acceleration curves be evaluated")]
    [SerializeField] private float onAirEvaluationSpeed = 2.2f;
    [Tooltip("Velocity change of the character, on the Y axis, when a jump is performed")]
    [SerializeField] private float jumpStrength = 19f;
    [Tooltip("Velocity change of the character, on the X and Y axis, when a jump is performed against the wall")]
    [SerializeField] private Vector2 wallJumpStrength = new (16.5f, 18f);
    [Tooltip("How fast should the character lose speed when no input is given, while on air")]
    [SerializeField] private float airDesceleration = 7.5f;
    [Tooltip("The maximum speed of which the character is allowed to reach, from a stand still jump (percentage of 'moveSpeed' variable)")]
    [SerializeField, Range(0, 1)] private float maxStandStillSpeed = 50f / 100f;

    private Vector2 moveDirection;
    private float currentSpeed = 0; // Grounded state calculates currentSpeed and Airborne state uses it as a base to calculate air movement.
    private float speedMultiplier = 0;
    private float airSpeedMultiplier = 0;
    private float onGroundAnimPoints = 0;
    private float onAirAnimPoints = 0;
    private bool startAirAcceleration = false;

    public float TopSpeed => moveSpeed;
    public float CurrentSpeed { set { currentSpeed = value; } }
    public Vector3 MoveDirection { set { moveDirection = value; } }

    private void Update() {
        
        if (currentSpeed == 0) {
            ResetOnGroundValues();
        }

        if (currentSpeed != 0 && speedMultiplier == 0) {
            speedMultiplier = currentSpeed / moveSpeed;
        }

        if (currentSpeed >= moveSpeed) {
            onGroundAnimPoints = 1;
        }
    }

    /// <summary>
    /// Only calculates the velocity on the X axis.
    /// *Note* Best use of this function is, for when the character is grounded
    /// </summary>
    /// <param name="moveInput">Player input</param>
    /// <param name="runsIntoWall">Did the character hit a wall?</param>
    /// <returns>A vector with the calculated velocity for the X axis only</returns>
    public Vector2 OnGroundHorizontalVelocity(Vector2 moveInput, bool runsIntoWall) {

        if (runsIntoWall) {
            ResetOnGroundValues();
            return Vector3.zero;
        }
        ResetOnAirValues();
        GroundAnimPointsEvaluation(moveInput);

        if (moveInput != Vector2.zero) {
            moveDirection = moveInput;
        }
        float runDirection = moveDirection.x;
        currentSpeed = speedMultiplier * moveSpeed;

        return new (currentSpeed * runDirection, 0);
    }

    private void ResetOnGroundValues() {
        speedMultiplier = 0;
        currentSpeed = 0;
        onGroundAnimPoints = 0;
    }

    private void GroundAnimPointsEvaluation(Vector2 moveInput) {

        if (moveInput != Vector2.zero) {
            onGroundAnimPoints += curveEvaluationSpeed * Time.deltaTime;
            speedMultiplier = accelerationCurve.Evaluate(onGroundAnimPoints);
        }

        if (moveInput == Vector2.zero) {
            if (onGroundAnimPoints > 0) {
                onGroundAnimPoints -= curveEvaluationSpeed * Time.deltaTime;
                speedMultiplier = descelerationCurve.Evaluate(onGroundAnimPoints);
            }
            if (onGroundAnimPoints == 0) {
                speedMultiplier = 0;
            }
        }
        onGroundAnimPoints = Mathf.Clamp01(onGroundAnimPoints);
    }

    /// <summary>
    /// Only calculates the velocity on the Y axis based on the strength of the jump.
    /// </summary>
    /// <param name="againstWall">Is the character performing a jump against the wall?</param>
    /// <returns>A vector with the calculated velocity for the Y axis only</returns>
    public Vector2 VerticalVelocity(bool againstWall) {

        if (againstWall) {
            ResetOnAirValues();
            moveDirection = -transform.right;
            currentSpeed = wallJumpStrength.x;

            float axisX = -transform.right.x;
            float axisY = wallJumpStrength.y;

            return new(axisX, axisY);
        }
        else {
            return new(0, jumpStrength);
        }
    }

    /// <summary>
    /// Handles the movement of the character when NOT grounded
    /// </summary>
    /// <param name="moveInput">Player input</param>
    /// <param name="runsIntoWall">Did the character hit a wall?</param>
    /// <returns>A vector with the calculated velocity for the X axis only, based on the move speed the character had when grounded</returns>
    public Vector2 OnAirHorizontalVelocity(Vector2 moveInput, bool runsIntoWall) {
        bool inputOppositeToMoveDirection = moveInput.x * moveDirection.x == -1;
        bool inputSameAsMoveDirection = moveInput.x * moveDirection.x == 1;
        bool noInputFromPlayer = moveInput == Vector2.zero;

        if (runsIntoWall) {
            onAirAnimPoints = 1;
            airSpeedMultiplier = 1;
        }
        if (currentSpeed == 0 && moveInput != Vector2.zero && runsIntoWall == false) {

            moveDirection = moveInput;
            currentSpeed = moveSpeed * maxStandStillSpeed;
            onAirAnimPoints = 0;
            startAirAcceleration = true;
        }
        else if (inputOppositeToMoveDirection) {
            OnAirAnimPointsEvaluation(true);

            if (onAirAnimPoints == 1) {

                moveDirection = moveInput;
                startAirAcceleration = true;
                onAirAnimPoints = 0;
            }
        }
        else if (inputSameAsMoveDirection && startAirAcceleration) {
            OnAirAnimPointsEvaluation(false);
        }
        else if (noInputFromPlayer) {
            if (currentSpeed > 0) {
                currentSpeed -= airDesceleration * Time.deltaTime;
            }
            else {
                currentSpeed = 0;
            }
        }

        // Use of 'currentSpeed' to calculate based on the speed the character had, before jumping/falling
        return new (airSpeedMultiplier * currentSpeed * moveDirection.x, 0);
    }

    private void ResetOnAirValues() {
        airSpeedMultiplier = 1;
        onAirAnimPoints = 0;
        startAirAcceleration = false;
    }

    private void OnAirAnimPointsEvaluation(bool oppositeDirection) {
        onAirAnimPoints += onAirEvaluationSpeed * Time.deltaTime;
        onAirAnimPoints = Mathf.Clamp01(onAirAnimPoints);

        if (oppositeDirection) {
            airSpeedMultiplier = 1 - airDescelerationCurve.Evaluate(onAirAnimPoints);
        }
        else {
            airSpeedMultiplier = airAccelerationCurve.Evaluate(onAirAnimPoints);
        }
    }
}
