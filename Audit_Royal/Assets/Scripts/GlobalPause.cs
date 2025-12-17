using UnityEngine;
using UnityEngine.SceneManagement;

public class GlobalPause : MonoBehaviour
{
    public static GlobalPause instance; 
    public string sceneDePause = "PauseMenu"; 
    
    private bool estEnPause = false;

    void Awake()
    {
        if (instance != null)
        {
            Destroy(gameObject); 
            return;
        }
        instance = this;
        DontDestroyOnLoad(gameObject);
    }

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            TogglePause();
        }
    }

    public void TogglePause()
    {
        if (estEnPause)
        {
            ReprendreJeu();
        }
        else
        {
            MettreEnPause();
        }
    }

    void MettreEnPause()
    {
        SceneManager.LoadScene(sceneDePause, LoadSceneMode.Additive);
        Time.timeScale = 0f;
        estEnPause = true;
    }

    public void ReprendreJeu()
    {
        SceneManager.UnloadSceneAsync(sceneDePause);
        Time.timeScale = 1f; 
        estEnPause = false;
    }
}