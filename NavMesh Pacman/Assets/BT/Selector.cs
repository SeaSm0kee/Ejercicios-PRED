using System.Collections.Generic;

namespace BehaviorTree
{
    public class Selector : Node
    {
        public Selector(BTree bTree) : base(bTree) { }
        public Selector(BTree bTree, List<Node> children) : base(bTree, children) { }

        public override NodeState Evaluate()
        {
            foreach (Node node in children)
            {
                switch (node.Evaluate())
                {
                    case NodeState.FAILURE:
                        continue;
                    case NodeState.SUCCESS:
                        state = NodeState.SUCCESS;
                        return state;
                    case NodeState.RUNNING:
                        state = NodeState.RUNNING;
                        return state;
                    default:
                        continue;
                }
            }

            state = NodeState.FAILURE;
            return state;
        }

    }

}