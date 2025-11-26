using UnityEngine;
using UnityEngine.SceneManagement;

namespace Script.Room
{
    public class Room2 : MonoBehaviour
    {
 

        private void OnMouseDown()
        {
            Debug.Log($"Clic sur {gameObject.name}");
            
            string sceneACharger = "";
            
            switch (gameObject.name)
            {
                case "GameObject_Bg":
                    sceneACharger = "SecretaireCom"; // ou créer une scène spécifique
                    break;
                case "GameObject_Bd":
                    sceneACharger = "Technicien";
                    break;
                case "GameObject_Hg":
                    sceneACharger = "Reseau";
                    break;
                case "GameObject_Hd":
                    sceneACharger = "Director";
                    break;
                case "GameObject_door":
                    SceneManager.LoadScene("Map");
                    return;
            }
            
            if (!string.IsNullOrEmpty(sceneACharger))
            {
                if (GameStateManager.Instance != null)
                {
                    GameStateManager.Instance.SelectionnerPersonnage(sceneACharger);
                }
                
                SceneManager.LoadScene(sceneACharger);
            }
        }

    }
}