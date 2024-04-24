using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerAnimController : MonoBehaviour
{
    public Animator anim;

    private int speedHash;
    private int isJumpingHash;
    private int isAscendingHash;

    private void Start()
    {
        speedHash = Animator.StringToHash("speed");
        isJumpingHash = Animator.StringToHash("isJumping");
        isAscendingHash = Animator.StringToHash("isAscending");
    }

    public void SetMoveSpeed(float speed)
    {
        anim.SetFloat(speedHash, speed, 0.1f, Time.deltaTime);
    }

    public void SetJumpingState(bool isJumping)
    {
        anim.SetBool(isJumpingHash, isJumping);
    }

    public void SetAscendingState(bool isAscending)
    {
        anim.SetBool(isAscendingHash, isAscending);
    }
}
