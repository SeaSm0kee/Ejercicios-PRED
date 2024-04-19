using System.Collections;
using System.Collections.Generic;
using BehaviorTree;
using UnityEngine;

public class GhostBT : BTree
{
    public List<Transform> points;
    public LayerMask layerMask;
    public float radius;

    protected override Node SetupTree()
    {
        Node root = new Selector(this, new List<Node>(){
            new Sequence(this,new List<Node>(){
                new TaskPacmanIsOnRange(this),
                new TaskGoToTarget(this)
            }),
            new TaskPatrol(this)
        });
        return root;
    }
}
