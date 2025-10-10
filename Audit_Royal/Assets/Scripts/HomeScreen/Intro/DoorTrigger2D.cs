using UnityEngine;

public class DoorTrigger2D : MonoBehaviour
{
    public RectTransform player;
    public string nextScene = "Bureau";
    private SceneTransition transition;

    void Start()
    {
        transition = FindObjectOfType<SceneTransition>();
    }

    void Update()
    {
        if (Vector2.Distance(player.anchoredPosition, GetComponent<RectTransform>().anchoredPosition) < 50f)
        {
            transition.FadeAndLoadScene(nextScene);
        }
    }
}
