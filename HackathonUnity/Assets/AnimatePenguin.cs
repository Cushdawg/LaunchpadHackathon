using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AnimatePenguin : MonoBehaviour
{
    Animator animate;
    // Start is called before the first frame update
    void Start()
    {
        animate = GetComponent<Animator>();
    }

    // Update is called once per frame
    void Update()
    {
        bool forwardPressed = Input.GetKey("jump");
        if (forwardPressed)
        {
            animator.setBool("slide", true);
        }
    }
}
