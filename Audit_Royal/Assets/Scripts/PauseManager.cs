using UnityEngine;

public class PauseManager : MonoBehaviour
{
    // Cette variable statique permet de s'assurer qu'il n'y a qu'un seul PauseManager
    public static PauseManager instance;

    [Header("Glisse ici ton Panel de Menu Pause")]
    public GameObject pauseMenuUI;

    private bool isPaused = false;

    void Awake()
    {
        if (instance != null)
        {
            Destroy(gameObject); 
            return;
        }
        instance = this;
        Debug.Log($"Name {gameObject.name}");
        //DontDestroyOnLoad(gameObject); // ne pas supprimer l'objet quand on change de scene 
    }

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            if (isPaused)
            {
                Resume();
            }
            else
            {
                Pause();
            }
        }
    }

    public void Resume()
    {
        pauseMenuUI.SetActive(false); 
        Time.timeScale = 1f;        
        isPaused = false;
    }

    void Pause()
    {
        pauseMenuUI.SetActive(true);  
        Time.timeScale = 0f;          
        isPaused = true;
    }
}