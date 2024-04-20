using UnityEngine;

public class PlayerMovement : MonoBehaviour
{
    public Animator animator;

    void Update()
    {
        if (Input.GetKey(KeyCode.RightArrow))
        {
            animator.SetFloat("State", 1);
        }
        else if (Input.GetKey(KeyCode.LeftArrow))
        {
            animator.SetFloat("State", 1);
        }
        else if (Input.GetKey(KeyCode.UpArrow))
        {
            animator.SetFloat("State", 2);
        }
        else
        {
            animator.SetFloat("State", 0);
        }
    }
}