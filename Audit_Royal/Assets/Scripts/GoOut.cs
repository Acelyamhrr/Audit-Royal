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
    
    public void SortirDuBatiment()
    {
		Debug.Log("SortirDuBatiment called");
        SceneManager.LoadScene("Map");
    }
}