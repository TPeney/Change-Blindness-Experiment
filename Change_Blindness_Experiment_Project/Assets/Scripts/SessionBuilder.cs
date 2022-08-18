using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.SceneManagement;
using UXF;

public class SessionBuilder : MonoBehaviour
{
    string condition1;
    string condition2;
    string condition3;
    string condition4;
    int nReps;
    readonly int minTrialNum = 48; // 2 (targType) * 4 (transType) * 6 (targLoc) 

    public void GenerateExperiment(Session session)
    {
        condition1 = (string)Session.instance.participantDetails["condition1"];
        condition2 = (string)Session.instance.participantDetails["condition2"];
        condition3 = (string)Session.instance.participantDetails["condition3"];
        condition4 = (string)Session.instance.participantDetails["condition4"];

        nReps = Session.instance.settings.GetInt("n_reps");
        int n_trials = minTrialNum * nReps;

        if (!CheckConditionsUnique()) { return; }
        CreateBlocks(n_trials);
        OrderBlocks();
    }

    // Check each condition is unique - end session and reload scene if not
    private bool CheckConditionsUnique()
    {
        // Returns false if cannot be added to the hashset (as already added)
        List<string> conditions = new() { condition1, condition2, condition3, condition4 };
        var diffChecker = new HashSet<string>();
        bool allUnique = conditions.All(diffChecker.Add);

        if (!allUnique)
        {
            Destroy(GameObject.Find("[UXF_Rig]"));
            SceneManager.LoadScene("Start");
            Debug.Log("All conditions must be unique!");
        }
        return allUnique;
    }

    // Create a block for each condition of the experiment - assign it a name tag
    private void CreateBlocks(int n_trials)
    {
        Block block_RL = Session.instance.CreateBlock(n_trials);
        Block block_VRL = Session.instance.CreateBlock(n_trials);
        Block block_VRH = Session.instance.CreateBlock(n_trials);
        Block block_VRG = Session.instance.CreateBlock(n_trials);

        // Tag should be equal to corresponding Scene name
        block_RL.settings.SetValue("tag", "RL");
        block_VRL.settings.SetValue("tag", "VRL");
        block_VRH.settings.SetValue("tag", "VRH");
        block_VRG.settings.SetValue("tag", "VRG");

        bool includePracticeBlock = Session.instance.settings.GetBool("run_practice");
        if (includePracticeBlock)
        {
            int n_practice_trials = Session.instance.settings.GetInt("n_prac_trials");
            Block Practice = Session.instance.CreateBlock(n_practice_trials);
            Practice.settings.SetValue("tag", "Practice");
        }
    }

    // Re-orders the blocks in Session based upon the order given in the startup UI
    // Uses the blocks type to check against what type should be at the given position
    private void OrderBlocks()
    {
        List<Block> blockList = new(Session.instance.blocks);
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
            else if (blockType == "Practice")
            {
                // Practice is added to the block list last, and as such this should only run once all the other 
                // blocks have been sorted - placing it first and moving the other along by 1
                Session.instance.blocks.Remove(block);
                Session.instance.blocks.Insert(0, block);
            }
        }
    }
}