using UnityEngine;
using UnityEngine.SceneManagement;

namespace Script.Room
{
    public class Room1 : MonoBehaviour
    {
 

        private void OnMouseDown()
        {
            Debug.Log($"CLic sur {gameObject.name}");

            string sceneACharger = "";
            
            switch (gameObject.name)
            {
                case "GameObject_Bgauche":
                    sceneACharger = "DirectorCom";
                    break;
                case "GameObject_Bdroit":    
                    sceneACharger = "SecretaireCom";
                    break;
                case "GameObject_Bmil":
                    sceneACharger = "Technicien";
                    break;
                case "GameObject_Bdoor":
                    SceneManager.LoadScene("Map");
                    break;
            }
            
            if (!string.IsNullOrEmpty(sceneACharger))
            {
                // Notifie le GameStateManager du personnage sélectionné
                if (GameStateManager.Instance != null)
                {
                    GameStateManager.Instance.SelectionnerPersonnage(sceneACharger);
                }
                
                SceneManager.LoadScene(sceneACharger);
            }
        }

    }
}