using UnityEngine;

public class BoutonPause : MonoBehaviour
{
    public void ClicReprendre()
    {
        if (GlobalPause.instance != null)
        {
            GlobalPause.instance.ReprendreJeu();
        }
    }

    public void ClicQuitter()
    {
        Time.timeScale = 1f;
        UnityEngine.SceneManagement.SceneManager.LoadScene("MainMenu");
    }
}