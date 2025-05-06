using TMPro;
using UnityEngine;

public class ScoreCount : MonoBehaviour
{
    [SerializeField] private ScoreManager scoreManager;
    [SerializeField] private TMP_Text scoreCount;

    void Start()
    {
        scoreManager.ScoreChanged += OnScoreChanged;
    }

    private void OnScoreChanged(int score)
    {
        scoreCount.text = score.ToString();
    }
}
