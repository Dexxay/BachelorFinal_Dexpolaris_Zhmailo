using System;
using TMPro;
using UnityEngine;

public class Results : MonoBehaviour
{
    [SerializeField] private MainPlayer player;
    [SerializeField] private TMP_Text scoreCount;
    [SerializeField] private TMP_Text timeCount;

    void Start()
    {
        TimeSpan timeSpan = TimeSpan.FromSeconds(player.ScoreManager.TimeSurvived);

        string formattedTime = string.Format("{0:D2}:{1:D2}",
            timeSpan.Minutes,
            timeSpan.Seconds);

        scoreCount.text = player.ScoreManager.Score.ToString();
        timeCount.text = formattedTime;
    }
}
