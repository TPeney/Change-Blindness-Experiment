using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using TMPro;

public class InfoScreen : MonoBehaviour
{
    PlayerInput controls;

    bool responseReceived = false;

    private void Awake()
    {
        controls = SessionController.instance.controls;
    }

    public IEnumerator AwaitResponse()
    {
        this.GetComponent<TextMeshPro>().enabled = true;
        while (!responseReceived)
        {
            yield return null;
        }
    }

    public void HandleResponse(InputAction.CallbackContext value)
    {
        if (value.action == controls.actions.FindAction("Continue") && !value.started)
        {
            responseReceived = true;
            this.GetComponent<TextMeshPro>().enabled = false;
            SessionController.instance.loadedInfoScreen = null;
        }
    }

}
