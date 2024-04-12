using NPBehave;
using UnityEngine;
using UnityEngine.AI;
using Action = NPBehave.Action;

namespace Agents
{
    public class SurvivorAI : IndividualAgent
    {
        public Transform m_safeZoneTransform;
        public GameObject m_rifle;
        private readonly float fireCooldown = 1.0f;
        private float currentFireCooldown = 0.0f;

        private void Start()
        {
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
                        new BlackboardCondition("isFacingTarget", Operator.IS_EQUAL, true, Stops.IMMEDIATE_RESTART,
                            new Action(Shoot))
                    ))
            );
            m_BT.Blackboard["hasATarget"] = false;
            m_BT.Blackboard["isFacingTarget"] = false;
        }

        private void TurnTowardsTarget()
        {
            m_navMeshAgent.ResetPath();
            var targetDirection = m_target.transform.position - transform.position;
            float angle = Vector3.Angle(transform.forward, targetDirection);
            if (angle < 10) {
                m_BT.Blackboard["isFacingTarget"] = true;
                return;
            }
            var targetRotation = Quaternion.LookRotation(targetDirection);
            transform.rotation = Quaternion.RotateTowards(transform.rotation, targetRotation, 240 * Time.deltaTime);
        }

        private void Shoot()
        {
            if (currentFireCooldown <= 0) {
                m_rifle.GetComponent<AudioSource>().Play();
                m_rifle.GetComponent<ParticleSystem>().Play();
                currentFireCooldown = fireCooldown;
                Debug.Log("Shot fired");
                m_target.GetComponent<IndividualAgent>().Die();
                m_targets.Remove(m_target);
                clearTarget(m_target);
                TargetClosestAgent();
            }
        }

        private void NavigateToSafeZone()
        {
            if (!m_navMeshAgent.hasPath) {
                m_navMeshAgent.SetDestination(m_safeZoneTransform.position);
            }
        }

        protected override void clearTarget(GameObject agent)
        {
            base.clearTarget(agent);
            m_BT.Blackboard["isFacingTarget"] = false;
        }

        protected override void setTarget(GameObject agent)
        {
            Debug.Log("Zombie spotted");
            base.setTarget(agent);
        }

        private void Update()
        {
            if (currentFireCooldown > 0) {
                currentFireCooldown -= Time.deltaTime;
            }
        }
    }
}
