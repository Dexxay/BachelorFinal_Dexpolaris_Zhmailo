using UnityEngine;
public abstract class DecisionNode : ScriptableObject
{
    public abstract ActionNode MakeDecision(UFOBehaviour ufo);
}