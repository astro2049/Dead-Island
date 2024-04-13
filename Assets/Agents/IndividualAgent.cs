using System.Collections.Generic;
using UnityEngine;
using NPBehave;
using UnityEngine.AI;
using UnityEngine.Serialization;

namespace Agents
{
    public enum AgentType
    {
        Zombie = 0,
        Survivor = 1
    }

    public enum LifeStatus
    {
        Alive = 0,
        Dead = 1
    }

    public class IndividualAgent : MonoBehaviour
    {
        public GameManager m_gameManager;

        protected Dictionary<GameObject, HashSet<PerceptionType>> m_targets = new Dictionary<GameObject, HashSet<PerceptionType>>();
        protected GameObject m_target;
        protected NavMeshAgent m_navMeshAgent;
        protected Root m_BT;
        protected AgentType agentType;
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

        protected virtual void clearTarget()
        {
            m_targets.Remove(m_target);
            // Clear flags and m_target
            m_BT.Blackboard["hasATarget"] = false;
            m_target = null;
        }

        protected virtual void setTarget(GameObject agent)
        {
            m_BT.Blackboard["hasATarget"] = true;
            m_target = agent;
        }

        protected void RetargetClosestAgent()
        {
            clearTarget();
            GameObject closestAgent = null;
            float closestDistance = Mathf.Infinity;
            foreach (KeyValuePair<GameObject, HashSet<PerceptionType>> pair in m_targets) {
                var foe = pair.Key;
                float distance = Vector3.Distance(transform.position, foe.transform.position);
                if (distance < closestDistance) {
                    closestDistance = distance;
                    closestAgent = foe;
                }
            }
            if (closestAgent) {
                setTarget(closestAgent);
            }
        }

        public virtual void SenseIndividualAgent(GameObject agent, PerceptionType perception)
        {
            if (!m_targets.ContainsKey(agent)) {
                m_targets.Add(agent, new HashSet<PerceptionType>());
            }
            m_targets[agent].Add(perception);

            // Choose this agent as target, if currently m_target isn't set
            if (!m_target) {
                setTarget(agent);
            }
        }

        public virtual void LosePerceptionOnIndividualAgent(GameObject agent, PerceptionType perception)
        {
            m_targets[agent].Remove(perception);
            if (m_targets[agent].Count == 0) {
                if (agent == m_target) {
                    RetargetClosestAgent();
                } else {
                    m_targets.Remove(agent);
                }
            }
        }

        public virtual void Die()
        {
            if (lifeStatus == LifeStatus.Dead) {
                return;
            }
            m_navMeshAgent.enabled = false;
            transform.Rotate(new Vector3(0, 90, 0));
            DeactivateBT();
            if (agentType == AgentType.Zombie) {
                m_gameManager.m_zombieCount--;
                m_gameManager.UpdateZombieCountText();
            } else {
                m_gameManager.m_survivorCount--;
                m_gameManager.UpdateSurvivorCountText();
            }
        }
    }
}
