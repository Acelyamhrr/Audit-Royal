using UnityEngine;

public class AudioManager : MonoBehaviour
{
    public static AudioManager instance;

    [Header("Configuration Audio")]
    public AudioSource musiqueSource; 
    public AudioClip musiqueDeFond;  

    void Awake()
    {
        if (instance != null)
        {
            Destroy(gameObject);
            return;
        }

        instance = this;
        DontDestroyOnLoad(gameObject);
    }

    void Start()
    {
        if (musiqueSource != null && musiqueDeFond != null)
        {
            musiqueSource.clip = musiqueDeFond;
            musiqueSource.loop = true; 
            musiqueSource.Play();
        }
    }
}