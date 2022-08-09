using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.InputSystem;
using UXF;
using UnityEngine.XR.Management;

public class SessionController : MonoBehaviour
{
    public static SessionController instance;

    public Block currentBlock;
    public PlayerInput controls;
    private TrialRunner loadedTrialRunner;
    public InfoScreen loadedInfoScreen;

    private int numberOfConditions;
    private int nextBlockIndex = 1;

    // Create Singleton pattern
    private void Awake()
    {
        if (instance == null)
        {
            DontDestroyOnLoad(gameObject);
            instance = this;
        }
        else { Destroy(gameObject); }
        XRGeneralSettings.Instance.Manager.StopSubsystems();
    }

    // Get the next block by index and load related scene using its tag
    public void LoadNextCondition()
    {
        XRGeneralSettings.Instance.Manager.StartSubsystems();

        if (nextBlockIndex > Session.instance.blocks.Count)
        {
            // load end scene
            Application.Quit();
        }
        else
        {   // Identify the next block and load it's scene
            currentBlock = Session.instance.GetBlock(nextBlockIndex);
            nextBlockIndex++;
            string ConditionTag = currentBlock.settings.GetString("tag");
            //if (ConditionTag != "RL")
            //{
            //    XRGeneralSettings.Instance.Manager.StartSubsystems();
            //}
            //else
            //{
            //    XRGeneralSettings.Instance.Manager.StopSubsystems();
            //}

            StartCoroutine(LoadScene(ConditionTag));
        }
    }

    // Asynchronously loads next scene, finding the trial runner and starting
    // the next trial when done
    private IEnumerator LoadScene(string ConditionTag)
    {
        AsyncOperation loadingScene = SceneManager.LoadSceneAsync(ConditionTag);
        // Wait until scene is fully loaded before continuing 
        while (!loadingScene.isDone)
        {
            yield return null;
        }
        // Wait a frame so every Awake and Start method is called 
        yield return new WaitForEndOfFrame();

        // Find the new trial runner & input controller for the scene
        loadedTrialRunner = FindObjectOfType<TrialRunner>();
        controls = FindObjectOfType<PlayerInput>();

        // If there is a StartScreen - show it
        GameObject loadedInfoScreenObject = GameObject.Find("StartScreen");
        if (loadedInfoScreenObject != null)
        {
            loadedInfoScreen = loadedInfoScreenObject.GetComponent<InfoScreen>();
            yield return StartCoroutine(ShowInfoScreen());
        }

        StartTrial();
    }

    // Calls the scene's trialHandler to start a trial
    public void StartTrial()
    {
        loadedTrialRunner.BeginTrial();
    }

    // Called by the scene's trialHandler once the trial is complete
    // Checks whether the next trial is the last in block - if so, toggles to load next scene after
    public void EndOfTrialCheck()
    {
        // If this trial is the last in the block, load the next condition
        if (Session.instance.CurrentTrial == Session.instance.CurrentBlock.lastTrial)
        {
            LoadNextCondition();
        }
        else
        {
            loadedTrialRunner.BeginTrial();
        }
    }

    // Called when an info screen should be shown
    // Yields control back to session controller once a response has been given
    private IEnumerator ShowInfoScreen()
    {
        yield return StartCoroutine(loadedInfoScreen.AwaitResponse());
    }

    // Passes an input to the scene's trialHandler
    public void PassResponse(InputAction.CallbackContext value)
    {
        loadedTrialRunner.HandleResponse(value);
        if (loadedInfoScreen != null) { loadedInfoScreen.HandleResponse(value); }
    }
}
