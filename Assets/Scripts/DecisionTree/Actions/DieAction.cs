using UnityEngine;
using System.Collections;

[CreateAssetMenu(menuName = "AI/UFO/Actions/Die")]
public class DieAction : ActionNode
{
    public override IEnumerator Execute(UFOBehaviour ufo)
    {
        if (ufo.enableDebugLogs) Debug.Log($"UFO {ufo.name} is now Dying.");
        ufo.Die(); 
        yield break;  
    }
}