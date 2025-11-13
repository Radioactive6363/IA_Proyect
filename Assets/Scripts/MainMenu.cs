using UnityEngine;
using UnityEngine.SceneManagement;

public class MainMenu : MonoBehaviour
{
   public void Play()
    {
        SceneManager.LoadScene("Level2");
    }

    public void Exit()
    {
        Debug.Log("saliendo del juego...");
        Application.Quit();

    }
}
