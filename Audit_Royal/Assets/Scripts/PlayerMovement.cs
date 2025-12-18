using System;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.U2D.IK;

/// <summary>
/// Gère le mouvement du joueur en 2D, l'orientation du sprite et l'animation.
/// </summary>
public class PlayerMovement2 : MonoBehaviour
{
    /// <summary>
    /// Vitesse de déplacement du joueur.
    /// </summary>
    public float speed;
    
    /// <summary>
    /// Référence au Rigidbody2D du joueur.
    /// </summary
    public Rigidbody2D rb;
    
    /// <summary>
    /// Référence à l'Animator du joueur pour gérer les animations.
    /// </summary>
    public Animator animator;
    
    /// <summary>
    /// Référence au SpriteRenderer du joueur pour gérer l'orientation.
    /// </summary>
    public SpriteRenderer spriteRenderer;
    
    /// <summary>
    /// Transform du joueur (position).
    /// </summary>
    public Transform PlayerPos;


    /// <summary>
    /// Position calculée pour le mouvement.
    /// </summary>
    private Vector3 pos;
    
    /// <summary>
    /// Vecteur de déplacement calculé depuis l'input.
    /// </summary>
    private Vector2 movement;
    
    /// <summary>
    /// Indique si le joueur fait face à droite (true) ou gauche (false).
    /// </summary>
    private bool facingRight = true;

    /// <summary>
    /// Initialisation du joueur au démarrage de la scène.
    /// Configure le Rigidbody2D et désactive la gravité.
    /// </summary>
    void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        rb.gravityScale = 0; // Pas de gravit�
    }

    /// <summary>
    /// Mise à jour à chaque frame : lecture de l'input, gestion du sprite et des animations.
    /// </summary>
    void Update()
    {
        // Récupère l'input du joueur
        movement.x = Input.GetAxisRaw("Horizontal");
        movement.y = Input.GetAxisRaw("Vertical");
        movement.Normalize();
        
        // Gestion de l'orientation du sprite selon le mouvement horizontal
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

        // Ajustement du flip pour le mouvement vertical
        if((movement.y > 0.1f || movement.y < -0.1f) && facingRight)
        {
            
            spriteRenderer.flipX = false;
        }
        else if((movement.y > 0.1f || movement.y < -0.1f) && !facingRight)
        {
            spriteRenderer.flipX = true; 
        }

        // Mise à jour de la vitesse pour l'Animator
        float characterVelocity = movement.sqrMagnitude;
        animator.SetFloat("Speed", characterVelocity);
    }
    
    /// <summary>
    /// Physique à chaque frame fixe : déplace le joueur en fonction de l'input et de la vitesse.
    /// </summary>
    void FixedUpdate()
    {
        rb.MovePosition(rb.position + movement * speed * Time.fixedDeltaTime);
    }
}


