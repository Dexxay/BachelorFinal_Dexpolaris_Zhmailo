using System.Collections;
using UnityEngine;

[CreateAssetMenu(menuName = "AI/UFO/Actions/Attack")]
public class AttackAction : ActionNode
{
    public override IEnumerator Execute(UFOBehaviour ufo)
    {
        if (ufo.enableDebugLogs) Debug.Log($"UFO {ufo.name} is now Attacking.");
        ufo.StartAttackCoroutine();
        yield break;
    }
}