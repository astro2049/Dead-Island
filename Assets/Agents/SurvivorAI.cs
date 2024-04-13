using NPBehave;
using UnityEngine;
using UnityEngine.AI;
using UnityEngine.UIElements;
using Action = NPBehave.Action;

namespace Agents
{
    public class SurvivorAI : IndividualAgent
    {
        public Vector3 m_spawnTransform;
        public Transform m_safeZoneTransform;
        public GameObject m_rifle;
        private readonly float fireCooldown = 1.0f;
        private float currentFireCooldown = 0.0f;
        private bool isFacingTarget = false;

        private void Start()
        {
            m_spawnTransform = transform.position;
            agentType = AgentType.Survivor;
            m_navMeshAgent = GetComponent<NavMeshAgent>();
            InitializeBT();
        }

        private void InitializeBT()
        {
            m_BT = new Root(
                new Selector(
                    new BlackboardCondition("hasATarget", Operator.IS_EQUAL, false, Stops.IMMEDIATE_RESTART,
                        new Action(NavigateToSafeZone)
                    ),
                    new Sequence(
                        new Action(TurnTowardsTarget),
                        new Selector(
                            new Condition(() => isFacingTarget && currentFireCooldown <= 0,
                                new Action(Shoot)),
                            new Condition(() => Vector3.Distance(transform.position, m_target.transform.position) <= 7,
                                new Action(BackUp))
                        )
                    )
                )
            );
            m_BT.Blackboard["hasATarget"] = false;
        }

        private void TurnTowardsTarget()
        {
            m_navMeshAgent.ResetPath();
            var targetDirection = m_target.transform.position - transform.position;
            float angle = Vector3.Angle(transform.forward, targetDirection);
            var targetRotation = Quaternion.LookRotation(targetDirection);
            transform.rotation = Quaternion.RotateTowards(transform.rotation, targetRotation, 360 * Time.deltaTime);
            if (angle < 10) {
                isFacingTarget = true;
            }
        }

        private void BackUp()
        {
            var targetDirection = m_target.transform.position - transform.position;
            m_navMeshAgent.SetDestination(transform.position - targetDirection);
            m_navMeshAgent.updateRotation = false;
        }

        private void Shoot()
        {
            m_rifle.GetComponent<AudioSource>().Play();
            m_rifle.GetComponent<ParticleSystem>().Play();
            currentFireCooldown = fireCooldown;
            m_target.GetComponent<IndividualAgent>().Die();
            RetargetClosestAgent();
        }

        private void NavigateToSafeZone()
        {
            if (!m_navMeshAgent.hasPath) {
                m_navMeshAgent.SetDestination(m_safeZoneTransform.position);
                m_navMeshAgent.updateRotation = true;
            }
        }

        protected override void clearTarget()
        {
            base.clearTarget();
            isFacingTarget = false;
        }

        private void Update()
        {
            if (currentFireCooldown > 0) {
                currentFireCooldown -= Time.deltaTime;
            }
        }
    }
}
