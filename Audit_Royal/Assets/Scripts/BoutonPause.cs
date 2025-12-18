using UnityEngine;

public class BoutonPause : MonoBehaviour
{
    public void ClicReprendre()
    {
        if (GlobalPause.instance != null)
        {
            GlobalPause.instance.ReprendreJeu();
        }
    }

    public void ClicQuitter()
    {
        Application.Quit();
        #if UNITY_EDITOR
                UnityEditor.EditorApplication.isPlaying = false;
        #endif
    }
    
    
}