using System.Collections.Generic;
using UnityEngine;
using NPBehave;
using UnityEngine.AI;

namespace Agents
{
    public enum LifeStatus
    {
        Alive = 0,
        Dead = 1
    }

    public class IndividualAgent : MonoBehaviour
    {
        protected Dictionary<GameObject, HashSet<PerceptionType>> m_targets = new Dictionary<GameObject, HashSet<PerceptionType>>();
        protected GameObject m_target;
        protected NavMeshAgent m_navMeshAgent;
        protected Root m_BT;
        public LifeStatus lifeStatus = LifeStatus.Alive;

        public void ActivateBT()
        {
            m_BT.Start();
        }

        public void DeactivateBT()
        {
            if (m_BT.CurrentState == Node.State.ACTIVE) {
                lifeStatus = LifeStatus.Dead;
                m_BT.Stop();
            }
        }

        protected virtual void clearTarget(GameObject agent)
        {
            // Clear flags and m_target
            m_BT.Blackboard["hasATarget"] = false;
            m_target = null;
        }

        protected virtual void setTarget(GameObject agent)
        {
            m_BT.Blackboard["hasATarget"] = true;
            m_target = agent;
        }

        protected void TargetClosestAgent()
        {
            GameObject closestAgent = null;
            float closestDistance = Mathf.Infinity;
            foreach (KeyValuePair<GameObject, HashSet<PerceptionType>> pair in m_targets) {
                var zombie = pair.Key;
                float distance = Vector3.Distance(transform.position, zombie.transform.position);
                if (distance < closestDistance) {
                    closestDistance = distance;
                    closestAgent = zombie;
                }
            }
            if (closestAgent) {
                setTarget(closestAgent);
            }
        }

        public virtual void SenseIndividualAgent(GameObject agent, PerceptionType perception)
        {
            if (!m_targets.ContainsKey(agent)) {
                Debug.Log("Spotted");
                m_targets.Add(agent, new HashSet<PerceptionType>());
            }
            m_targets[agent].Add(perception);

            // Choose this agent as target, if currently m_target isn't set
            if (!m_target) {
                setTarget(agent);
            }
        }

        public virtual void UnsenseIndividualAgent(GameObject agent, PerceptionType perception)
        {
            m_targets[agent].Remove(perception);
            if (m_targets[agent].Count == 0) {
                Debug.Log("Unspotted");
                m_targets.Remove(agent);
            }

            if (agent == m_target) {
                Debug.Log("Lost target");
                clearTarget(agent);

                // Find a new target (closest agent)
                TargetClosestAgent();
            }
        }

        public virtual void Die()
        {
            m_navMeshAgent.enabled = false;
            // m_navMeshAgent.ResetPath();
            transform.Rotate(new Vector3(0, 90, 0));
            DeactivateBT();
        }
    }
}
