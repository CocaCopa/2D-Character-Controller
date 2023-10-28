using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AIController : HumanoidController {

    [SerializeField] private Transform playerTransform;

    protected override void Update() {
        base.Update();
        Vector3 moveDirection = (playerTransform.position - transform.position).normalized;
        Vector3 moveInput = moveDirection.x > 0 ? new Vector3(1, 0) : new Vector3(-1, 0);
        ChangeHorizontalVelocity(moveInput);
        FlipCharacter(moveInput.x);
    }
}
