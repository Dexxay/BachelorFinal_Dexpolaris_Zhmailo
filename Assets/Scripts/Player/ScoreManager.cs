using System;
using UnityEngine;

public class ScoreManager : MonoBehaviour
{
    private int score;
    private float timeSurvived;

    public event Action<int> ScoreChanged;

    public float TimeSurvived => timeSurvived;
    public int Score => score;

    void Start()
    {
        score = 0;
    }
    void Update()
    {
        timeSurvived += Time.deltaTime;
    }

    public void AddScore(int scoresAnount)
    {
        score += scoresAnount;
        ScoreChanged?.Invoke(score);
    }
}
