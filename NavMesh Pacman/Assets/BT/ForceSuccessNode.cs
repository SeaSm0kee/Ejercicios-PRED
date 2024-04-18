using System.Collections.Generic;
using UnityEngine;

namespace BehaviorTree
{
    public class ForceSuccessNode : Node
    {

        public ForceSuccessNode(BTree bTree) : base(bTree) { }
        public ForceSuccessNode(BTree bTree, Node child) : base(bTree, child) { }

        public override NodeState Evaluate()
        {
            var result = children[0].Evaluate();
            if (result == NodeState.RUNNING)
            {
                state = NodeState.RUNNING;
            }
            else
            {
                state = NodeState.SUCCESS;
            }
            return state;
        }

    }

}