using UnityEngine;
using UnityEngine.SceneManagement;

/// <summary>
/// Gère le clic sur un personnage dans la scène.
/// Met à jour le personnage sélectionné dans le GameStateManager et charge la scène de dialogue.
/// </summary>
public class CharacterClicker : MonoBehaviour
{
    /// <summary>
    /// Nom de la scène ou identifiant du personnage associé à ce GameObject.
    /// Correspond au nom utilisé dans GameStateManager pour récupérer le fichier JSON du personnage.
    /// </summary>
    public string nomScenePersonnage;

    /// <summary>
    /// Méthode appelée lors du clic sur le personnage.
    /// Sélectionne le personnage dans le GameStateManager et charge la scène de dialogue.
    /// </summary>
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

        SceneManager.LoadScene("SceneDialogue");
    }
}