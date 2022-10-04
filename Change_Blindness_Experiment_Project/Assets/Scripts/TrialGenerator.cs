using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UXF;

// On Scene Load - Adds the trial info for the current block based on Inspector settings
public class TrialGenerator : MonoBehaviour
{
    [Header("Experiment Stimuli")]
    [Tooltip("List all stimuli prefabs here")]
    public List<GameObject> MainStimuliList = new();

    [Tooltip("A list of the materials (colours) to be used by the stimuli - Lowest RGB at index 0")]
    public List<Material> stimuliColours = new();

    [Tooltip("A list of transforms representing the desired potential spawn points for the target")]
    [SerializeField] private List<Transform> TargetSpawnPoints;
    
    readonly List<TrialInfo> trialParameters = new();
    readonly System.Random random = new();

    private void Awake()
    {
        Block loadedBlock = SessionController.instance.currentBlock;
        
        if (loadedBlock.settings.GetString("tag") == "Practice")
        {
            CreatePracticeTrials(loadedBlock);
        }
        else
        {
            int reps = Session.instance.settings.GetInt("n_reps");
            for (int i = 0; i < reps; i++) { CreateTrialParameters(loadedBlock); }
        }
        
        AssignTrials(loadedBlock, trialParameters);

        // DEBUG
        for (int i = 0; i < loadedBlock.trials.Count; i++)
        {
            Debug.Log(loadedBlock.trials[i].settings.GetFloat("hide_duration"));
        }
    }

    // Creates a list of objects holding the parameters for each trial of the current block,
    // a trial exists for each combination of params - transType & targLoc - with the chosen target sharing those params for both
    // an onset and a luminance trial
    private void CreateTrialParameters(Block loadedBlock)
    {
        List<float> ISI_times = BuildISItimes(loadedBlock);
        Debug.Log(ISI_times);
        foreach (Material transientType in stimuliColours)
        {
            // Avoid using mid-grey for target to ensure luminance trials always show a change
            if (transientType.name == "Grey_128") { continue; } 
            foreach (Transform spawnPoint in TargetSpawnPoints) 
            {
                float ISI = ISI_times[0];
                ISI_times.RemoveAt(0);

                TrialInfo onsetTrialInfo = new()
                {
                    SelectedMaterial = transientType,
                    SelectedSpawnPoint = spawnPoint,
                    ISI = ISI
                };

                MainStimuliList.Shuffle();
                onsetTrialInfo.Target = MainStimuliList[0];

                TrialInfo luminanceTialInfo = (TrialInfo) onsetTrialInfo.Clone();

                onsetTrialInfo.TrialType = "onset";
                trialParameters.Add(onsetTrialInfo);

                luminanceTialInfo.TrialType = "luminance";
                trialParameters.Add(luminanceTialInfo);
            }
        }
    }
    
    // Modified trial generation to create n trials with randomised attributes for practice block
    private void CreatePracticeTrials(Block practiceBlock)
    {
        GameObject target = null;
        Transform selectedSpawnPoint = null;
        Material selectedMaterial = null;
        bool selectNewTarget = true;

        foreach (Trial trial in practiceBlock.trials)
        {
            if (selectNewTarget)
            {
                MainStimuliList.Shuffle();
                target = MainStimuliList[0];

                TargetSpawnPoints.Shuffle();
                selectedSpawnPoint = TargetSpawnPoints[0];

                // Give target a random colour that's not the midpoint grey
                do
                {
                    selectedMaterial = stimuliColours[random.Next(stimuliColours.Count)];
                }
                while (selectedMaterial.name == "Grey_128");
            }

            TrialInfo pracTrialInfo = new()
            {
                Target = target,
                SelectedSpawnPoint = selectedSpawnPoint,
                SelectedMaterial = selectedMaterial,
                ISI = 0.1f
            };

            switch (selectNewTarget)
            {
                case true:
                    pracTrialInfo.TrialType = "onset";
                    selectNewTarget = false;
                    break;
                case false:
                    pracTrialInfo.TrialType = "luminance";
                    selectNewTarget = true;
                    break;
            }

            trialParameters.Add(pracTrialInfo);
        }
    }

    // Assign generated trial parameters to each trial object
    private void AssignTrials(Block block, List<TrialInfo> trialParamList)
    {
        for (int i = 0; i < trialParamList.Count; i++)
        {
            Trial trial = block.trials[i];
            TrialInfo p = trialParamList[i];

            trial.settings.SetValue("trial_type", p.TrialType);
            trial.settings.SetValue("target", p.Target);
            trial.settings.SetValue("targetLocation", p.SelectedSpawnPoint);
            trial.settings.SetValue("targetSide", p.SelectedSpawnPoint.tag);
            trial.settings.SetValue("targetColour", p.SelectedMaterial);
            trial.settings.SetValue("hide_duration", p.ISI);
        }

        block.trials.Shuffle();

        // Assign random ISI - DEPRECATED

        //int duration_list_index = -1;
        //int nTrials = block.trials.Count;
        //for (int i = 0; i < nTrials; i++)
        //{
        //    if (i % (nTrials / 4) == 0) 
        //    {
        //        duration_list_index++;
        //    }
        //    block.trials[i].settings.SetValue("hide_duration", hide_durations[duration_list_index]);
        //}
        //block.trials.Shuffle();

    }

    private List<float> BuildISItimes(Block block)
    {
        int nTrials = block.trials.Count;
        int nReps = Session.instance.settings.GetInt("n_reps");
        int multiplier = ((nTrials / nReps) / 2) / 4;
        List<float> hide_duration_values = Session.instance.settings.GetFloatList("hide_durations");
        List<float> hide_durations = new List<float>();

        foreach (float value in hide_duration_values)
        {
            for (int i = 0; i < multiplier; i++)
            {
                hide_durations.Add(value);
            }
        }
        hide_durations.Shuffle();
        return hide_durations;
    }
}

public class TrialInfo
{
    public GameObject Target { get; set; }
    public Transform SelectedSpawnPoint { get; set; }
    public Material SelectedMaterial { get; set; }
    public string TrialType { get; set; }
    public float ISI { get; set; }

    public object Clone()
    {
        return this.MemberwiseClone();
    }
}