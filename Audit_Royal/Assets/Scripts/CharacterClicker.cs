using UnityEngine;
using UnityEngine.SceneManagement;

/// Attaché sur le bouton / GameObject du personnage dans le prefab service
public class CharacterClicker : MonoBehaviour
{
    [Tooltip("Clé de scène/personnage telle que dans GameStateManager.sceneVersPersonnage (ex: 'TechnicienInfo').")]
    public string nomScenePersonnage;

    // Méthode publique à appeler depuis le Button.OnClick (ou depuis EventTrigger)
    public void OnClick()
    {
        if (GameStateManager.Instance != null)
        {
            GameStateManager.Instance.SelectionnerPersonnage(nomScenePersonnage);
        }
        else
        {
            Debug.LogError("GameStateManager introuvable (CharacterClicker).");
        }

        // Charger la scène qui contient DialogueUIManager
        SceneManager.LoadScene("SceneDialogue");
    }
}