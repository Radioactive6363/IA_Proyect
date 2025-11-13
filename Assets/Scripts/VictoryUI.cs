using UnityEditor.SearchService;
using UnityEngine;
using UnityEngine.SceneManagement;

public class VictoryUI : MonoBehaviour
{
    [SerializeField] private GameObject victoryPanel;
    [SerializeField] private GameObject defeatPanel;


    public void ShowVictory()
    {
        victoryPanel.SetActive(true);
        Time.timeScale = 0f;

    }
    public void ShowDefeat()
    {
        defeatPanel.SetActive(true);
        Time.timeScale = 0f;

    }

    public void Replay()
    {
        Time.timeScale = 1f;

        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);

    }
}
