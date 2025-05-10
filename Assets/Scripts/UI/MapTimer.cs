using UnityEngine;
using TMPro;

public class MapTimer : MonoBehaviour
{
    [SerializeField] public float mapDurationInSeconds = 120f;
    [SerializeField] private float indicateTimeTillEnd = 30f;
    [SerializeField] private TMP_Text timerText;
    [SerializeField] private PlayerHealthManager playerHealthManager;

    private float timeLeft;
    private bool timerEnded = false;

    void Start()
    {
        timeLeft = mapDurationInSeconds;
        UpdateTimerText();
    }

    void Update()
    {
        if (timerEnded) return;

        timeLeft -= Time.deltaTime;

        if (timeLeft <= indicateTimeTillEnd)
        {
            timerText.color = Color.red;
        }

        if (timeLeft <= 0f && !timerEnded)
        {
            timeLeft = 0f;
            timerEnded = true;
            UpdateTimerText();
            playerHealthManager.ReduceHealth(playerHealthManager.MaxHealth);
            return;
        }

        UpdateTimerText();
    }

    private void UpdateTimerText()
    {
        int minutes = Mathf.FloorToInt(timeLeft / 60f);
        int seconds = Mathf.FloorToInt(timeLeft % 60f);
        timerText.text = $"{minutes:00}:{seconds:00}";
    }
}
