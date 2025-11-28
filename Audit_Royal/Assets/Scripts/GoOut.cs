using UnityEngine;
using UnityEngine.SceneManagement;

public class GoOut : MonoBehaviour
{
    void Update()
    {
        if (Input.GetKeyDown(KeyCode.L))
        {
            SortirDuBatiment();
        }
    }
    
    // Fonction publique appelable par un bouton UI
    public void SortirDuBatiment()
    {
        SceneManager.LoadScene("Map");
    }
}