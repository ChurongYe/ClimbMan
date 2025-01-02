using StarterAssets;
using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using static StarterAssets.Player;

public class Animationstop : MonoBehaviour
{
    public GameObject Player;
    public Animator animator;
    public bool JumpPaused = false;
    public bool ClimbPaused = false;
    public bool RopePaused = false;
    private PlayerState State;
    private bool IfIdle;
    // Start is called before the first frame update
    void Start()
    {
        animator.Play("Idle");
    }

    // Update is called once per frame
    void Update()
    {
        State = Player.GetComponent<Player>().State;
        IfIdle = Player.GetComponent<Player>().IfIdle;
        if (State == StarterAssets.Player.PlayerState.Move && !IfIdle )
        {
            animator.Play("Walk");
            animator.speed = 1;
        }
        else if(State == StarterAssets.Player.PlayerState.Move && IfIdle)
        {
            animator.Play("Idle");
            animator.speed = 1;
        }
        if (State == StarterAssets.Player.PlayerState.Jump)
        {
            animator.Play("Jump");
            animator.speed = 1;
            if (JumpPaused)
            {
                animator.speed = 0;
            }
        }
        if(State == StarterAssets.Player.PlayerState.Fall )
        {
            animator.speed = 1;
            JumpPaused = false;
        }
        if (State == StarterAssets.Player.PlayerState.Climb)
        {
            if (Player.GetComponent<Player>().IfclimbingOn || Player.GetComponent<Player>().IfclimbingOff)
            {
                animator.Play("Climb");
                animator.speed = 1.5f;
            }
            else
            {
                animator.Play("ClimbMove");
                if (ClimbPaused)
                {
                    animator.speed = 0;
                }
                else
                {
                    animator.speed = 1;
                }
                if (Player.GetComponent<Player>().IfClimbmove)
                {
                    ClimbPaused = false;
                }
                else
                {
                    ClimbPaused = true;
                }
            }

        }
        if (State == StarterAssets.Player.PlayerState.Throw )
        {
            animator.Play("Rope");
            if (RopePaused)
            {
                animator.speed = 0;
            }
            else
            {
                animator.speed = 2;
            }
            if (Player.GetComponent<Player>().IfMove)
            {
                RopePaused = false;
            }
            else
            {
                RopePaused = true;
                animator.speed = 1;
            }
        }
        if (State == StarterAssets.Player.PlayerState.Jumpdown)
        {
            animator.Play("JumpDown");
            animator.speed = 1;

        }
    }
    public void OnAnimatorEvent1()
    {
        animator.Play("Idle");
        animator.speed = 1;
    }
    public void OnAnimatorEvent2()
    {
        JumpPaused = true;
    }
}
