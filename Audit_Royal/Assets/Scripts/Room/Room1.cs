using UnityEngine;
using UnityEngine.SceneManagement;

namespace Script.Room
{
    public class Room1 : MonoBehaviour
    {
 

        private void OnMouseDown()
        {
            Debug.Log("OnMouseDown");
            switch (gameObject.name)
            {
                case "GameObject_Bgauche":
                    Debug.Log(gameObject.name + " bgauche");
                    //TODO
                    break;
                case "GameObject_Bdroit":    
                    //TODO
                    break;
                case "GameObject_Bbas":
                    //TODO
                    break;
            }
        }

    }
}