using UnityEngine;

[CreateAssetMenu(fileName = "KeyCodesDefaults", menuName = "ScriptableObjects/KeyCodesDefaults", order = 1)]
public class KeyCodesDefaults : ScriptableObject
{
    public KeyCode defaultButtonJump = KeyCode.Space;
    public KeyCode defaultButtonSprint = KeyCode.LeftShift;
    public KeyCode defaultButtonReload = KeyCode.R;

    public KeyCode defaultForward = KeyCode.W;
    public KeyCode defaultBackward = KeyCode.S;
    public KeyCode defaultLeft = KeyCode.A;
    public KeyCode defaultRight = KeyCode.D;

    public KeyCode defaultShot = KeyCode.Mouse0;
    public KeyCode defaultEscape = KeyCode.Escape;
}
