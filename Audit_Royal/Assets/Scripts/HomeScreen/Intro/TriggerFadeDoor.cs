using UnityEngine;

public class TriggerFadeDoor : MonoBehaviour
{
    public SceneTransition fadeScript; // Ton script SceneTransition
    public string sceneToLoad = "Bureau";

    private bool triggered = false;

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (triggered) return; // pour éviter plusieurs déclenchements
        if (other.CompareTag("Player"))
        {
            triggered = true;
            fadeScript.FadeAndLoadScene(sceneToLoad);
        }
    }
}
