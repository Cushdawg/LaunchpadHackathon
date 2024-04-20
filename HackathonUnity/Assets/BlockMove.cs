using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BlockMove : MonoBehaviour
{
    public float deadZone = -15;
    public float moveSpeed = 1;
    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        if (transform.position.y < deadZone)
        {
            Destroy(gameObject);
        }
        transform.position = transform.position + (Vector3.down * moveSpeed) * Time.deltaTime;
    }
}
