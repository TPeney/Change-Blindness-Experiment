using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UXF;

public class TestSettup : MonoBehaviour
{
    [SerializeField] GameObject target;
    [SerializeField] Transform targetLocationTransform;

    public void SetUpTestScene()
    {
        Session.instance.settings.SetValue("time_between_trials", 0.5f);
        Session.instance.settings.SetValue("display_duration", 5);
        Session.instance.settings.SetValue("fixate_duration", 0.5f);
        Session.instance.settings.SetValue("hide_duration", 1);
        Session.instance.settings.SetValue("response_duration", 1);
        Session.instance.settings.SetValue("targetSide", "left");


        Session.instance.settings.SetValue("trial_type", "onset");
        Session.instance.settings.SetValue("tag", "RL");


        Block testBlock = Session.instance.CreateBlock(5);
        testBlock.settings.SetValue("target", target);
        Vector3 targetLocation = targetLocationTransform.position;
        testBlock.settings.SetValue("targetLocation", targetLocation);

    }
}
