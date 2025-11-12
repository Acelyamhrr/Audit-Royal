using UnityEngine;

public class BtnQuitter : MonoBehaviour
{
    public void Quitter() {
	Debug.Log("Quitter le jeu ...");
        Application.Quit();
    }
}
