using UnityEngine;

[RequireComponent(typeof(Rigidbody2D))]
public class CharacterSlide : MonoBehaviour {

    [Tooltip("How fast should the character's speed decrease when floor sliding")]
    [SerializeField] private float floorSlideDesceleration = 7f;
    [Tooltip("Exponential acceleration when the character is wall sliding")]
    [SerializeField] private float wallSlideAcceleration = 5f;

    private Rigidbody2D playerRb;
    private bool wallSlideFlag = true;
    private float gravityScale;

    private void Awake() {

        playerRb = GetComponent<Rigidbody2D>();
        gravityScale = playerRb.gravityScale;
    }

    /// <summary>
    /// Controls the behaviour of the character for as long as the character is floor sliding
    /// </summary>
    /// <param name="minimumSpeed">Character speed will stop being reduced as soon as it reaches the given value</param>
    public void FloorSlide(float minimumSpeed = 0) {

        Vector3 currentVelocity = playerRb.velocity;
        Vector3 characterFaceDirection = transform.right;
        bool inputSameAsFaceDir = currentVelocity.x * characterFaceDirection.x > 0;

        if (inputSameAsFaceDir) {

            if (Mathf.Abs(currentVelocity.x) > minimumSpeed)
                currentVelocity.x -= floorSlideDesceleration * characterFaceDirection.x * Time.deltaTime;
            else
                currentVelocity.x = minimumSpeed * characterFaceDirection.x;
        }
        else {

            currentVelocity.x = 0;
        }

        playerRb.velocity = currentVelocity;
    }

    /// <summary>
    /// Controls the behaviour of the character for as long as the character is wall sliding.
    /// </summary>
    public void EnterWallSlide() {

        if (wallSlideFlag && playerRb.velocity.y <= 0) {

            playerRb.velocity = Vector3.zero;
            playerRb.gravityScale = 0;
            wallSlideFlag = false;
        }

        Vector3 currentVelocity = playerRb.velocity;
        currentVelocity.y -= wallSlideAcceleration * wallSlideAcceleration * Time.deltaTime;

        playerRb.velocity = currentVelocity;
    }

    /// <summary>
    /// Resets certain values when the character exits the wall slide.
    /// </summary>
    public void ExitWallSlide() {

        playerRb.gravityScale = gravityScale;
        wallSlideFlag = true;
    }
}
