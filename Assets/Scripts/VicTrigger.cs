using UnityEngine;
using UnityEngine.SceneManagement;

public class VicTrigger : MonoBehaviour
{
    [SerializeField] private string victorySceneName = "Victory";



    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            SceneManager.LoadScene(victorySceneName);

        }
    }
}
