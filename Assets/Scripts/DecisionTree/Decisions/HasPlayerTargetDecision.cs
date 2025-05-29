using UnityEngine;

[CreateAssetMenu(menuName = "AI/UFO/Decisions/Has Player Target")]
public class HasPlayerTargetDecision : DecisionNode
{
    public DecisionNode trueNode;
    public ActionNode falseNode;

    public override ActionNode MakeDecision(UFOBehaviour ufo)
    {
        if (ufo.PlayerTransform != null)
        {
            return trueNode.MakeDecision(ufo);
        }
        else
        {
            return falseNode;
        }
    }
}