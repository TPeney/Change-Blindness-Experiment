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
                                      Random.Range(-0.00001f, -0.001f));
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

        if (!ghost.CompareTag("hit") || attempts >= maxAttempts)
        {
            this.transform.position = coordinates;
            placed = true;
        }

        Destroy(ghost);
        attempts++;
        attemptingToPlace = false;

        yield return null;
    }

    // Creates a temporary GameObject used for checking collision in an area 
    // Using a Physics check might be more efficient? 
    private GameObject CreateGhost(Vector3 coordinates)
    {
        GameObject ghost = GameObject.CreatePrimitive(PrimitiveType.Cube);
        // Destroy the initial collider so the new collider will be adapted to the updated scale
        Destroy(ghost.GetComponent<BoxCollider>());
        Destroy(ghost.GetComponent<MeshRenderer>());

        ghost.name = "ghost";
        
        // Add a colider which is the same dimensions as the current stimuli's collider 
        ghost.transform.localScale = this.transform.lossyScale;
        BoxCollider bx = ghost.AddComponent<BoxCollider>();
        bx.isTrigger = true;
        bx.size = this.GetComponent<BoxCollider>().size;

        Rigidbody rb = ghost.AddComponent(typeof(Rigidbody)) as Rigidbody;
        rb.isKinematic = true;

        ghost.transform.position = coordinates;
        
        return ghost;
    }
}
