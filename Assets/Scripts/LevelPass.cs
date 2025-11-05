using UnityEngine;

public class LevelPass : MonoBehaviour
{
    private void OnTriggerEnter(Collider collider)
    {
        Application.ForceCrash(0);
    }
}
