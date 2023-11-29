using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class Score : MonoBehaviour
{   
    private bool isPaused = false;
    public float timeValue = 0;
    public Text timerText;

    void Update()
    {
        if (!isPaused)
        {
            timeValue += Time.deltaTime * 2;
            DisplayTime(timeValue);
        }
    }

    void DisplayTime(float timeToDisplay)
    {
        float scores = Mathf.FloorToInt(timeToDisplay % 10000000);

        timerText.text = string.Format("{0:000000000}", scores);
    }
    public void PauseTimer()
    {
        isPaused = true;
    }

    public void ResumeTimer()
    {
        isPaused = false;
    }
}
