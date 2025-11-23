using UnityEngine;

public class BuildingTrigger : MonoBehaviour
{
	    [Header("Configuration")]
    public string nomService = "comptabilite"; // "communication", "comptabilite", "info", "restauration", "technicien"

    private ServiceUIManager serviceUI;

    void Start()
    {
        serviceUI = FindFirstObjectByType<ServiceUIManager>();

        // Vérifier que c'est bien un trigger
        Collider2D col = GetComponent<Collider2D>();
        if (col != null)
        {
            col.isTrigger = true;
        }
    }

    void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {
            Debug.Log($"Joueur entre dans: {nomService}");

            if (serviceUI != null)
            {
                serviceUI.ChargerService(nomService);
            }
        }
    }

    void OnTriggerExit2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {
            Debug.Log($"Joueur sort de: {nomService}");

            if (serviceUI != null)
            {
                serviceUI.CacherService();
            }
        }
    }
}
