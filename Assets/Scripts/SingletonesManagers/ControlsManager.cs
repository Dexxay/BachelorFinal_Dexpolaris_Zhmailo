using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ControlsManager : MonoBehaviour
{
    public static ControlsManager instance;
    [SerializeField] private KeyCodesDefaults keyCodesDefaults;

    private KeyCode moveForwardKey;
    private KeyCode moveBackwardKey;
    private KeyCode moveLeftKey;
    private KeyCode moveRightKey;
    private KeyCode jumpKey;
    private KeyCode sprintKey;
    private KeyCode shotKey;
    private KeyCode reloadKey;
    private KeyCode escapeKey;

    public KeyCode MoveForwardKey => moveForwardKey;
    public KeyCode MoveBackwardKey => moveBackwardKey;
    public KeyCode MoveLeftKey => moveLeftKey;
    public KeyCode MoveRightKey => moveRightKey;
    public KeyCode JumpKey => jumpKey;
    public KeyCode SprintKey => sprintKey;
    public KeyCode ShotKey => shotKey;
    public KeyCode ReloadKey => reloadKey;
    public KeyCode EscapeKey => escapeKey;

    void Start()
    {
        if (instance == null)
        {
            instance = this;
        }
        else if (instance != this)
        {
            Destroy(this);
        }

        jumpKey = GetKeyCode("JumpKey", keyCodesDefaults.defaultButtonJump);
        sprintKey = GetKeyCode("SprintKey", keyCodesDefaults.defaultButtonSprint);
        reloadKey = GetKeyCode("ReloadKey", keyCodesDefaults.defaultButtonReload);

        moveForwardKey = keyCodesDefaults.defaultForward;
        moveBackwardKey = keyCodesDefaults.defaultBackward;
        moveLeftKey = keyCodesDefaults.defaultLeft;
        moveRightKey = keyCodesDefaults.defaultRight;
        escapeKey = keyCodesDefaults.defaultEscape;
        shotKey = keyCodesDefaults.defaultShot;
    }

    private KeyCode GetKeyCode(string key, KeyCode defaultValue)
    {
        string keyString = PlayerPrefs.GetString(key, defaultValue.ToString());

        if (Enum.TryParse(keyString, out KeyCode keyCode))
        {
            return keyCode;
        }
        else
        {
            return defaultValue;
        }
    }
}
