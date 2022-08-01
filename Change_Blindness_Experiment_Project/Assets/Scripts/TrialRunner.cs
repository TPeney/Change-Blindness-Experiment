using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TrialRunner : MonoBehaviour
{
    [SerializeField] GameObject testStimuli;

    [Tooltip("The empty GameObject representing the spawn area for stimuli")]
  

    // Start is called before the first frame update
    void Start()
    {
        for (int i = 0; i < 20; i++)
        {
            CreateStimuli();
        }

    }

    private void Update()
    {
        
    }


   

    private void CreateStimuli()
    {
        Instantiate(testStimuli);
    }

 


}
