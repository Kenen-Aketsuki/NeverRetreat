using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class TmpStopGame : MonoBehaviour
{
    private void OnEnable()
    {
        Time.timeScale = 0f;
    }

    private void OnDisable()
    {
        Time.timeScale = 1.0f;
    }

    public void Continue()
    {
        gameObject.SetActive(false);
    }

    public void SaveGame()
    {
        GameUtility.������Ϸ();
    }

    public void SaveAndQuit()
    {
        GameUtility.������Ϸ();
        SceneManager.LoadScene("Title");
    }

    public void QuitGame()
    {
        SceneManager.LoadScene("Title");
    }
}
