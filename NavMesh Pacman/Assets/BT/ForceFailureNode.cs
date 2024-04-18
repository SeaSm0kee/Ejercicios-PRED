using System.Collections.Generic;
using UnityEngine;

namespace BehaviorTree
{
    public class ForceFailureNode : Node
    {

        public ForceFailureNode(BTree bTree) : base(bTree) { }
        public ForceFailureNode(BTree bTree, Node child) : base(bTree, child) { }

        public override NodeState Evaluate()
        {
            var result = children[0].Evaluate();
            if (result == NodeState.RUNNING)
            {
                state = NodeState.RUNNING;
            }
            else
            {
                state = NodeState.FAILURE;
            }
            return state;
        }

    }

}