using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using UXF;

public class TrialRunner : MonoBehaviour
{
    // Visual elements set from inspector
    [Header("Non-stimuli Trial GameObjects")]
    [SerializeField] GameObject fixationCross;
    [SerializeField] GameObject responsePrompt;
    [SerializeField] GameObject blanker;

    // Variables relating to trial results
    GameObject targetObject;
    double start, end, RT;
    string sideResponse;
    bool trialPassed;

    // Cached references from Start()
    Transform stimuliHolder;
    List<GameObject> StimuliList;
    GameObject background;
    PlayerInput controls;

    // States
    bool AwaitingResponse = false;

    private void Start()
    {
        stimuliHolder = GameObject.Find("SpawnedStimuli").transform;
        StimuliList = GetComponent<TrialGenerator>().MainStimuliList;
        background = GameObject.Find("WindowBackground");
        controls = SessionController.instance.controls;
    }

    // Invoked via the UXF OnTrialBegin Event - Prepares and then starts the trial
    public void BeginTrial()
    {
        Session.instance.NextTrial.Begin();                 
        if (!Session.instance.InTrial) { Debug.Log("Not in trial"); }
        //Debug.Log(Session.instance.CurrentBlock.settings.GetString("tag"));
        //Debug.Log(Session.instance.currentTrialNum);

        StartCoroutine(MainTrialLoop());
    }

    // Run the logic for a single trial
    private IEnumerator MainTrialLoop()
    {
        float interTrialTime = Session.instance.settings.GetFloat("time_between_trials");
        float stimuliDisplayDuration = Session.instance.settings.GetFloat("display_duration");
        float hideDuration = Session.instance.settings.GetFloat("hide_duration");
        string trialType = Session.instance.CurrentTrial.settings.GetString("trial_type");

        Debug.Log(trialType);

        LoadTarget();
        yield return StartCoroutine(LoadStimuli());

        yield return new WaitForSecondsRealtime(interTrialTime);

        yield return StartCoroutine(ShowFixation());
        LoadStimuliForTrialType(trialType);
        yield return new WaitForSecondsRealtime(stimuliDisplayDuration);
        
        HideScene(true);
        ApplyTrialTypeManipulation(trialType);
        yield return new WaitForSecondsRealtime(hideDuration);
        HideScene(false);

        yield return new WaitForSecondsRealtime(stimuliDisplayDuration);

        yield return StartCoroutine(AwaitResponse());

        Cleanup();
    }

    // Load target into scene and assign it target-related properties
    private void LoadTarget()
    {
        GameObject target = (GameObject)Session.instance.CurrentTrial.settings.GetObject("target");
        Vector3 targetLocation = (Vector3)Session.instance.CurrentTrial.settings.GetObject("targetLocation");

        targetObject = Instantiate(target, stimuliHolder);
        targetObject.tag = "Target";
        targetObject.transform.position = targetLocation;
        targetObject.gameObject.name = "Target";
        targetObject.GetComponent<Renderer>().sortingOrder = 10;
        targetObject.GetComponent<Renderer>().enabled = false;
    }

    // Create the remaining stimuli from the stimuli list
    private IEnumerator LoadStimuli()
    {
        foreach (GameObject stimuli in StimuliList)
        {
            if (stimuli != targetObject)
            {
                GameObject stimuliObject = Instantiate(stimuli, stimuliHolder);
                stimuliObject.GetComponent<Renderer>().enabled = false;
                StimuliSpawn spawnScript = stimuliObject.GetComponent<StimuliSpawn>();
                while (!spawnScript.placed)
                {
                    if (!spawnScript.attemptingToPlace)
                    {
                        yield return StartCoroutine(spawnScript.AttemptToPlace());
                    }
                }
            }
        }
    }

    // Manipulates the stimuli prior to display based on the trial type
    private void LoadStimuliForTrialType(string trialType)
    {
        switch (trialType)
        {
            case "onset":
                ShowStimuli(true, incTarget: false);
                break;
            default:
                ShowStimuli(true);
                break;
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
                stimuli.GetComponent<Renderer>().enabled = toggle;
            }
        }
    }

    // Hides/un-hides the scene based on a given bool
    private void HideScene(bool toggle)
    {
        blanker.SetActive(toggle);
    }

    // Manipulates the stimuli based on the trial type
    private void ApplyTrialTypeManipulation(string trialType)
    {
        switch (trialType)
        {
            case "onset":
                ShowStimuli(true, incTarget: true);
                break;

            case "luminance": // Needs adjusting
                Color backgroundColor = background.GetComponent<MeshRenderer>().material.color;
                Color originalColor = targetObject.GetComponent<Renderer>().material.color;
                Color newColor = backgroundColor - originalColor;
                targetObject.GetComponent<Renderer>().material.color = newColor;
                break;

            default:
                break;
        }
    }

    // Allows a response to be provided for a given duration
    private IEnumerator AwaitResponse()
    {
        float maxTimeToRespond = Session.instance.settings.GetFloat("response_duration");

        ShowStimuli(false);
        responsePrompt.SetActive(true);
        AwaitingResponse = true;
        start = Time.realtimeSinceStartupAsDouble;

        while (AwaitingResponse)
        {
            double duration = Time.realtimeSinceStartupAsDouble - start;
            if (duration >= maxTimeToRespond)
            {
                HandleResponse(timedOut: true);
            }
            yield return null;
        }
    }

    // Handles a given response - saves trial data 
    public void HandleResponse(InputAction.CallbackContext value)
    {
        if (!Session.instance.InTrial || !value.started || !AwaitingResponse) { return; }
        
        if (value.action == controls.actions.FindAction("RespondLeft"))
        {
            sideResponse = "Left";
        }
        else if (value.action == controls.actions.FindAction("RespondRight"))
        {
            sideResponse = "Right";
        }
        else
        {
            return;
        }

        AwaitingResponse = false;
        end = Time.realtimeSinceStartupAsDouble;
        responsePrompt.SetActive(false);

        SaveResults();
    }

    public void HandleResponse(bool timedOut)
    {
        if (!Session.instance.InTrial || !AwaitingResponse) { return; }

        sideResponse = "";
        AwaitingResponse = false;
        end = Time.realtimeSinceStartupAsDouble;
        responsePrompt.SetActive(false);

        SaveResults();
    }

    // Saves the trial data to the UXF results system
    private void SaveResults()
    {
        Trial currentTrial = Session.instance.CurrentTrial;
        
        string blockTag = Session.instance.CurrentBlock.settings.GetString("tag");
        currentTrial.result["Condition"] = blockTag;

        string targetSide = Session.instance.CurrentTrial.settings.GetString("targetSide");
        currentTrial.result["targetSide"] = targetSide;
        currentTrial.result["sideResponse"] = sideResponse;

        trialPassed = sideResponse == targetSide ? true : false;
        currentTrial.result["trialPassed"] = trialPassed;

        RT = end - start;
        currentTrial.result["RT"] = RT;
    }

    // Destroys all trial-specific objects and ends the trial
    private void Cleanup()
    {
        foreach (Transform stimuli in stimuliHolder)
        {
            Destroy(stimuli.gameObject);
        }
        StopAllCoroutines();
        Session.instance.CurrentTrial.End();
        // TODO Show end screen if last trial - wait for response before contuining
        SessionController.instance.EndOfTrialCheck();
    }
}