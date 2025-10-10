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
                case "GameObject_Bgauche":
                    //TODO
                    SceneManager.LoadScene("Map");
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