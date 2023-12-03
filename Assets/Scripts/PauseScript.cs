using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement; 

public class pause : MonoBehaviour
{   
    public static bool GameIsPaused = false;
    public GameObject pauseMenuUI;
    public AudioManagerFactory audioManager;

    void Start()
    {
        
    }

    public void Resume()
    {
        audioManager = FindObjectOfType<AudioManagerFactory>();
        pauseMenuUI.SetActive(false);
        Time.timeScale = 1f;
        GameIsPaused = false;
        audioManager.musicAudioSource.UnPause();

        LevelController controller = GameObject.FindWithTag("GameController").GetComponent<LevelController>();
        controller.state = LevelController.State.PLAYING;
    }

    public void Pause()
    {
        audioManager = FindObjectOfType<AudioManagerFactory>();
        pauseMenuUI.SetActive(true);
        Time.timeScale = 0f;
        GameIsPaused = true;
        audioManager.musicAudioSource.Pause();

        LevelController controller = GameObject.FindWithTag("GameController").GetComponent<LevelController>();
        controller.state = LevelController.State.PAUSE;
    }

    public void restart()
    {
        GameObject.FindWithTag("GameController").GetComponent<LevelController>().ChangeArc(true, SceneManager.GetActiveScene().buildIndex);
        
        // Reset các thông số của Robot
        GameObject.FindWithTag("Timer").GetComponent<Score>().timeValue = 0;
        GameObject.FindWithTag("Timer").GetComponent<Score>().score = 0;

        Resume();
    }
    public void quit()
    {
        Application.Quit();
    }
    public void mainmenu()
    {
        SceneManager.LoadScene(0);
    }
}