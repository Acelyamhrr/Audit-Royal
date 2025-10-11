using UnityEngine;

public class DoorTrigger2D : MonoBehaviour
{
    public RectTransform player;
    public string nextScene = "Bureau";
    public float triggerDistance = 50f;

    private SceneTransition transition;
    private bool hasTriggered = false;

    void Start()
    {
        transition = FindObjectOfType<SceneTransition>();
    }

    void Update()
    {
        if (hasTriggered) return; 

        float distance = Vector2.Distance(player.anchoredPosition, GetComponent<RectTransform>().anchoredPosition);
        if (distance < triggerDistance)
        {
            hasTriggered = true;
            transition.FadeAndLoadScene(nextScene);
        }
    }
}
