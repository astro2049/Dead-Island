using System;
using System.Collections;
using System.Collections.Generic;
using NPBehave;
using UnityEngine;
using UnityEngine.AI;
using Action = NPBehave.Action;
using Random = UnityEngine.Random;

public enum PerceptionType
{
    Visual = 0,
    Aural = 1
}

public class ZombieAI : MonoBehaviour
{
    private Dictionary<GameObject, HashSet<PerceptionType>> m_targets = new Dictionary<GameObject, HashSet<PerceptionType>>(); // list of survivors perceived
    private GameObject m_target; // The only target survivor the zombie is gonna chase after
    private NavMeshAgent m_navMeshAgent;
    private Root m_BT;
    private bool m_isAttacking = false;
    private bool hasDestination = false;

    private void Start()
    {
        m_navMeshAgent = GetComponent<NavMeshAgent>();
        InitializeBT();
    }

    private void InitializeBT()
    {
        m_BT = new Root(
            new Selector(
                new BlackboardCondition("hasATarget", Operator.IS_EQUAL, false, Stops.IMMEDIATE_RESTART,
                    new Sequence(
                        new Action(Roam),
                        new Wait(Random.Range(1, 2)))
                ),
                new Selector(
                    new BlackboardCondition("WithinAttackRadius", Operator.IS_EQUAL, false, Stops.IMMEDIATE_RESTART,
                        new Action(Chase)),
                    new Action(Attack)
                )
            )
        );
        m_BT.Blackboard["hasATarget"] = false;
        m_BT.Blackboard["WithinAttackRadius"] = false;
    }

    public void ActivateBT()
    {
        m_BT.Start();
    }

    public void DeactivateBT()
    {
        m_BT.Stop();
    }

    private void Roam()
    {
        if (m_navMeshAgent.remainingDistance <= m_navMeshAgent.stoppingDistance) {
            generateARandomDestination();
        }
    }

    private void generateARandomDestination()
    {
        // Logic to move to a random point
        var randomDirection = Random.insideUnitSphere * 10; // 10 is the roam radius
        randomDirection += transform.position;
        NavMeshHit hit;
        NavMesh.SamplePosition(randomDirection, out hit, 10, 1);
        var finalPosition = hit.position;
        m_navMeshAgent.SetDestination(finalPosition);
    }

    private void Chase()
    {
        m_navMeshAgent.SetDestination(m_target.transform.position);
    }

    private void Attack()
    {
        m_navMeshAgent.ResetPath();
    }

    private void clearTarget(GameObject survivor)
    {
        // Clear flag and m_target
        m_BT.Blackboard["hasATarget"] = false;
        m_target = null;
    }

    private void setTarget(GameObject survivor)
    {
        m_BT.Blackboard["hasATarget"] = true;
        m_target = survivor;
    }

    // This is only called when zombie loses track of the current chasing survivor, for a zombie will chase a survivor to death (..?)
    private void SelectClosestSurvivor()
    {
        GameObject closestSurvivor = null;
        float closestDistance = Mathf.Infinity;
        foreach (KeyValuePair<GameObject, HashSet<PerceptionType>> pair in m_targets) {
            var survivor = pair.Key;
            float distance = Vector3.Distance(transform.position, survivor.transform.position);
            if (distance < closestDistance) {
                closestDistance = distance;
                closestSurvivor = survivor;
            }
        }
        if (closestSurvivor) {
            setTarget(closestSurvivor);
        }
    }

    private void Update()
    {
        // if (m_Targets.Count != 0) {
        //     float distanceToTarget = Vector3.Distance(transform.position, m_Targets[0].transform.position);
        //     m_BT.Blackboard["hasATarget"] = distanceToTarget <= 10; // Visible within 10 units
        //     m_BT.Blackboard["WithinAttackRadius"] = distanceToTarget <= 1; // Attack range of 2 units
        // }
    }

    public void gainPerceptionOnSurvivor(GameObject survivor, PerceptionType perception)
    {
        m_targets[survivor].Add(perception);

        // Chase this survivor if no target is specified at the moment
        if (!m_target) {
            setTarget(survivor);
        }
    }

    public void losePerceptionOnSurvivor(GameObject survivor, PerceptionType perception)
    {
        m_targets[survivor].Remove(perception);
        if (m_targets[survivor].Count == 0) {
            m_targets.Remove(survivor);
        }

        if (survivor == m_target) {
            clearTarget(survivor);

            // Find the currently closest survivor, to chase
            SelectClosestSurvivor();
        }
    }
}
