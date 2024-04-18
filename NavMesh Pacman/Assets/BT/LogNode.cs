using UnityEngine;


namespace BehaviorTree
{
    public class LogNode : Node
    {
        string message;

        public LogNode(BTree btree, string message) : base(btree)
        {
            this.message = message;
        }

        public override NodeState Evaluate()
        {
            Debug.Log(message);
            state = NodeState.SUCCESS;
            return state;
        }

    }
}