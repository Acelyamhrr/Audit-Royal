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
                    Debug.Log(gameObject.name + " bgauche");
                    //TODO
                    break;
                case "GameObject_bgm":    
                    //TODO
                    break;
                case "GameObject_bmd":
                    //TODO
                    break;
                case "GameObject_bd":
                    //TODO
                    break;
                case "GameObject_door":
                    SceneManager.LoadScene("Map");
                    break;
            }
        }

    }
}