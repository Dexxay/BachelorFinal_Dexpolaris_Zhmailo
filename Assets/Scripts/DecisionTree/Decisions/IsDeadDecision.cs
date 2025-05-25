using UnityEngine;

[CreateAssetMenu(menuName = "AI/UFO/Decisions/Is Dead")]
public class IsDeadDecision : DecisionNode
{
    public ActionNode deadNode;       
    public DecisionNode notDeadNode;  

    public override ActionNode MakeDecision(UFOBehaviour ufo)
    {
        if (ufo.CurrentHealth <= 0)
        {
            return deadNode;
        }
        else
        {
            return notDeadNode.MakeDecision(ufo);  
        }
    }
}