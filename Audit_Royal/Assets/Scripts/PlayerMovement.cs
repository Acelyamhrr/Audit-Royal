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
        rb.gravityScale = 0; // Pas de gravité
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
            // Si le joueur s'arrête, on garde la dernière orientation
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


        // Gérer les limites du personnages sur l'axe des Y

        if (movement.y > 0.1f)
        {
            if (PlayerPos.position.y > 4.6f)
            {
                pos.x = PlayerPos.position.x;
                pos.y = 4.6f;
                PlayerPos.position = pos;
            }
        }
        else if(movement.y < 0.1f)
        {
            if (PlayerPos.position.y < -4.6f)
            {
                pos.x = PlayerPos.position.x;
                pos.y = -4.6f;
                PlayerPos.position = pos;
            }
        }

        // Gérer les limites du personnages sur l'axe des X

        if (movement.x > 0.1f)
        {
            if (PlayerPos.position.x > 9.3f)
            {
                pos.x = 9.3f;
                pos.y = PlayerPos.position.y;
                PlayerPos.position = pos;
            }
        }
        else if (movement.x < 0.1f)
        {
            if (PlayerPos.position.x < -9.3f)
            {
                pos.x = -9.3f;
                pos.y = PlayerPos.position.y;
                PlayerPos.position = pos;
            }
        }
    }
    
    void FixedUpdate()
    {
        rb.MovePosition(rb.position + movement * speed * Time.fixedDeltaTime);
    }

    private void LimitY(float lim)
    {
        if (PlayerPos.position.y > lim || PlayerPos.position.y < lim)
        {
            pos.x = PlayerPos.position.x;
            pos.y = lim;
            PlayerPos.position = pos;
        }
    }
    private void LimitX(float lim)
    {
        if (PlayerPos.position.x > lim || PlayerPos.position.x < lim)
        {
            pos.x = lim;
            pos.y = PlayerPos.position.y;
            PlayerPos.position = pos;
        }
    }
}


