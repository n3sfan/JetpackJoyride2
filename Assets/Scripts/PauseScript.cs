using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

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
        audioManager = FindObjectOfType<AudioManagerFactory>();
        pauseMenuUI.SetActive(false);
        Time.timeScale = 1f;
        GameIsPaused = false;
        audioManager.musicAudioSource.UnPause();

        PlatformerRobot robot = GameObject.FindWithTag("Robot").GetComponent<PlatformerRobot>();

        GameObject gameOverUI = GameObject.Find("GameOver");
        // Khi Robot chết, bấm Play Again
        if (gameOverUI != null) {
            gameOverUI.SetActive(false);

            robot.GetComponent<Animator>().enabled = true;
            GameObject score = GameObject.FindGameObjectWithTag("Timer");
            score.GetComponent<Score>().ResumeTimer();
            
            AudioManagerFactory audioManager = GameObject.FindWithTag("Audio").GetComponent<AudioManagerFactory>();
            audioManager.musicAudioSource.Play();
        }

        LevelController controller = GameObject.FindWithTag("GameController").GetComponent<LevelController>();
        controller.arcPlaySeconds = 0;
        controller.ChangeArc(true, SceneManager.GetActiveScene().buildIndex);
        
        // Reset các thông số của Robot
        GameObject.FindWithTag("Timer").GetComponent<Score>().timeValue = 0;
        GameObject.FindWithTag("Timer").GetComponent<Score>().score = 0;

        robot.life = 3;
        foreach (GameObject obj in robot.hearts) {
            obj.GetComponent<Image>().enabled = true;
        }
        
        // Tạo lại background
        GameObject[] objects = GameObject.FindGameObjectsWithTag("Background");

        foreach (GameObject obj in objects) {
            if (obj != null) {
                Destroy(obj);
            }
        }

        int levelIndex = SceneManager.GetActiveScene().buildIndex; 
        String bgName = "";
        switch (levelIndex) {
            case 1: 
                bgName = "Factory";
                break;
            case 2:
                bgName = "Ocean";
                break;
            case 3:
                bgName = "End";
                break;
            default:
                break;
        }

        GameObject prefabBackground = (GameObject) Resources.Load("Prefabs/Background/" + bgName);
        Instantiate(prefabBackground, new Vector3(-0.03f, 1.33f), Quaternion.identity);
    }
    public void quit()
    {
        Application.Quit();
    }
    public void mainmenu()
    {
        Resume();
        SceneManager.LoadScene(0);

        if (GameObject.FindWithTag("Menu") != null)
            Destroy(GameObject.FindWithTag("Menu"));
        if (GameObject.FindWithTag("Robot") != null)
            Destroy(GameObject.FindWithTag("Robot"));
        if (GameObject.FindWithTag("GameController") != null)
            Destroy(GameObject.FindWithTag("GameController"));
        if (GameObject.Find("EventSystem") != null)
            Destroy(GameObject.Find("EventSystem"));
        if (GameObject.Find("Jumpfire") != null)
            Destroy(GameObject.Find("Jumpfire"));

        GameObject[] objects = GameObject.FindGameObjectsWithTag("Background");
        // Giữ cho Scene sau
        foreach (GameObject obj in objects) {
            if (obj != null)
                Destroy(obj);
        }        
    }
}