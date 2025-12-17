using UnityEngine; 
using UnityEngine.Video; 
using UnityEngine.SceneManagement;

/// <summary>
/// Gère la lecture d'une vidéo avec VideoPlayer et passe à une scène suivante lorsque la vidéo se termine.
/// À attacher sur un GameObject contenant un VideoPlayer.
/// </summary>
public class VideoLoader : MonoBehaviour 
{
	/// <summary>
	/// Composant VideoPlayer à utiliser pour lire la vidéo.
	/// </summary>
	public VideoPlayer videoPlayer; 
	
	/// <summary>
	/// Nom de la scène à charger après la fin de la vidéo.
	/// </summary>
	public string nextSceneName = "MainMenu"; 
	
	/// <summary>
	/// Initialise le VideoPlayer, prépare la vidéo et configure les événements pour jouer et détecter la fin.
	/// </summary>
	void Start() { 
		videoPlayer.Prepare();
    		videoPlayer.prepareCompleted += (vp) => videoPlayer.Play();
    		videoPlayer.loopPointReached += OnVideoEnd;	
	} 
	
	/// <summary>
	/// Appelé lorsque la vidéo est terminée. Charge la scène suivante.
	/// </summary>
	/// <param name="vp">Le VideoPlayer ayant terminé la lecture.</param>
	void OnVideoEnd(VideoPlayer vp) { 
		SceneManager.LoadScene(nextSceneName); 
	}
}
