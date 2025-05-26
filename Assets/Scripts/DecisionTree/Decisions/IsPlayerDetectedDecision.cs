using UnityEngine;

[CreateAssetMenu(menuName = "AI/UFO/Decisions/Is Player Detected")]
public class IsPlayerDetectedDecision : DecisionNode
{
    public DecisionNode inRangeNode;
    public ActionNode outOfRangeNode;

    public override ActionNode MakeDecision(UFOBehaviour ufo)
    {
        if (ufo.PlayerTransform == null) return outOfRangeNode;

        float range = ufo.PlayerDetectionRadius;

        float distanceToPlayer = Vector3.Distance(ufo.transform.position, ufo.PlayerTransform.position);
        if (distanceToPlayer <= range)
        {
            return inRangeNode.MakeDecision(ufo);
        }
        else
        {
            return outOfRangeNode;
        }
    }
}