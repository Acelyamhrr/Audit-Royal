using System;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.U2D.IK;

public class PlayerMovement2 : MonoBehaviour
{
    public float speed;
    public Rigidbody2D rb;
    public Animator animator;
    public SpriteRenderer spriteRenderer;
    public Transform PlayerPos;



    private Vector3 pos;
    private Vector2 movement;
    private bool facingRight = true;

    void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        rb.gravityScale = 0; // Pas de gravit�
    }

    void Update()
    {
        movement.x = Input.GetAxisRaw("Horizontal");
        movement.y = Input.GetAxisRaw("Vertical");
        movement.Normalize();
        

        if (movement.x > 0.1f)
        {
            spriteRenderer.flipX = false; // vers la droite
            facingRight = true;
        }else if (movement.x < -0.1f)
        {
            spriteRenderer.flipX = true; // vers la gauche
            facingRight = false;
        }
        else
        {
            // Si le joueur s'arr�te, on garde la derni�re orientation
            spriteRenderer.flipX = facingRight;
        }

        if((movement.y > 0.1f || movement.y < -0.1f) && facingRight)
        {
            
            spriteRenderer.flipX = false;
        }
        else if((movement.y > 0.1f || movement.y < -0.1f) && !facingRight)
        {
            spriteRenderer.flipX = true; 
        }

        float characterVelocity = movement.sqrMagnitude;
        animator.SetFloat("Speed", characterVelocity);
    }
    
    void FixedUpdate()
    {
        rb.MovePosition(rb.position + movement * speed * Time.fixedDeltaTime);
    }
}


