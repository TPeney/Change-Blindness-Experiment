using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ShowGizmo : MonoBehaviour
{
    [SerializeField] Color GizmoColor = new Color(0, 1, 0, 0.5f);

    private void OnDrawGizmos()
    {
        Gizmos.color = GizmoColor;
        Gizmos.DrawCube(transform.position, transform.lossyScale);
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.name == "ghost")
        {
            other.tag = "hit";
        }
    }
}
