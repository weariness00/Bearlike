using BehaviorTree.Base;
using UnityEngine;

public class TestJester : MonoBehaviour
{
    private BehaviorTreeRunner _bt;
    
    void Start()
    {
        
    }

    void Update()
    {
        
    }
    
    public INode InitBT()
    {
        var Idle = new ActionNode(IdleNode);
        
        var TP = new ActionNode(TeleportAction);
        

        var AttackPattern = new SelectorNode(
                                true, 
                                TP      
                                );
        
        
        var loop = new SequenceNode(
                        Idle,
                        AttackPattern
                        );

        return loop;
    }

    private INode.NodeState IdleNode()
    {
        
        return INode.NodeState.Success;
    }

    private INode.NodeState TeleportAction()
    {
        return INode.NodeState.Success;
    }
}
