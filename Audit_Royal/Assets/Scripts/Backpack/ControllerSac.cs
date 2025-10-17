using UnityEngine;

public class ControllerSac : MonoBehaviour
{
	public Animator backPackAnimator;
	private bool isOpen=false;

	public void ToggleMenu() {
		isOpen = !isOpen;
		backPackAnimator.SetBool("isOpen", isOpen);
	}
    
}
