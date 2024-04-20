using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PenguinAnimate : MonoBehaviour
{
    public Animator animator;
    int idle = 0;
    int slide = 1;
    int jump = 2;
    [SerializeField] private Transform groundCheck;
    [SerializeField] private LayerMask groundLayer;
    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        if (Input.GetKey(KeyCode.RightArrow) && IsGrounded()) 
        {
            animator.SetInteger("state", slide);
        } 
        
        else if (Input.GetKey(KeyCode.LeftArrow) && IsGrounded()) 
        {
            animator.SetInteger("state", slide);
        } 
        else if (Input.GetKey(KeyCode.Space)) 
        {
            animator.SetInteger("state", idle);
        } 
        else if (IsGrounded())
        {
            animator.SetInteger("state", idle);
        }
    }

    private bool IsGrounded()
    {
        return Physics2D.OverlapCircle(groundCheck.position, 0.2f, groundLayer);
    }
}
