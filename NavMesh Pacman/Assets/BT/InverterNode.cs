using System.Collections.Generic;
using UnityEngine;

namespace BehaviorTree
{
    public class InverterNode : Node
    {

        public InverterNode(BTree bTree) : base(bTree) { }
        public InverterNode(BTree bTree, Node child) : base(bTree, child) { }

        public override NodeState Evaluate()
        {

            switch (children[0].Evaluate())
            {
                case NodeState.FAILURE:
                    state = NodeState.SUCCESS;
                    break;
                case NodeState.SUCCESS:
                    state = NodeState.FAILURE;
                    break;
                default:
                case NodeState.RUNNING:
                    state = NodeState.RUNNING;
                    break;
            }
            return state;

        }

    }

}