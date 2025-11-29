using UnityEngine;
using UnityEngine.SceneManagement;

public class CharacterClicker : MonoBehaviour
{
    public string nomScenePersonnage;

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