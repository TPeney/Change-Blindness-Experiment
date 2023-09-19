using UnityEngine;
using UnityEngine.InputSystem;

// Script to reset XRrigs back to their starting points
public class PositionReset : MonoBehaviour
{
    [SerializeField] Transform resetTransform;
    GameObject player;
    Camera playerHead;

    private void Awake()
    {
        player = gameObject;
        playerHead = GetComponentInChildren<Camera>();
    }

    public void ResetView()
    {
        if (!gameObject.activeInHierarchy) { return; }

        // Reset position
        var distanceDiff = resetTransform.position -
            playerHead.transform.position;
        player.transform.position += distanceDiff;

        // Reset rotation
        var rotationAngleY = resetTransform.rotation.eulerAngles.y - playerHead.transform.rotation.eulerAngles.y;
        player.transform.Rotate(0, rotationAngleY, 0);
    }
}
