using UnityEngine;

public class AnimatePenguin : MonoBehaviour
{
    Animator animator;

    void Start()
    {
        animator = GetComponent<Animator>();
    }

    void Update()
    {
        bool jumpPressed = Input.GetButtonDown("Jump"); // Corrected input checking for "Jump"
        bool shouldSlide = jumpPressed; // This is a placeholder, set your own condition for sliding

        animator.SetBool("slide", shouldSlide); // We set "slide" according to the condition

        // If you need to handle other states like jumping or idle, you should add them here
        // For example:
        // bool jumpPressed = Input.GetButtonDown("Jump");
        // animator.SetBool("jump", jumpPressed);
    }
}

