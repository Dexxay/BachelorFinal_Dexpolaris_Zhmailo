 using UnityEngine;
using System.Collections;  
 public abstract class DecisionNode : ScriptableObject
{
     public abstract ActionNode MakeDecision(UFOBehaviour ufo);
}