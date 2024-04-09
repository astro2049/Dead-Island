using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class VisualPerception : MonoBehaviour
{
    public class AuralPerception : MonoBehaviour
    {
        private void OnTriggerEnter(Collider other)
        {
            if (other.CompareTag("Survivor")) {
                transform.parent.GetComponent<ZombieAI>().gainPerceptionOnSurvivor(other.gameObject, PerceptionType.Visual);
            }
        }

        private void OnTriggerExit(Collider other)
        {
            if (other.CompareTag("Survivor")) {
                transform.parent.GetComponent<ZombieAI>().losePerceptionOnSurvivor(other.gameObject, PerceptionType.Visual);
            }
        }
    }
}
