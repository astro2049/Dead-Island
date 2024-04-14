using System.Collections;
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
        private TextMeshPro m_actionText;

        private void Start()
        {
            base.Start();

            agentType = AgentType.Zombie;
            m_navMeshAgent = GetComponent<NavMeshAgent>();
            InitializeBT();
            m_actionText = transform.Find("Text - Action").GetComponent<TextMeshPro>();
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
            m_actionText.text = "";
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
            m_actionText.text = "Chase";
            m_navMeshAgent.SetDestination(m_target.transform.position);
        }

        private void Attack()
        {
            m_target.GetComponent<IndividualAgent>().Die();
            RetargetClosestAgent();
        }

        protected override void clearTarget()
        {
            base.clearTarget();
            m_BT.Blackboard["withinAttackRadius"] = false;
        }

        private void Update()
        {
            if (m_target) {
                if (Vector3.Distance(transform.position, m_target.transform.position) <= 3) {
                    m_BT.Blackboard["withinAttackRadius"] = true; // Attack range of 3m
                }
            }
        }

        public override void Die()
        {
            if (lifeStatus == LifeStatus.Dead) {
                return;
            }
            lifeStatus = LifeStatus.Dead;
            m_navMeshAgent.enabled = false;
            transform.Rotate(new Vector3(0, 90, 0));
            DeactivateBT();
            m_gameManager.m_zombieCount--;
            m_gameManager.UpdateZombieCountText();
            StartCoroutine(SinkUnderMap());
        }

        private IEnumerator SinkUnderMap()
        {
            float duration = 5.0f;
            var startPosition = transform.position;
            var endPosition = startPosition + new Vector3(0, -10, 0);
            float elapsedTime = 0;
            while (elapsedTime < duration) {
                transform.position = Vector3.Lerp(startPosition, endPosition, elapsedTime / duration);
                elapsedTime += Time.deltaTime;
                yield return null;
            }
            Destroy(gameObject);
        }
    }
}
