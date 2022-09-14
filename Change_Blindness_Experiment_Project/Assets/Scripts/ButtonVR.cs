using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ButtonVR : MonoBehaviour
{
    public GameObject button;
    public GameObject hintText;
    AudioSource sound;
    public bool isPressed;
    Material buttonR;
    Color buttonGlow;

    // Start is called before the first frame update  
    void Start()
    {
        sound = GetComponent<AudioSource>();
        isPressed = false;
        buttonR = GameObject.Find("Press").GetComponent<Renderer>().material;
        buttonGlow = buttonR.GetColor("_EmissionColor");
    }

    private void OnTriggerEnter(Collider other)
    {
        if (!isPressed)
        {
            button.transform.localPosition = new Vector3(0, 0.003f, 0);
            sound.Play();
            buttonR.SetColor("_EmissionColor", Color.black);
            hintText.SetActive(false);

            isPressed = true;
        }
    }

    public void ResetButton()
    {
        {
            button.transform.localPosition = new Vector3(0, 0.018f, 0);
            buttonR.SetColor("_EmissionColor", buttonGlow);
            hintText.SetActive(true);
            isPressed = false;
        }
    }
}
