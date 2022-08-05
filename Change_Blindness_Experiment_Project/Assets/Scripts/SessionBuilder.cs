using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UXF;
using UnityEngine.InputSystem;

public class SessionBuilder : MonoBehaviour
{
    int n_trials;

    public void GenerateExperiment(Session session)
    {
        n_trials = Session.instance.settings.GetInt("n_trials");

        // Trial count must be even as half will be onset and the other half luminance
        if (n_trials % 2 != 0) { Debug.LogError("Trial count must be an even number."); }

        CreateBlocks(n_trials);
        OrderBlocks();
    }

    private static void CreateBlocks(int n_trials)
    {
        Block block_RL = Session.instance.CreateBlock(n_trials);
        Block block_VRL = Session.instance.CreateBlock(n_trials);
        Block block_VRH = Session.instance.CreateBlock(n_trials);
        Block block_VRG = Session.instance.CreateBlock(n_trials);

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
}