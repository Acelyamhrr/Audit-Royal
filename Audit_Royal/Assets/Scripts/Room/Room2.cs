using UnityEngine;
using UnityEngine.SceneManagement;

namespace Script.Room
{
    public class Room2 : MonoBehaviour
    {
 

        private void OnMouseDown()
        {
            Debug.Log("OnMouseDown");
            switch (gameObject.name)
            {
                case "GameObject_Bg":
                    SceneManager.LoadScene("Scenes/Building/B2/SecretaireCom");
                    
                    break;
                case "GameObject_Bd":    
                    SceneManager.LoadScene("Scenes/Building/B2/Technicien");
                    break;
                case "GameObject_Hg":
                    SceneManager.LoadScene("Scenes/Building/B2/Reseau");
                    break;
                case "GameObject_Hd":
                    SceneManager.LoadScene("Scenes/Building/B2/Director");
                    break;
                case "GameObject_door":
                    SceneManager.LoadScene("Map");
                    break;
            }
        }

    }
}