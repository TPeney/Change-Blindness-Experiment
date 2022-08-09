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
    int maxAttempts = 100;

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

    private void OnTriggerEnter(Collider other)
    {
        if (other.name == "ghost")
        {
            other.tag = "hit";
        }
    }

    private void OnTriggerStay(Collider collision)
    {
        if (collision.tag == "SpawnAreaBorder") {  AttemptToPlace(); }

    }

    private Vector3 SelectRandomPosition()
    {
        Vector3 randomRange;

        if (blockType != "VRG")
        {
            randomRange = new Vector3(Random.Range(-range.x, range.x),
                                      Random.Range(-range.y, range.y),
                                      Random.Range(-0.00001f, -0.008f));
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
        //bool hit;

        GameObject ghost = GameObject.CreatePrimitive(PrimitiveType.Cube);
        ghost.name = "ghost";

        BoxCollider bx = ghost.AddComponent(typeof(BoxCollider)) as BoxCollider;
        bx.isTrigger = true;
        
        Rigidbody rb = ghost.AddComponent(typeof(Rigidbody)) as Rigidbody;
        rb.isKinematic = true;

        ghost.transform.localScale = new Vector3(this.transform.lossyScale.x, this.transform.lossyScale.y, 1);
        ghost.transform.position = coordinates;

        Instantiate(ghost);
        
        Debug.Break();

        Debug.Log(ghost.tag);
        if (ghost.tag != "hit")
        {
            this.transform.position = coordinates;
            placed = true;
        }

        Destroy(ghost);
        //if (blockType != "VRG")
        //{
        //    this.GetComponent<SpriteRenderer>().sortingOrder = sortingOrder;
        //    Vector2 checkArea = new Vector2(this.transform.lossyScale.x, this.transform.lossyScale.y);
        //    Collider2D collision = Physics2D.OverlapBox(coordinates, checkArea, 0);
        //    Debug.Log(collision);
        //    if (collision) { hit = true; } else { hit = false; }
        //}
        //else
        //{
        //    Vector3 checkArea = new Vector3(this.transform.lossyScale.x, this.transform.lossyScale.y, this.transform.lossyScale.z);
        //    hit = Physics.CheckBox(coordinates, checkArea);
        //    Debug.Log(hit);
        //}

        //if (!hit || attempts >= maxAttempts)
        //{
        //    this.transform.position = coordinates;
        //    placed = true;
        //}

        //attempts++;
        //Debug.Log(attempts);
    }
}
