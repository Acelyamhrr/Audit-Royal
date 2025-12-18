using UnityEngine;
using UnityEngine.SceneManagement;

/// <summary>
/// Gère le système de pause global du jeu. 
/// Ce script survit au changement de scènes et permet d'afficher un menu de pause par-dessus le jeu.
/// </summary>
public class GlobalPause : MonoBehaviour
{   
    
    /// <summary>
    /// Instance unique du GlobalPause (Pattern Singleton).
    /// </summary>
    public static GlobalPause instance; 
    
    /// <summary>
    /// État actuel du jeu (vrai si le menu pause est ouvert).
    /// </summary>
    public string sceneDePause = "PauseMenu"; 
    
    
    /// <summary>
    /// État actuel du jeu (vrai si le menu pause est ouvert).
    /// </summary>
    private bool estEnPause = false;

    
    /// <summary>
    /// Initialise le Singleton et rend l'objet immortel entre les scènes.
    /// </summary>
    void Awake()
    {
        if (instance != null)
        {
            Destroy(gameObject); 
            return;
        }
        instance = this;
        Debug.Log($"Name {gameObject.name}");
        DontDestroyOnLoad(gameObject);
    }

    
    /// <summary>
    /// Vérifie à chaque image si l'utilisateur appuie sur la touche Échap.
    /// </summary>
    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            TogglePause();
        }
    }
    
    
    /// <summary>
    /// Alterne entre l'état de pause et l'état de jeu normal.
    /// </summary>
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

    
    /// <summary>
    /// Met le jeu en pause, fige le temps et charge la scène de menu.
    /// </summary>
    void MettreEnPause()
    {
        SceneManager.LoadScene(sceneDePause, LoadSceneMode.Additive);
        Time.timeScale = 0f;
        estEnPause = true;
    }

    
    /// <summary>
    /// Ferme le menu de pause, relance le temps et décharge la scène de menu.
    /// </summary>
    public void ReprendreJeu()
    {
        SceneManager.UnloadSceneAsync(sceneDePause);
        Time.timeScale = 1f; 
        estEnPause = false;
    }
}