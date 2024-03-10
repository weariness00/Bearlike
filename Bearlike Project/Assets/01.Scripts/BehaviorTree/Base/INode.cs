namespace BehaviorTree.Base
{
    /// <summary>
    /// Node의 상태와 반환하는 함수를 가지고 있는 Interface
    /// </summary>
    public interface INode
    {
        public enum NodeState
        {
            Break,
            Success,
            Failure,
        }

        public NodeState Evaluate();
    }
}