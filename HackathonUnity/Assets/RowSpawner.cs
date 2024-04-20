using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RowSpawner : MonoBehaviour
{
    public GameObject[] objects;
    public float spawnRate = 4;
    private float timer = 0;
    public float heightOffset = 10;
    // Start is called before the first frame update
    void Start()
    {
        spawnRow();
    }

    // Update is called once per frame
    void Update()
    {
        if (timer < spawnRate)
        {
            timer += Time.deltaTime;
        }
        else
        {
            spawnRow();
            timer = 0;
        }
    }

    void spawnRow()
    {
        int row = Random.Range(0, objects.Length - 1);

        Instantiate(objects[row], new Vector3(transform.position.x, transform.position.y), transform.rotation);
    }
}
