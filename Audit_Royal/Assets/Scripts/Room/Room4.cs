using UnityEngine;
using UnityEngine.SceneManagement;

namespace Script.Room
{
    public class Room4 : MonoBehaviour
    {
 

        private void OnMouseDown()
        {
            Debug.Log("OnMouseDown");
            switch (gameObject.name)
            {
                case "GameObject_bg":
                    SceneManager.LoadScene("Scenes/Building/B4/Chef");
                    //TODO
                    break;
                case "GameObject_bgm":   
                    SceneManager.LoadScene("Scenes/Building/B4/Secretaire");
                    //TODO
                    break;
                case "GameObject_bmd":
                    //TODO
                    break;
                case "GameObject_bd":
                   
                    break;
                case "GameObject_door":
                    SceneManager.LoadScene("Map");
                    break;
            }
        }

    }
}