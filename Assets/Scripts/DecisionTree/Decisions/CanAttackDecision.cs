using UnityEngine;

[CreateAssetMenu(menuName = "AI/UFO/Decisions/Can Attack")]
public class CanAttackDecision : DecisionNode
{
    public ActionNode canAttackNode;
    public ActionNode cannotAttackNode;

    public override ActionNode MakeDecision(UFOBehaviour ufo)
    {
        if (ufo.CanAttack())
        {
            return canAttackNode;
        }
        else
        {
            return cannotAttackNode;
        }
    }
}