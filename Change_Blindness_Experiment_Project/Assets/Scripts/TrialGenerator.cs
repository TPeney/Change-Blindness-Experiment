using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UXF;

// On Scene Load - Adds the trial info for the current block based on Inspector settings
public class TrialGenerator : MonoBehaviour
{
    [Header("Experiment Stimuli")]
    [Tooltip("List all stimuli prefabs here")]
    [SerializeField] public List<GameObject> MainStimuliList = new List<GameObject>();

    [Tooltip("A list of transforms representing the desired potential spawn points for the target")]
    [SerializeField] private List<Transform> TargetSpawnPoints;

    private void Awake()
    {
        Block loadedBlock = SessionController.instance.currentBlock;
        {
            CreateTrials(loadedBlock);
        }
    }

    // Adds the trial information for a given block
    // Each pair of trial types shares a target and the targets spawn location
    private void CreateTrials(Block condition)
    {
        int n_trials = Session.instance.settings.GetInt("n_trials");

        GameObject target = null;
        Transform selectedSpawnPoint = null;
       
        bool selectNewTarget = true;

        foreach (Trial trial in condition.trials)
        {
            if (selectNewTarget)
            {
                MainStimuliList.Shuffle();
                target = MainStimuliList[0];

                TargetSpawnPoints.Shuffle();
                selectedSpawnPoint = TargetSpawnPoints[0];
            }

            switch (selectNewTarget)
            {
                case true:
                    trial.settings.SetValue("trial_type", "onset");
                    selectNewTarget = false;
                    break;
                case false:
                    trial.settings.SetValue("trial_type", "luminance");
                    selectNewTarget = true;
                    break;
            }

            // Assign Target Values to each trial
            trial.settings.SetValue("target", target);
            trial.settings.SetValue("targetLocation", selectedSpawnPoint.position);
            trial.settings.SetValue("targetSide", selectedSpawnPoint.tag);
        }
        condition.trials.Shuffle();
    }
    

}
