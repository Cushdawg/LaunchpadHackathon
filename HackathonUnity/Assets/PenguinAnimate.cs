using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PenguinAnimate : MonoBehaviour
{
    public Animator animator;
    int idle = 0;
    int slide = 1;
    [SerializeField] private Transform groundCheck;
    [SerializeField] private LayerMask groundLayer;
    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
    }

    public void Slide()
    {
        if (IsGrounded())
        {
            animator.SetInteger("state", slide);
        }
        else
        {
            Idle();
        }
    }

    public void Idle()
    {
        animator.SetInteger("state", idle);
    }

    private bool IsGrounded()
    {
        return Physics2D.OverlapCircle(groundCheck.position, 0.2f, groundLayer);
    }
}
