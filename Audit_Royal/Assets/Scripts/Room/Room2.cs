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
                    //TODO
                    
                    break;
                case "GameObject_Bd":    
                    //TODO
                    break;
                case "GameObject_Hg":
                    //TODO
                    break;
                case "GameObject_Hd":
                    //TODO
                    break;
                case "GameObject_door":
                    SceneManager.LoadScene("Map");
                    break;
            }
        }

    }
}