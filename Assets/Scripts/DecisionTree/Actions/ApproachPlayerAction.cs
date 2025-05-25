using UnityEngine;
using System.Collections;

[CreateAssetMenu(menuName = "AI/UFO/Actions/Approach Player")]
public class ApproachPlayerAction : ActionNode
{
    public override IEnumerator Execute(UFOBehaviour ufo)
    {
        if (ufo.enableDebugLogs) Debug.Log($"UFO {ufo.name} is now Approaching Player.");
        yield break;
    }
}