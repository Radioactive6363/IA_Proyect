using UnityEngine;
using UnityEngine.SceneManagement;

public class MainMenu : MonoBehaviour
{
   public void PlayGame()
    {
        SceneManager.LoadScene("Level");
    }


    public void ExitGame()
    {
        Application.Quit();

        Debug.Log("Juego Cerrado");
    }

    public void Reiniciar()
    {
        Time.timeScale = 1.0f;
        SceneManager.LoadScene("Menu");
    }
}
