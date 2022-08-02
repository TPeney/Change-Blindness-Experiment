using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using UXF;

public class TrialRunner : MonoBehaviour
{
    [Header("Non-stimuli Trial GameObjects")]
    [SerializeField] GameObject fixationCross;
    [SerializeField] GameObject responsePrompt;
    [SerializeField] GameObject blanker;

    GameObject targetObject;
    float RT;
    string sideResponse;
    bool trialPassed;

    // Cached References from Start()
    Transform stimuliHolder;
    List<GameObject> StimuliList;

    bool AwaitingResponse = false;

    private void Start()
    {
        stimuliHolder = GameObject.Find("SpawnedStimuli").transform;
        StimuliList = FindObjectOfType<SessionBuilder>().MainStimuliList;
    }

    // Invoked via the UXF OnTrialBegin Event - Prepares and then starts the trial
    public void BeginTrial()
    {
        if (!Session.instance.InTrial) { return; }
        LoadTarget();
        LoadStimuli();
        StartCoroutine(MainTrialLoop());
    }

    // Run the logic for a single trial
    private IEnumerator MainTrialLoop()
    {
        float stimuliDisplayDuration = Session.instance.settings.GetFloat("display_duration");
        float hideDuration = Session.instance.settings.GetFloat("hide_duration");
        float interTrialTime = Session.instance.settings.GetFloat("time_between_trials");

        string trialType = Session.instance.CurrentTrial.settings.GetString("trial_type");

        yield return new WaitForSecondsRealtime(interTrialTime);
        yield return StartCoroutine(ShowFixation());

        switch (trialType)
        {
            case "onset":
                ShowStimuli(true, incTarget: false);
                break;
            default:
                ShowStimuli(true);
                break;
        }

        yield return new WaitForSecondsRealtime(stimuliDisplayDuration);
        
        HideScene(true);

        switch (trialType)
        {
            case "onset":
                ShowStimuli(true, incTarget: true);
                break;
            case "luminance":
                // Add colour change logic here
                break;
            default:
                break;
        }

        yield return new WaitForSecondsRealtime(hideDuration);

        HideScene(false);


        yield return new WaitForSecondsRealtime(stimuliDisplayDuration);

        yield return StartCoroutine(AwaitResponse());

        Cleanup();
    }

    // Load target into scene and assign it target-related properties
    private void LoadTarget()
    {
        GameObject target = (GameObject)Session.instance.CurrentTrial.settings["target"];
        Vector3 targetLocation = (Vector3)Session.instance.CurrentTrial.settings["targetLocation"];

        targetObject = Instantiate(target, stimuliHolder);
        targetObject.tag = "Target";
        targetObject.transform.position = targetLocation;
        targetObject.gameObject.name = "Target";
        targetObject.GetComponent<SpriteRenderer>().sortingOrder = 10;
        targetObject.GetComponent<SpriteRenderer>().enabled = false;
    }

    // Create the remaining stimuli from the stimuli list
    private void LoadStimuli()
    {
        foreach (GameObject stimuli in StimuliList)
        {
            if (stimuli != targetObject)
            {
                GameObject stimuliObject = Instantiate(stimuli, stimuliHolder);
                stimuliObject.GetComponent<SpriteRenderer>().enabled = false;
            }
        }
    }

    // Toggles the Fixation Point on for a duration (Set in settings)
    private IEnumerator ShowFixation()
    {
        float fixateDuration = Session.instance.settings.GetFloat("fixate_duration");

        fixationCross.SetActive(true);
        yield return new WaitForSecondsRealtime(fixateDuration);
        fixationCross.SetActive(false);
    }

    // Toggles the rendering of the stimuli (and target if included)
    private void ShowStimuli(bool toggle, bool incTarget = true)
    {
        foreach (Transform stimuli in stimuliHolder)
        {
            if (!incTarget && stimuli.tag == "Target") { continue; }
            else
            {
                stimuli.GetComponent<SpriteRenderer>().enabled = toggle;
            }
        }
    }

    // Hides/un-hides the scene based on a given bool
    private void HideScene(bool toggle)
    {
        blanker.SetActive(toggle);
    }

    // Allows a response to be provided for a given duration
    private IEnumerator AwaitResponse()
    {
        float maxTimeToRespond = Session.instance.settings.GetFloat("response_duration");
        float start, end;

        start = Time.time;
        ShowStimuli(false);
        responsePrompt.SetActive(true);
        AwaitingResponse = true;

        while (AwaitingResponse)
        {
            float duration = Time.time - start;
            if (duration >= maxTimeToRespond)
            {
                sideResponse = "None";
                AwaitingResponse = false;
                SaveResults();
                yield return null;
            }
            yield return null;
        }
        end = Time.time;
        RT = end - start;
    }

    // Handles a given response - saves trial data 
    public void HandleResponse(InputAction.CallbackContext value)
    {
        if (!Session.instance.InTrial || !value.started || !AwaitingResponse) { return; }
        
        PlayerInput controls = GetComponent<PlayerInput>();
        if (value.action == controls.actions.FindAction("RespondLeft"))
        {
            sideResponse = "Left";
        }
        else if (value.action == controls.actions.FindAction("RespondRight"))
        {
            sideResponse = "Right";
        }


        AwaitingResponse = false;
        responsePrompt.SetActive(false);

        SaveResults();
    }

    private void SaveResults()
    {
        Trial currentTrial = Session.instance.CurrentTrial;

        string targetSide = Session.instance.CurrentTrial.settings.GetString("targetSide");
        trialPassed = sideResponse == targetSide ? true : false;

        currentTrial.result["RT"] = RT;
        currentTrial.result["sideResponse"] = sideResponse;
        currentTrial.result["targetSide"] = targetSide;
        currentTrial.result["trialPassed"] = trialPassed;
    }

    // Destroys all trial-specific objects and ends the trial
    private void Cleanup()
    {
        foreach (Transform stimuli in stimuliHolder)
        {
            Destroy(stimuli.gameObject);
        }
        StopAllCoroutines();
        Session.instance.EndCurrentTrial();
    }
}