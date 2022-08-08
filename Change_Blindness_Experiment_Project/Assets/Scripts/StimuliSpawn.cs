using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UXF;

public class StimuliSpawn : MonoBehaviour
{
    Transform stimuliSpawnArea;
    Vector3 origin, range;
    string blockType;

    int attempts = 0;
    int maxAttempts = 10;

    bool placed = false;

    private void Start()
    {
        stimuliSpawnArea = GameObject.Find("StimuliSpawnArea").transform;
        origin = stimuliSpawnArea.position;
        range = stimuliSpawnArea.lossyScale / 2f; // Div 2 to give + and - from center point
        blockType = Session.instance.CurrentBlock.settings.GetString("tag");
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
        if (collision.tag == "SpawnAreaBordder") {  AttemptToPlace(); }
    }

    private Vector3 SelectRandomPosition()
    {
        Vector3 randomRange;

        if (blockType != "VRG")
        {
            randomRange = new Vector3(Random.Range(-range.x, range.x),
                                      Random.Range(-range.y, range.y),
                                      0);
        } 
        else
        {
            randomRange = new Vector3(Random.Range(-range.x, range.x),
                                      Random.Range(-range.y, range.y),
                                      Random.Range(-range.z, range.z));
        }

        Vector3 randomCoordinates = origin + randomRange;

        return randomCoordinates;
    }

    private void AttemptToPlace()
    {
        Vector3 coordinates = SelectRandomPosition();
        bool hit;

        if (blockType != "VRG")
        {
            Vector2 checkArea = new Vector2(this.transform.lossyScale.x, this.transform.lossyScale.y);
            Collider2D collision = Physics2D.OverlapBox(coordinates, checkArea, 0);
            if (!collision) { hit = true; } else { hit = false; }
        }
        else
        {
            Vector3 checkArea = new Vector3(this.transform.lossyScale.x, this.transform.lossyScale.y, this.transform.lossyScale.x);
            hit = Physics.CheckBox(coordinates, checkArea);
            Debug.Log(hit);
        }

        if (!hit || attempts >= maxAttempts)
        {
            this.transform.position = coordinates;
            placed = true;
        }

        attempts++;
    }
}
