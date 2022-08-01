using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class StimuliSpawn : MonoBehaviour
{
    Transform stimuliSpawnArea;
    Vector3 origin, range;

    private void Start()
    {
        stimuliSpawnArea = GameObject.Find("StimuliSpawnArea").transform;
        origin = stimuliSpawnArea.position;
        range = stimuliSpawnArea.localScale / 2.0f; // Div 2 to give + and - from center point

        this.transform.position = SelectRandomPosition();

    }

    private void OnTriggerStay2D(Collider2D collision)
    {
        if (collision.tag == "SpawnAreaBorder")
        {
            Debug.Log(this.name + " overlaps" + collision.name);
            this.transform.position = SelectRandomPosition();
        }
    }

    private Vector3 SelectRandomPosition()
    {
        Vector3 randomRange = new Vector3(Random.Range(-range.x, range.x),
                                                  Random.Range(-range.y, range.y),
                                                  origin.z);
        Vector3 randomCoordinates = origin + randomRange;

        return randomCoordinates;
    }
    
}
