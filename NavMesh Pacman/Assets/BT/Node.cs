using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting.Antlr3.Runtime.Tree;

namespace BehaviorTree
{

    public enum NodeState
    {
        RUNNING,
        SUCCESS,
        FAILURE
    }

    public class Node
    {
        protected NodeState state;

        public Node parent;
        protected List<Node> children = new List<Node>();

        protected BTree bTree;


        public Node(BTree bTree)
        {
            parent = null;
            this.bTree = bTree;
        }
        public Node(BTree bTree, List<Node> children)
        {
            parent = null;
            this.bTree = bTree;
            foreach (Node child in children)
                Attach(child);
        }

        public Node(BTree bTree, Node child) : this(bTree, new List<Node>() { child }) { }

        private void Attach(Node node)
        {
            node.parent = this;
            children.Add(node);
        }

        public virtual NodeState Evaluate() => NodeState.FAILURE;
    }

}