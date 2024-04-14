using NPBehave;
using UnityEngine;
using UnityEngine.AI;
using Action = NPBehave.Action;

namespace Agents
{
    public class SurvivorAI : IndividualAgent
    {
        // Squad
        public SquadAgent m_squadAgent;
        public int m_squadMemberID = -1;
        private SquadMember m_squadMember;

        public Transform m_safeZoneTransform;
        public GameObject m_rifle;
        private readonly float fireCooldown = 1.0f;
        private float currentFireCooldown = 0.0f;
        private bool isFacingTarget = false;

        private void Start()
        {
            agentType = AgentType.Survivor;
            m_navMeshAgent = GetComponent<NavMeshAgent>();

            InitializeBT();
        }

        // Squad
        public void JoinSquad(SquadAgent squadAgent, int squadMemberID)
        {
            m_squadAgent = squadAgent;
            m_squadMemberID = squadMemberID;
            m_squadMember = m_squadAgent.m_members[m_squadMemberID];
        }

        public void QuitSquad()
        {
            m_squadAgent = null;
            m_squadMemberID = -1;
            m_squadMember = null;
        }

        private GameObject GetLeader()
        {
            return m_squadAgent.m_members[0].survivor;
        }

        private void InitializeBT()
        {
            m_BT = new Root(
                new Selector(
                    new BlackboardCondition("hasATarget", Operator.IS_EQUAL, false, Stops.IMMEDIATE_RESTART,
                        new Selector(
                            new Condition(() => m_squadMember.squadRole == SquadRole.Leader,
                                new Action(NavigateToSafeZone)),
                            new Action(NavigateToStickWithLeader)
                        )
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

        private void NavigateToSafeZone()
        {
            if (!m_navMeshAgent.hasPath) {
                m_navMeshAgent.SetDestination(m_safeZoneTransform.position);
                m_navMeshAgent.updateRotation = true;
            }
        }

        private void NavigateToStickWithLeader()
        {
            m_navMeshAgent.SetDestination(GetLeader().transform.TransformPoint(m_squadMember.positioningOffset));
            m_navMeshAgent.updateRotation = true;
        }

        private void TurnTowardsTarget()
        {
            m_navMeshAgent.ResetPath();
            if (isFacingTarget) {
                return;
            }
            var targetDirection = m_target.transform.position - transform.position;
            float angle = Vector3.Angle(transform.forward, targetDirection);
            var targetRotation = Quaternion.LookRotation(targetDirection);
            transform.rotation = Quaternion.RotateTowards(transform.rotation, targetRotation, 360 * Time.deltaTime);
            if (angle <= 10) {
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

        public override void Die()
        {
            base.Die();
            m_squadAgent.RemoveSurvivor(transform.gameObject);
        }
    }
}
