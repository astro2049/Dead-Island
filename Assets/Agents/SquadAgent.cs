using System;
using UnityEngine;
namespace Agents
{
    public enum SquadRole
    {
        Leader = 0,
        Follower = 1
    }

    public class SquadMember
    {
        public GameObject survivor;
        public SquadRole squadRole;
        public Vector3 positioningOffset;
    }

    public class SquadAgent
    {
        public SquadMember[] m_members = new SquadMember[4];
        public int m_memberCount = 0;

        public SquadAgent()
        {
            m_members[0] = new SquadMember { squadRole = SquadRole.Leader };
            m_members[1] = new SquadMember { squadRole = SquadRole.Follower, positioningOffset = new Vector3(2, 0, -2) };
            m_members[2] = new SquadMember { squadRole = SquadRole.Follower, positioningOffset = new Vector3(-2, 0, -2) };
            m_members[3] = new SquadMember { squadRole = SquadRole.Follower, positioningOffset = new Vector3(0, 0, -6) };
        }

        public void AddSurvivor(GameObject survivor)
        {
            var survivorAI = survivor.GetComponent<SurvivorAI>();
            // Find a spot fot the new member
            for (int i = 0; i < 4; i++) {
                if (!m_members[i].survivor) {
                    m_members[i].survivor = survivor;
                    survivorAI.JoinSquad(this, i);
                    break;
                }
            }
            m_memberCount++;
        }

        public void RemoveSurvivor(GameObject survivor)
        {
            var survivorAI = survivor.GetComponent<SurvivorAI>();
            m_members[survivorAI.m_squadMemberID].survivor = null;
            survivorAI.QuitSquad();
            m_memberCount--;
        }
    }
}
