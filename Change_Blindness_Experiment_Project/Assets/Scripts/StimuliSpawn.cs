using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UXF;

public class StimuliSpawn : MonoBehaviour
{
    Transform stimuliSpawnArea;
    string blockType;

    int attempts = 0;
    int maxAttempts = 20;

    public bool placed = false;
    public bool attemptingToPlace = false;

    private void Awake()
    {
        stimuliSpawnArea = GameObject.Find("StimuliSpawnArea").transform;
        blockType = Session.instance.CurrentBlock.settings.GetString("tag");
    }

    // If an area check ghost is spawned atop an existing placed stimuli, set ghost to acknowledge a hit
    private void OnTriggerEnter(Collider other)
    {
        if (other.name == "ghost")
        {
            other.tag = "hit";
        }
        Debug.Log("I have been entered");
    }

    private void OnTriggerStay(Collider other)
    {
        if (other.CompareTag("SpawnAreaBorder")) 
        {
            StartCoroutine(AttemptToPlace());
        }
    }

    // Returns a random Vector3 from within the defined spawn area
    private Vector3 SelectRandomPosition()
    {
        Vector3 origin = stimuliSpawnArea.position;
        Vector3 range = stimuliSpawnArea.lossyScale / 2f; // Div 2 to give + and - from center point

        Vector3 randomRange;

        if (blockType == "VRG")
        {
            randomRange = new Vector3(Random.Range(-range.x, range.x),
                                      Random.Range(-range.y, range.y),
                                      Random.Range(-range.z, range.z));
        } 
        else // For 2D conditions, Z value is random between two small values to prevent Z-Fighting 
        {
            randomRange = new Vector3(Random.Range(-range.x, range.x),
                                      Random.Range(-range.y, range.y),
                                      Random.Range(-0.00001f, -0.008f));
        }

        Vector3 randomCoordinates = origin + randomRange;

        return randomCoordinates;
    }

    // Attempts to place a stimuli in the scene by first using a 'ghost' GameObject to check if the position is empty
    public IEnumerator AttemptToPlace()
    {
        attemptingToPlace = true;
        Vector3 coordinates = SelectRandomPosition();
        GameObject ghost = CreateGhost(coordinates);
        yield return new WaitForFixedUpdate();

        Debug.Log(ghost.tag);

        if (!ghost.CompareTag("hit") || attempts >= maxAttempts)
        {
            this.transform.position = coordinates;
            placed = true;
        }

        Destroy(ghost);
        attempts++;
        Debug.Log(attempts);
        attemptingToPlace = false;
    }

    private GameObject CreateGhost(Vector3 coordinates)
    {
        GameObject ghost = GameObject.CreatePrimitive(PrimitiveType.Cube);
        Destroy(ghost.GetComponent<MeshRenderer>());
        Destroy(ghost.GetComponent<BoxCollider>());

        ghost.name = "ghost";
        
        ghost.transform.localScale = this.transform.lossyScale;
        BoxCollider bx = ghost.AddComponent<BoxCollider>();
        bx.isTrigger = true;

        Rigidbody rb = ghost.AddComponent(typeof(Rigidbody)) as Rigidbody;
        rb.isKinematic = true;

        Vector3 colliderSize = this.GetComponent<BoxCollider>().bounds.extents;
        Vector3 stimuliSize = this.transform.lossyScale;
        Vector3 checkSize = new Vector3(stimuliSize.x * colliderSize.x, stimuliSize.y * colliderSize.y, stimuliSize.z * colliderSize.z);

        bx.size = this.GetComponent<BoxCollider>().size;
        ghost.transform.position = coordinates;
        return ghost;
    }
}
