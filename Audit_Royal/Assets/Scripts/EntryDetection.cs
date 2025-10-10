using UnityEngine;
using UnityEngine.SceneManagement;

public class EntryDetection : MonoBehaviour
{
    private Collider2D zoneCollider;
    private Collider2D playerCollider;
    
    private bool hasLoadedScene = false; // pour �viter de recharger en boucle

    private void Start()
    {
        zoneCollider = GetComponent<Collider2D>();
    }

    private void Update()
    {
        if (!hasLoadedScene && playerCollider != null && zoneCollider.OverlapPoint(playerCollider.bounds.center))
        {
            hasLoadedScene = true; // emp�cher rechargement
            switch (gameObject.name)
            {
                case "EntryZoneB1":
                    SceneManager.LoadScene("InsideBuilding1");
                    break;
                case "EntryZoneB2":
                    SceneManager.LoadScene("InsideBuilding2");
                    break;
                case "EntryZoneB3":
                    SceneManager.LoadScene("InsideBuilding3");
                    break;
                case "EntryZoneB4":
                    SceneManager.LoadScene("InsideBuilding4");
                    break;
            }
        }
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
            playerCollider = other;
    }
    
    
    
    
        
    private void OnTriggerExit2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {
            playerCollider = null;
            hasLoadedScene = false;
        }
    }
}
