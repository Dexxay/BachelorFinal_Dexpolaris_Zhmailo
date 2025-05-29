using TMPro;
using UnityEngine;

public class HealthCount : MonoBehaviour
{
    [SerializeField] private PlayerHealthManager playerHealthManager;
    [SerializeField] private TMP_Text healthCount;


    void Start()
    {
        if (playerHealthManager.Health != 0)
        {
            UpdateHealthUI(playerHealthManager.Health);
        }
        playerHealthManager.HealthChanged += UpdateHealthUI;
    }

    private void UpdateHealthUI(int newHp)
    {
        healthCount.text = newHp.ToString();
    }
}
