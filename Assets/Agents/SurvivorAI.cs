using System.Collections;
using System.Collections.Generic;
using NPBehave;
using UnityEngine;
using UnityEngine.AI;
using Random = UnityEngine.Random;

public class SurvivorAI : MonoBehaviour
{
    public Transform safeZoneLocation;
    private NavMeshAgent m_navMeshAgent;
    private Root m_BT;

    private void Start()
    {
        m_navMeshAgent = GetComponent<NavMeshAgent>();
        InitializeBT();
    }

    private void InitializeBT()
    {
        m_BT = new Root(
            new Action(NavigateToSafeZone)
        );
    }

    public void ActivateBT()
    {
        m_BT.Start();
    }

    public void DeactivateBT()
    {
        m_BT.Stop();
    }

    private void NavigateToSafeZone()
    {
        if (!m_navMeshAgent.hasPath) {
            m_navMeshAgent.SetDestination(safeZoneLocation.position);
        }
    }
}
