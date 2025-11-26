using UnityEngine;
using UnityEngine.SceneManagement;

namespace Script.Room
{
    public class Room4 : MonoBehaviour
    {
 

        private void OnMouseDown()
        {
            Debug.Log($"Clic sur {gameObject.name}");
            
            string sceneACharger = "";
            
            switch (gameObject.name)
            {
                case "GameObject_bg":
                    sceneACharger = "Chef";
                    break;
                case "GameObject_bgm":
                    sceneACharger = "Cuisinier";
                    break;
                case "GameObject_bmd":
                    // Autre personnage si nécessaire
                    break;
                case "GameObject_bd":
                    // Autre personnage si nécessaire
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