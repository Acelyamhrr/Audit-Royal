using UnityEngine;
using TMPro;


public class afficheText : MonoBehaviour
{
    public TextMeshProUGUI texteUI;      // zone où afficher
    //public CarnetManager carnet; 
    
    public void BoutonClique()
    {
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
}
