using UnityEngine;
using TMPro;


public class afficheText : MonoBehaviour
{
    public TextMeshProUGUI texteUI;      // zone où afficher
    public GameObject scrollViewGameObject;

    //public CarnetManager carnet; 
    
    public void BoutonClique()
    {
        if (!CarnetManager.visible)
        {
            scrollViewGameObject.SetActive(true);
            CarnetManager.visible = true;
            if(CarnetManager.Instance != null)
            {
                string resultat = CarnetManager.Instance.afficherCarnet();
                texteUI.text = resultat;
                
            }
            else
            {
                texteUI.text = "instance du carnet incorrect";
            }
        }
        else
        {
            scrollViewGameObject.SetActive(false);
            CarnetManager.visible = false;
        }
    }
        
}
