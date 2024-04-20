using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PenguinAnimate : MonoBehaviour
{
    public Animator animator;
    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
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
