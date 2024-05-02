using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerAnimController : MonoBehaviour
{
    public Animator anim;

    private int speedHash;
    private int isJumpingHash;
    private int isAscendingHash;
    private int isFallingHash;
    private void Start()
    {
        speedHash = Animator.StringToHash("speed");
        isJumpingHash = Animator.StringToHash("isJumping");
        isAscendingHash = Animator.StringToHash("isAscending");
        isFallingHash = Animator.StringToHash("isFalling");
    }

    public void SetMoveSpeed(float speed)
    {
        anim.SetFloat(speedHash, speed);
    }

    public void SetJumpingState(bool isJumping)
    {
        anim.SetBool(isJumpingHash, isJumping);
    }

    public void SetAscendingState(bool isAscending)
    {
        anim.SetBool(isAscendingHash, isAscending);
    }

    public void SetFallingState(bool isFalling)
    {
        anim.SetBool(isFallingHash, isFalling);
    }
}
