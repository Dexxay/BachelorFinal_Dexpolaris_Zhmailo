using System.Collections;
using UnityEngine;

[CreateAssetMenu(menuName = "AI/UFO/Actions/Circle Player")]
public class CirclePlayerAction : ActionNode
{
    public override IEnumerator Execute(UFOBehaviour ufo)
    {
        if (ufo.enableDebugLogs) Debug.Log($"UFO {ufo.name} is now Circling Player.");
        yield break;
    }
}