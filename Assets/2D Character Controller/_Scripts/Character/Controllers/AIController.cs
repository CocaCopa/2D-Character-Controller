using CocaCopa;
using UnityEngine;

public class AIController : HumanoidController {

    [SerializeField] private Transform playerTransform;

    /*float timer;
    float time = 2f;*/

    protected override void Update() {
        base.Update();
        /*Vector3 moveDirection = (playerTransform.position - transform.position).normalized;
        Vector3 moveInput = moveDirection.x > 0 ? new Vector3(1, 0) : new Vector3(-1, 0);
        
        ChangeHorizontalVelocity(moveInput);
        FlipCharacter(moveInput.x);
        LedgeGrab();

        if (Utilities.TickTimer(ref timer, time)) {
            TryJumping();
        }*/

        Vector3 lookDirection = (playerTransform.position - transform.position).normalized;
        Vector3 eulerAngles = lookDirection.x > 0 ? new(0, 0, 0) : new(0, 180, 0);
        transform.eulerAngles = eulerAngles;
    }
}
