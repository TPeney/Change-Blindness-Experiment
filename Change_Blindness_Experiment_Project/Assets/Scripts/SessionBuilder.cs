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

    public void GenerateExperiment(Session session)
    {
        // Generate Blocks and Trials Here 
        int n_trials = Session.instance.settings.GetInt("n_trials");
        Block mainBlock = session.CreateBlock(n_trials);

        // Assign each trial a target based on the given list of stimuli (currently at random)
        foreach (Trial trial in mainBlock.trials)
        {
            MainStimuliList.Shuffle();
            trial.settings.SetValue("target", MainStimuliList[0]);

            TargetSpawnPoints.Shuffle();
            Transform SelectedSpawnPoint = TargetSpawnPoints[0];
            trial.settings.SetValue("targetLocation", SelectedSpawnPoint.position);
            trial.settings.SetValue("targetSide", SelectedSpawnPoint.tag);
            trial.settings.SetValue("trial_type", "onset");

            
        }
    }
}