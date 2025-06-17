using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Restart : MonoBehaviour
{
    public GameObject automata; 

    // Listen for restart ('R') and quit ('Q') key presses each frame
    private void Update()
    {
        if (automata == null)
        {
            Debug.LogError("Automata object is missing");
            return;
        }

        if (Input.GetKeyDown(KeyCode.R))
        {
            automata.SetActive(false);
            automata.SetActive(true);
        }
        else if (Input.GetKeyDown(KeyCode.Q))
        {
            Quit();
        }
    }
    
    public void Quit()
    {
        Application.Quit();

#if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
#endif
    }
}
