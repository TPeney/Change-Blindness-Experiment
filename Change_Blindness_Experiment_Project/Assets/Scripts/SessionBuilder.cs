using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UXF;
using UnityEngine.InputSystem;

public class SessionBuilder : MonoBehaviour
{
    [Header("Experiment Stimuli")]
    [Tooltip("List all stimuli prefabs here")]
    [SerializeField] public List<GameObject> MainStimuliList = new List<GameObject>();

    [Tooltip("A list of transforms representing the desired potential spawn points for the target")]
    [SerializeField] private List<Transform> TargetSpawnPoints;

    int n_trials;

    public void GenerateExperiment(Session session)
    {
        n_trials = Session.instance.settings.GetInt("n_trials");

        // Trial count must be even as half will be onset and the other half luminance
        if (n_trials % 2 != 0)
        {
            Debug.LogError("Trial count must be an even number.");
        }

        Block realWorldBlock = session.CreateBlock();

        CreateTrials(realWorldBlock);
    }

    // Creates and adds one of each trial type to a given block
    // Each pair of trial types shares a target and the targets spawn location
    private void CreateTrials(Block condition)
    {
        for (int i = 0; i < n_trials / 2; i++)
        {
            Trial onsetTrial = condition.CreateTrial();
            onsetTrial.settings.SetValue("trial_type", "onset");

            Trial luminanceTrial = condition.CreateTrial();
            luminanceTrial.settings.SetValue("trial_type", "luminance");

            MainStimuliList.Shuffle();
            GameObject target = MainStimuliList[0];

            TargetSpawnPoints.Shuffle();
            Transform SelectedSpawnPoint = TargetSpawnPoints[0];

            AssignTrialData(onsetTrial, target, SelectedSpawnPoint);
            AssignTrialData(luminanceTrial, target, SelectedSpawnPoint);
        }

        condition.trials.Shuffle();
    }

    // Helper method to assign the data defined in CreateTrials to each pair of trial types
    private void AssignTrialData(Trial trial, GameObject target, Transform selectedSpawnPoint)
    {
        trial.settings.SetValue("target", target);
        trial.settings.SetValue("targetLocation", selectedSpawnPoint.position);
        trial.settings.SetValue("targetSide", selectedSpawnPoint.tag);
    }
}