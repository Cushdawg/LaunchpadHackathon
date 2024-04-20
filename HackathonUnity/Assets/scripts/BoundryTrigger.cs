using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BoundryTrigger : MonoBehaviour
{
    public LogicScript script;
    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        int randomX = Random.Range(-30, 30);
        collision.gameObject.transform.SetPositionAndRotation(new Vector3(randomX, 10, 0), transform.rotation);
        //script.restartGame();
    }
}
