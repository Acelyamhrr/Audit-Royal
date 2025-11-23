using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
namespace Script.Room.B2
{
        public class ButtonHandlerB1 : MonoBehaviour
        {
           public Button button;

            public void Start()
            {
                button.onClick.AddListener(() => buttonClicked(button));
            }

            public void buttonClicked(Button buttonHandler)
            {
                switch (buttonHandler.name)
                {
                    case "button_bm":
                        SceneManager.LoadScene("Bureau");
                        break;
                    
                    case "button_bd":
                    {
                        // todo
                        break;
                    }
                    case "button_bg":
                    {
                        // todo
                        break;
                    }
                    case "button_door":
                    {
                        SceneManager.LoadScene("Scenes/Map");
                        break;
                    }
                }
            }
        }
}