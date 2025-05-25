using UnityEngine;
using System.Collections;  

 public abstract class ActionNode : ScriptableObject
{
     public abstract IEnumerator Execute(UFOBehaviour ufo);
}