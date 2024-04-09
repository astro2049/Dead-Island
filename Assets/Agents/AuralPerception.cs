using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AuralPerception : MonoBehaviour
{
    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Survivor")) {
            transform.parent.GetComponent<ZombieAI>().gainPerceptionOnSurvivor(other.gameObject, PerceptionType.Aural);
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("Survivor")) {
            transform.parent.GetComponent<ZombieAI>().losePerceptionOnSurvivor(other.gameObject, PerceptionType.Aural);
        }
    }
}
