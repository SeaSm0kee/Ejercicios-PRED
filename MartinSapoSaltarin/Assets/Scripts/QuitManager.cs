using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class QuitManager : MonoBehaviour
{
    // Update is called once per frame
    void Update()
    {
        if(Input.GetKeyDown(KeyCode.Q)) QuitGame();
    }

    public void QuitGame()
    {
        SceneManager.LoadScene(0);
    }
}
