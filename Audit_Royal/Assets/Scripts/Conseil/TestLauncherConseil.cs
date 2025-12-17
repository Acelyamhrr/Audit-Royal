using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class TestLauncherConseil : MonoBehaviour
{
    [Header("Paramètres de test")]
    [Range(0, 100)]
    public int scoreTest = 85;  // Change cette valeur dans l'Inspector pour tester différents scores
    
    [Header("Références")]
    public Button boutonLancer;
    
    void Start()
    {
        // Assigner le score de test au GameStateManager
        if (GameStateManager.Instance != null)
        {
            GameStateManager.Instance.ScoreDernierRapport = scoreTest;
            Debug.Log($"Score de test défini : {scoreTest}%");
        }
        
        // Ajouter le listener au bouton
        if (boutonLancer != null)
        {
            boutonLancer.onClick.AddListener(LancerConseilAdmin);
        }
    }
    
    public void LancerConseilAdmin()
    {
        // Mettre à jour le score avant de charger (au cas où tu l'as changé dans l'Inspector)
        if (GameStateManager.Instance != null)
        {
            GameStateManager.Instance.ScoreDernierRapport = scoreTest;
            Debug.Log($"Chargement de ConseilAdmin avec score : {scoreTest}%");
        }
        
        // Charger la scène
        SceneManager.LoadScene("ConseilAdmin");
    }
}