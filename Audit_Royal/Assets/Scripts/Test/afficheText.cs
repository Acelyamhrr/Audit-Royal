using UnityEngine;
using TMPro;


public class afficheText : MonoBehaviour
{
    public TextMeshProUGUI texteUI;      // zone où afficher
    public CarnetManager carnet; 
    
    public void BoutonClique()
    {
        string resultat = carnet.afficherCarnet();
        texteUI.text = resultat;
    }
}
