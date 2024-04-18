using System.Collections.Generic;
using UnityEngine;

namespace BehaviorTree
{
    public class RepeatNode : Node
    {
        int numOfLoops = 1;

        public RepeatNode(BTree bTree, int numOfLoops = 1) : base(bTree) { this.numOfLoops = numOfLoops; }
        public RepeatNode(BTree bTree, Node child, int numOfLoops = 1) : base(bTree, child) { this.numOfLoops = numOfLoops; }

        public override NodeState Evaluate()
        {
            for (int i = 0; i < numOfLoops; i++)
            {
                //Debug.Log("Repetition number: " + (i + 1));
                switch (children[0].Evaluate())
                {
                    case NodeState.FAILURE:
                        state = NodeState.FAILURE;
                        return state;
                    case NodeState.SUCCESS:
                        continue;
                    case NodeState.RUNNING:
                        state = NodeState.RUNNING;
                        return state;
                    default:
                        continue;
                }

            }

            state = NodeState.SUCCESS;
            return state;
        }

    }

}