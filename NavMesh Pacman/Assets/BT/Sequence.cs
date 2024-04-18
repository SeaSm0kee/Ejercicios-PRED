using System.Collections.Generic;

namespace BehaviorTree
{
    public class Sequence : Node
    {
        public Sequence(BTree bTree) : base(bTree) { }
        public Sequence(BTree bTree, List<Node> children) : base(bTree, children) { }

        public override NodeState Evaluate()
        {
            //bool anyChildIsRunning = false;

            foreach (Node node in children)
            {
                switch (node.Evaluate())
                {
                    case NodeState.FAILURE:
                        state = NodeState.FAILURE;
                        return state;
                    case NodeState.SUCCESS:
                        continue;
                    case NodeState.RUNNING:
                        //anyChildIsRunning = true;
                        state = NodeState.RUNNING;
                        return state;
                    default:
                        state = NodeState.SUCCESS;
                        return state;
                }
            }

            //state = anyChildIsRunning ? NodeState.RUNNING : NodeState.SUCCESS;
            state = NodeState.SUCCESS;
            return state;
        }

    }

}