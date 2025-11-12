using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
namespace Script.Room.B2
{
        public class ButtonHandlerB3 : MonoBehaviour
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
                    case "button_hl":

                        SceneManager.LoadScene("Bureau");
                        break;
                    
                    case "button_bl":
                    {
                        
                        break;
                    }
                    case "button_hr":
                    {
                        break;
                    }
                    case "button_br":
                    {
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