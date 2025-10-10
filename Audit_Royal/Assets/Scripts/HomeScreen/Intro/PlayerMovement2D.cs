using UnityEngine;

public class PlayerMovement2D : MonoBehaviour
{
    public float speed = 200f; 
    private RectTransform rect;

    void Start()
    {
        rect = GetComponent<RectTransform>();
    }

    void Update()
    {
        float h = Input.GetAxis("Horizontal"); 
        float v = Input.GetAxis("Vertical");   

        Vector3 move = new Vector3(h, v, 0) * speed * Time.deltaTime;
        rect.anchoredPosition += new Vector2(move.x, move.y);
    }
}
