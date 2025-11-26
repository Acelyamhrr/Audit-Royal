using UnityEngine;
using UnityEngine.SceneManagement;

namespace Script.Room
{
    public class Room3 : MonoBehaviour
    {
 

        private void OnMouseDown()
        {
            Debug.Log($"Clic sur {gameObject.name}");
            
            string sceneACharger = "";
            
            switch (gameObject.name)
            {
                case "GameObject_bg":
                    sceneACharger = "ComptaPatron";
                    break;
                case "GameObject_bd":
                    sceneACharger = "ComptaComptable";
                    break;
                case "GameObject_bm":
                    sceneACharger = "ComptaSecretaire";
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