using UnityEngine;
using UnityEngine.SceneManagement;

public class EntryDetection : MonoBehaviour
{
    private Collider2D zoneCollider;
    private Collider2D playerCollider;

    private bool hasLoadedScene = false; 
    private string sceneACharger = "";

    private void Start()
    {
        zoneCollider = GetComponent<Collider2D>();
    }

    private void Update()
    {
        if (!hasLoadedScene && playerCollider != null && zoneCollider.OverlapPoint(playerCollider.bounds.center))
        {
            hasLoadedScene = true;

            switch (gameObject.name)
            {
                case "EntryZoneCrous":
                    sceneACharger = "Crous";
                    break;

                case "EntryZoneInfo":
                    sceneACharger = "Informatique";
                    break;

                case "EntryZoneCompta":
                    sceneACharger = "Compta";
                    break;

                case "EntryZoneCom":
                    sceneACharger = "Communication";
                    break;

                case "EntryZoneBTP":
                    sceneACharger = "Techniciens";
                    break;
            }

            if (!string.IsNullOrEmpty(sceneACharger))
            {
                if (GameStateManager.Instance != null)
                {
                    GameStateManager.Instance.EntrerDansBatiment(sceneACharger);
                }

                SceneManager.LoadScene(sceneACharger);
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