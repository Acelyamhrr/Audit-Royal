using UnityEngine;

/// <summary>
/// Gère les actions des boutons à l'intérieur du menu de pause.
/// </summary>
public class BoutonPause : MonoBehaviour
{
    
    /// <summary>
    /// Ferme le menu de pause et reprend la partie.
    /// Appelé par le bouton "Play" ou "Resume".
    /// </summary>
    public void ClicReprendre()
    {
        if (GlobalPause.instance != null)
        {
            GlobalPause.instance.ReprendreJeu();
        }
    }
    
    /// <summary>
    /// Ferme complètement l'application (Alt+F4).
    /// Arrête également le mode lecture si exécuté dans l'éditeur Unity.
    /// </summary>
    public void ClicQuitter()
    {
        Application.Quit();
        #if UNITY_EDITOR
                UnityEditor.EditorApplication.isPlaying = false;
        #endif
    }
    
    
}