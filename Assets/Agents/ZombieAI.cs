using System.Collections;
using System.Collections.Generic;
using NPBehave;
using UnityEngine;
using UnityEngine.AI;

public class ZombieAI : MonoBehaviour
{
    public HashSet<GameObject> m_Targets;
    private NavMeshAgent m_NavMeshAgent;
    private Root m_BT;
    private bool m_isAttacking = false;

    private void Start()
    {
        m_NavMeshAgent = GetComponent<NavMeshAgent>();
        InitializeBT();
    }

    private void InitializeBT()
    {
        m_BT = new Root(
            new Selector(
                new BlackboardCondition("CanSeeSurvivor", Operator.IS_EQUAL, false, Stops.IMMEDIATE_RESTART,
                    new Action(Roam)
                ),
                new Selector(
                    new BlackboardCondition("WithinAttackRadius", Operator.IS_EQUAL, false, Stops.IMMEDIATE_RESTART,
                        new Action(Chase)),
                    new Action(Attack)
                )
            )
        );
        m_BT.Start();
    }

    private void Roam()
    {
        // Logic to move to a random point
        Vector3 randomDirection = UnityEngine.Random.insideUnitSphere * 10; // 10 is the roam radius
        randomDirection += transform.position;
        NavMeshHit hit;
        NavMesh.SamplePosition(randomDirection, out hit, 10, 1);
        Vector3 finalPosition = hit.position;
        m_NavMeshAgent.SetDestination(finalPosition);
    }

    private void Chase()
    {
        // m_NavMeshAgent.SetDestination(m_Targets[0].transform.position);
    }

    private void Attack()
    {
        m_NavMeshAgent.ResetPath();
    }

    private void Update()
    {
        // if (m_Targets.Count != 0) {
        //     float distanceToTarget = Vector3.Distance(transform.position, m_Targets[0].transform.position);
        //     m_BT.Blackboard["CanSeeSurvivor"] = distanceToTarget <= 10; // Visible within 10 units
        //     m_BT.Blackboard["WithinAttackRadius"] = distanceToTarget <= 1; // Attack range of 2 units
        // }
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Survivor")) {
            // Assume target is the survivor for simplicity
            m_Targets.Add(other.gameObject);
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("Survivor")) {
            m_Targets.Remove(other.gameObject);
        }
    }
}
