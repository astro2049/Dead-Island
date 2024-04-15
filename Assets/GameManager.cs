using System.Collections.Generic;
using Agents;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class GameManager : MonoBehaviour
{
    public GameObject m_startButton;
    public GameObject m_toggleSoundButtonText;

    public List<GameObject> m_survivors = new List<GameObject>();
    public List<GameObject> m_zombies = new List<GameObject>();

    public int m_zombieCount, m_survivorCount;
    public GameObject m_zombieCountText, m_survivorCountText;
    public GameObject m_roundOverText;

    public SquadAgent squadAgent = new SquadAgent();

    public void ActivateActors()
    {
        m_startButton.GetComponent<Button>().interactable = false;
        foreach (var survivor in m_survivors) {
            var survivorAI = survivor.GetComponent<SurvivorAI>();
            survivorAI.ActivateBT();
        }
        foreach (var zombie in m_zombies) {
            var zombieAI = zombie.GetComponent<ZombieAI>();
            zombieAI.ActivateBT();
        }
    }

    public void UpdateZombieCountText()
    {
        m_zombieCountText.GetComponent<TextMeshPro>().text = m_zombieCount.ToString();
    }

    public void UpdateSurvivorCountText()
    {
        m_survivorCountText.GetComponent<TextMeshPro>().text = m_survivorCount.ToString();
        if (m_survivorCount == 0) {
            m_roundOverText.GetComponent<TextMeshPro>().text = "Game Over.";
        }
    }

    public void ReloadCurrentScene()
    {
        SceneManager.LoadScene(SceneManager.GetActiveScene().name);
    }

    public void toggleAudio()
    {
        AudioListener.volume = 1 - AudioListener.volume;
        m_toggleSoundButtonText.GetComponent<TextMeshProUGUI>().text = "Sound: " + (AudioListener.volume == 1 ? "On" : "Off");
    }
}
