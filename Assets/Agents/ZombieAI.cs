using System.Collections.Generic;
using NPBehave;
using TMPro;
using UnityEngine;
using UnityEngine.AI;
using Action = NPBehave.Action;
using Random = UnityEngine.Random;
namespace Agents
{


    public class ZombieAI : IndividualAgent
    {
        private bool m_isAttacking = false;
        private bool hasDestination = false;
        private TextMeshPro actionText;

        private void Start()
        {
            m_navMeshAgent = GetComponent<NavMeshAgent>();
            InitializeBT();
            actionText = transform.Find("Text - Action").GetComponent<TextMeshPro>();
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
                        new BlackboardCondition("withinAttackRadius", Operator.IS_EQUAL, false, Stops.IMMEDIATE_RESTART,
                            new Action(Chase)),
                        new Action(Attack)
                    )
                )
            );
            m_BT.Blackboard["hasATarget"] = false;
            m_BT.Blackboard["withinAttackRadius"] = false;
        }

        private void Roam()
        {
            m_navMeshAgent.speed = 2;
            actionText.text = "";
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
            m_navMeshAgent.speed = 7;
            actionText.text = "Chase";
            m_navMeshAgent.SetDestination(m_target.transform.position);
        }

        private void Attack()
        {
            m_target.GetComponent<IndividualAgent>().Die();
            m_targets.Remove(m_target);
            clearTarget(m_target);
            TargetClosestAgent();
        }

        protected override void clearTarget(GameObject agent)
        {
            base.clearTarget(agent);
            m_BT.Blackboard["withinAttackRadius"] = false;
        }

        protected override void setTarget(GameObject agent)
        {
            Debug.Log("Survivor spotted");
            base.setTarget(agent);
        }

        private void Update()
        {
            if (m_target) {
                if (Vector3.Distance(transform.position, m_target.transform.position) <= 3) {
                    m_BT.Blackboard["withinAttackRadius"] = true; // Attack range of 3m
                }
            }
        }
    }
}
