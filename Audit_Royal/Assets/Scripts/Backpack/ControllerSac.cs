using UnityEngine;

/// <summary>
/// Contrôle l'ouverture et la fermeture du sac à dos du joueur via l'Animator.
/// </summary>
public class ControllerSac : MonoBehaviour
{
	/// <summary>
	/// Référence à l'Animator attaché au sac à dos.
	/// </summary>
	public Animator backPackAnimator;
	
	/// <summary>
	/// Indique si le sac est ouvert (true) ou fermé (false).
	/// </summary>
	private bool isOpen=false;

	/// <summary>
	/// Bascule l'état du sac à dos : ouvert ou fermé.
	/// Met à jour le bool "isOpen" dans l'Animator.
	/// </summary>
	public void ToggleMenu() {
		isOpen = !isOpen;
		backPackAnimator.SetBool("isOpen", isOpen);
	}
    
}
