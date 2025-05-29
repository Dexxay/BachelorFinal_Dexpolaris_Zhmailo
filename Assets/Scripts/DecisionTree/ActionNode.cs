using System.Collections;
using UnityEngine;

public abstract class ActionNode : ScriptableObject
{
    public abstract IEnumerator Execute(UFOBehaviour ufo);
}