using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PauseManager : MonoBehaviour
{
    private static PauseManager instance;

    [SerializeField] MainPlayer player;

    private bool isPaused;
    private bool isDead;

    public bool IsPaused => isPaused;
    public bool IsDead => IsDead;

    public event Action PauseStarted;
    public event Action PauseEnded;

    public static PauseManager Instance => instance;

    void Awake()
    {
        if (instance == null)
        {
            instance = this;
        }
        else if (instance != this)
        {
            Destroy(this);
        }

        isPaused = false;
        isDead = false;

        player.PlayerHealthManager.PlayerDied += OnPlayerDied;
    }

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.Escape) && !isDead)
        {
            if (isPaused)
            {
                OnPauseEnded();
            }
            else
            {
                OnPauseStarted();
            }
        }
    }

    public void OnPauseStarted()
    {
        PauseStarted?.Invoke();
        Cursor.visible = true;
        Cursor.lockState = CursorLockMode.None;
        Time.timeScale = 0;
        isPaused = true;
    }

    public void OnPauseEnded()
    {
        PauseEnded?.Invoke();
        Cursor.visible = false;
        Cursor.lockState = CursorLockMode.Locked;
        Time.timeScale = 1;
        isPaused = false;
    }


    private void OnPlayerDied()
    {
        isDead = true;
    }
}
