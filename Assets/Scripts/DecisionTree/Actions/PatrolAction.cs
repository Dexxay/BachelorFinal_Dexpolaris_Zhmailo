using UnityEngine;
using System.Collections;

[CreateAssetMenu(menuName = "AI/UFO/Actions/Patrol")]
public class PatrolAction : ActionNode
{
    public override IEnumerator Execute(UFOBehaviour ufo)
    {
        if (ufo.enableDebugLogs) Debug.Log($"UFO {ufo.name} is now Patrolling.");
        ufo.SetNewPatrolDestination(); 
        yield break;
    }
}