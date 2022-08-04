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
        if (n_trials % 2 != 0) { Debug.LogError("Trial count must be an even number."); }

        CreateBlocks();
        OrderBlocks();

        foreach (Block block in Session.instance.blocks)
        {
            CreateTrials(block);
        }
    }

    private static void CreateBlocks()
    {
        Block block_RL = Session.instance.CreateBlock();
        Block block_VRL = Session.instance.CreateBlock();
        Block block_VRH = Session.instance.CreateBlock();
        Block block_VRG = Session.instance.CreateBlock();

        block_RL.settings.SetValue("tag", "RL");
        block_VRL.settings.SetValue("tag", "VRL");
        block_VRH.settings.SetValue("tag", "VRH");
        block_VRG.settings.SetValue("tag", "VRG");
    }

    // Re-orders the blocks in Session based upon the order given in the startup UI
    private void OrderBlocks()
    {
        string condition1 = (string)Session.instance.participantDetails["condition1"];
        string condition2 = (string)Session.instance.participantDetails["condition2"];
        string condition3 = (string)Session.instance.participantDetails["condition3"];
        string condition4 = (string)Session.instance.participantDetails["condition4"];

        List<Block> blockList = new List<Block>(Session.instance.blocks);
        foreach (Block block in blockList)
        {
            string blockType = block.settings.GetString("tag");

            if (blockType == condition1)
            {
                Session.instance.blocks[0] = block;
            }
            else if (blockType == condition2)
            {
                Session.instance.blocks[1] = block;
            }
            else if (blockType == condition3)
            {
                Session.instance.blocks[2] = block;
            }
            else if (blockType == condition4)
            {
                Session.instance.blocks[3] = block;
            }
        }
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