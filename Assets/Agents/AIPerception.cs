using UnityEngine;
namespace Agents
{
    public enum PerceptionType
    {
        Visual = 0,
        Aural = 1
    }

    public class AIPerception : MonoBehaviour
    {
        public PerceptionType m_perception;
        private IndividualAgent individualAI;

        private void Start()
        {
            individualAI = transform.parent.transform.parent.gameObject.GetComponent<IndividualAgent>();
        }

        private void OnTriggerEnter(Collider other)
        {
            var otherGameObject = other.transform.parent.gameObject;
            if (otherGameObject.GetComponent<IndividualAgent>().lifeStatus == LifeStatus.Alive && !otherGameObject.CompareTag(transform.tag)) {
                individualAI.SenseIndividualAgent(otherGameObject, m_perception);
            }
        }

        private void OnTriggerExit(Collider other)
        {
            var otherGameObject = other.transform.parent.gameObject;
            if (otherGameObject.GetComponent<IndividualAgent>().lifeStatus == LifeStatus.Alive && !otherGameObject.CompareTag(transform.tag)) {
                individualAI.UnsenseIndividualAgent(otherGameObject, m_perception);
            }
        }
    }
}
