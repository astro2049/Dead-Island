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
                    survivorAI.m_squadAgent = this;
                    survivorAI.m_squadMemberID = i;
                    m_members[i].survivor = survivor;
                    break;
                }
            }
            m_memberCount++;
        }

        public void RemoveSurvivor(GameObject leaver)
        {
            var leaverAI = leaver.GetComponent<SurvivorAI>();
            int leaverSquadID = leaverAI.m_squadMemberID;
            m_members[leaverAI.m_squadMemberID].survivor = null;
            leaverAI.m_squadAgent = null;
            leaverAI.m_squadMemberID = -1;
            // If the leader has left, we're gonna have to find another one
            if (leaverSquadID == 0) {
                for (int i = 3; i >= 1; i--) {
                    if (m_members[i].survivor) {
                        m_members[i].survivor.GetComponent<SurvivorAI>().m_squadMemberID = 0;
                        m_members[0].survivor = m_members[i].survivor;
                        m_members[i].survivor = null;
                        break;
                    }
                }
            }
            m_memberCount--;
        }
    }
}
