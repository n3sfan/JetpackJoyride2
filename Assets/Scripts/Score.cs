using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class Score : MonoBehaviour
{   
    private bool isPaused = false;
    public float timeValue = 0f;
    public int score;
    public Text timerText;
    [SerializeField]
    TextMeshProUGUI highScoreText;

    void Update()
    {
        if (!isPaused)
        {
            timeValue += Time.deltaTime * 2;
            //DisplayTime(timeValue);
            if (timeValue >= 1f) {
                score += 5; 
                timeValue = 0;
            }
            DisplayTime(score);
            checkHighScore();
            updateHighScoreText();
        }
    }

    void checkHighScore() {
        if (score > PlayerPrefs.GetInt("HighScore", 0)) {
            PlayerPrefs.SetInt("HighScore", score);
        }
    }

    void updateHighScoreText() {
        highScoreText.text = $"HighScore:{PlayerPrefs.GetInt("HighScore", 0)}";
    }

    void DisplayTime(int score)
    {
        timerText.text = $"Score:{score}"; //string.Format("{0:000000000}", score);
    } 
    /*
    void DisplayTime(float timeToDisplay)
    {
        float scores = Mathf.FloorToInt(timeToDisplay % 10000000);

        timerText.text = string.Format("{0:000000000}", scores);
    } */
    public void PauseTimer()
    {
        isPaused = true;
    }

    public void ResumeTimer()
    {
        isPaused = false;
    }
    
}
