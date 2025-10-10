using UnityEngine; 
using UnityEngine.Video; 
using UnityEngine.SceneManagement;

public class VideoLoader : MonoBehaviour { 
	public VideoPlayer videoPlayer; 
	public string nextSceneName = "MainMenu"; 
       	
	void Start() { 
		videoPlayer.Prepare();
    		videoPlayer.prepareCompleted += (vp) => videoPlayer.Play();
    		videoPlayer.loopPointReached += OnVideoEnd;	
	} 
	
	void OnVideoEnd(VideoPlayer vp) { 
		SceneManager.LoadScene(nextSceneName); 
	}
}
