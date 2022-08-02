using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class StimuliSpawn : MonoBehaviour
{
    Transform stimuliSpawnArea;
    Vector3 origin, range;
    
    bool placed = false;

    private void Start()
    {
        stimuliSpawnArea = GameObject.Find("StimuliSpawnArea").transform;
        origin = stimuliSpawnArea.position;
        range = stimuliSpawnArea.localScale / 2.0f; // Div 2 to give + and - from center point
    }

    private void Update()
    {
        if (this.tag != "Target")
        {
            while (!placed)
            {
                AttemptToPlace();
            }
        }
    }

    private void OnTriggerStay2D(Collider2D collision)
    {
        AttemptToPlace();
    }

    private Vector3 SelectRandomPosition()
    {
        Vector3 randomRange = new Vector3(Random.Range(-range.x, range.x),
                                          Random.Range(-range.y, range.y),
                                          0);
        
        Vector3 randomCoordinates = origin + randomRange;

        return randomCoordinates;
    }

    private void AttemptToPlace()
    {
        Vector3 coordinates = SelectRandomPosition();
        Vector2 checkArea = new Vector2(this.transform.localScale.x, this.transform.localScale.y);
        Collider2D hit = Physics2D.OverlapBox(coordinates, checkArea, 0);
        if (!hit)
        {
            this.transform.position = coordinates;
            placed = true;
        }
    }
}
