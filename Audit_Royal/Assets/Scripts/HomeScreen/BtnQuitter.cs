using UnityEngine;

/// <summary>
/// Gestion du bouton permettant de quitter le jeu.
/// </summary>
public class BtnQuitter : MonoBehaviour
{
    /// <summary>
    /// Quitte l'application.
    /// Affiche un message dans la console avant de quitter.
    /// </summary>
    public void Quitter() {
	Debug.Log("Quitter le jeu ...");
        Application.Quit();
    }
}
