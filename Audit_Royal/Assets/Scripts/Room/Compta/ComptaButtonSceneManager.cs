using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.SceneManagement;

public class ComptaButtonSceneManager : MonoBehaviour
{
    public void buttonClicked()
    {
        GameObject clickedButton = EventSystem.current.currentSelectedGameObject;
        switch (clickedButton.name)
        {
            case "BtnExit":
                SceneManager.LoadScene("Compta");
                break;
            case "BtnExitMap":
                SceneManager.LoadScene("Map");
                break;
            case "BtnDirector":
                SceneManager.LoadScene("ComptaDirector");
                break;
            case "BtnComptable":
                SceneManager.LoadScene("ComptaComptable");
                break;
            case "BtnSecretaire":
                SceneManager.LoadScene("ComptaSecretaire");
                break;
        }
    }
}
