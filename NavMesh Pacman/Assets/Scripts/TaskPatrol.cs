using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;
using BehaviorTree;
using Unity.VisualScripting;

public class TaskPatrol : BehaviorTree.Node
{
    GhostBT ghostBT;
    NavMeshAgent agent;
    private int cont;
    Transform target;
    private bool isWaiting;

    public TaskPatrol(BTree bTree) : base(bTree)
    {
        ghostBT = bTree as GhostBT;
        agent = ghostBT.transform.GetComponent<NavMeshAgent>();
        cont = 0;
        isWaiting = false;
    }

    public override NodeState Evaluate()
    {
        target = ghostBT.points[cont];
        if (target != null) agent.destination = target.position;

        if (Vector2.Distance(new Vector2(ghostBT.transform.position.x, ghostBT.transform.position.z), new Vector2(agent.destination.x, agent.destination.z)) <= 0.8f)
            if (!isWaiting) bTree.StartCoroutine(CorWaitGhost());

        state = NodeState.RUNNING;
        return state;
    }

    IEnumerator CorWaitGhost()
    {
        isWaiting = true;
        yield return new WaitForSeconds(1);
        cont++;
        if (cont == 4) cont = 0;
        isWaiting = false;

    }
}
