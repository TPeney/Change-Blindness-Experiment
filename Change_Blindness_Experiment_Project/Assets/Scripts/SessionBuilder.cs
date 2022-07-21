using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UXF;
using UnityEngine.InputSystem;

public class SessionBuilder : MonoBehaviour
{
    public Transform stimuli;
    public GameObject blanker;
    private List<Transform> _stimuliList = new List<Transform>();

    // Start is called before the first frame update
    void Start()
    {
        foreach (Transform child in stimuli)
        {
            _stimuliList.Add(child);
        }
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void GenerateExperiment(Session session)
    {
        // Generate Blocks Here 
        int n_trials = Session.instance.settings.GetInt("n_trials");
        Block mainBlock = session.CreateBlock(n_trials);

        // Assign each trial a target based on the given list of stimuli (currently at random)
        List<Transform> stimuliList = new List<Transform>(_stimuliList);
        foreach (Trial trial in mainBlock.trials)
        {
            int index = Random.Range(0, stimuliList.Count);
            trial.settings.SetValue("target", stimuliList[index].name);
            stimuliList.Remove(stimuliList[index]);
        }
    }

    // Handles showing / hiding the target as well as showing the blanker
    // Invoked with 'On Trial Begin' Event
    public void PresentStimuli()
    {
        string targetName = Session.instance.CurrentTrial.settings.GetString("target");
        Transform target = stimuli.Find(targetName);

        StartCoroutine(FlickerStimuli(target));
    }

    IEnumerator FlickerStimuli(Transform target)
    {
        int displayDuration = Session.instance.settings.GetInt("display_duration");
        float flickerTime = Session.instance.settings.GetFloat("flicker_time");

        stimuli.gameObject.SetActive(false);
        yield return new WaitForSecondsRealtime(displayDuration); // change to setting
        stimuli.gameObject.SetActive(true);

        while (Session.instance.InTrial)
        {
            yield return new WaitForSecondsRealtime(displayDuration);
            blanker.SetActive(true);
            target.gameObject.SetActive(false);
            yield return new WaitForSecondsRealtime(flickerTime);
            blanker.SetActive(false);

            yield return new WaitForSecondsRealtime(displayDuration);
            blanker.SetActive(true);
            target.gameObject.SetActive(true);
            yield return new WaitForSecondsRealtime(flickerTime);
            blanker.SetActive(false);
        }
        target.gameObject.SetActive(true);
        Debug.Log("Stimuli Presentation Over");
    }

    public void HandleResponse(InputAction.CallbackContext value)
    {
        if (Session.instance.InTrial && value.started)
        {
            Session.instance.EndCurrentTrial();
            Debug.Log("Space Pressed in trial");
            StopAllCoroutines();
            BeginNext();
        }
    }
    
    
    void BeginNext()
    {
        Session.instance.BeginNextTrial();
    }
}
