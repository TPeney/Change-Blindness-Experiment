using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraSwap : MonoBehaviour
{
    [SerializeField] int waitTime;

    void Start()
    {
        StartCoroutine(ChangeCamera());
    }

    private IEnumerator ChangeCamera()
    {
        yield return new WaitForSecondsRealtime(waitTime);
        gameObject.SetActive(false);
    }

   
}
