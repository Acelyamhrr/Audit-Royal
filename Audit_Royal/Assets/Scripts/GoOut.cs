using UnityEngine;
using UnityEngine.SceneManagement;

/// <summary>
/// Permet au joueur de sortir d'un bâtiment et de revenir à la carte.
/// </summary>
public class GoOut : MonoBehaviour
{
    /// <summary>
    /// Vérifie à chaque frame si le joueur appuie sur la touche pour sortir.
    /// </summary>
    void Update()
    {
        if (Input.GetKeyDown(KeyCode.L))
        {
            SortirDuBatiment();
        }
    }
    
    /// <summary>
    /// Charge la scène "Map" pour retourner à la carte.
    /// </summary>
    public void SortirDuBatiment()
    {
		Debug.Log("SortirDuBatiment called");
        SceneManager.LoadScene("Map");
    }
}