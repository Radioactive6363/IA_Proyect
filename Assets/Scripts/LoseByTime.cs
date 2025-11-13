using UnityEngine;
using UnityEngine.SceneManagement;
using TMPro;

public class LoseByTime : MonoBehaviour
{
    [SerializeField] private string defeatSceneName = "Defeat";
    [SerializeField] private float timelimit = 30f;
    [SerializeField] private TextMeshProUGUI timerText;

    private float timer;

    private void Start()
    {
        timer = timelimit;
        UpdateTimerUI();


    }

    private void Update()
    {
        timer -= Time.deltaTime;
        if (timer < 0)
        {
            SceneManager.LoadScene(defeatSceneName);

        }
    }
    private void UpdateTimerUI()
    {
        if (timerText != null)
        {
            timerText.text = "Time: " + Mathf.Max(0, timer).ToString("F1");
        }
    }
}
