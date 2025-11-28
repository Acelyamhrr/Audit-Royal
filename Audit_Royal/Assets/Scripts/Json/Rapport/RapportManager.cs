using UnityEngine;
using System;
using Newtonsoft.Json.Linq;
using System.IO;
using System.Collections.Generic;
using TMPro;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class RapportManager : MonoBehaviour
{
    private int nbInfosVraies;          //Nombre total d'infos vraies
    private string fileTrue;
    private CarnetManager carnetManager;
    public GameObject reponses;
	public GameObject content;
    private int currentQuestion;
	private List<string> questions;
    
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        this.fileTrue = Path.Combine(Application.streamingAssetsPath, "JSON/scenario_verites.json");
        this.carnetManager = FindFirstObjectByType<CarnetManager>();

		if(carnetManager == null){
			GameObject go = new GameObject("CarnetManager");
			carnetManager = go.AddComponent<CarnetManager>();
		}
        
        string json = File.ReadAllText(this.fileTrue);
        JObject obj = JObject.Parse(json);

        this.nbInfosVraies = 0;

        foreach (var service in (JObject)obj["verites"])
        {
            var postes = (JObject)((JObject)service.Value)["postes"];

            foreach (var poste in postes.Properties())
            {
                var questions = (JObject)((JObject)poste.Value)["verites"];

                foreach (var question in questions.Properties())
                {
                    nbInfosVraies += ((JArray)question.Value).Count;
                }
            }
        }

		//TODO : GetName audit
		
		this.questions = carnetManager.getAllQuestions();
		createTextQuestionsContent();


		this.currentQuestion = 0;
    }

	private void createTextQuestionsContent(){
		// Nettoyer le contenu avant de recréer les textes
    	foreach (Transform child in content.transform)
    	{
        	Destroy(child.gameObject);
    	}

    	foreach (string question in this.questions)
    	{
			//Créer un panel pour la question et la réponse 
			GameObject panelObj = new GameObject("PanelText");
			panelObj.transform.SetParent(content.transform, false);

        	// Créer un nouvel objet texte question
        	GameObject textObj = new GameObject("QuestionText");
        	textObj.transform.SetParent(panelObj.transform, false);

        	// Ajouter le composant TextMeshProUGUI
        	TextMeshProUGUI tmp = textObj.AddComponent<TextMeshProUGUI>();
        	tmp.text = question;

        	// Ajuster la police, la taille, l’alignement
        	tmp.fontSize = 20;
        	tmp.alignment = TextAlignmentOptions.Left;
        	tmp.color = Color.black;

			
    	}
	}

    // Update is called once per frame
    void Update()
    {
		/*
        List<string> lst = getInfos(this.currentQuestion);

        foreach (string info in lst)
        {
            GameObject boutonGO = new GameObject("BoutonTMP", typeof(RectTransform), typeof(CanvasRenderer), typeof(Image), typeof(Button));
            boutonGO.transform.SetParent(reponses.transform, false);
            Image img = boutonGO.GetComponent<Image>();
            img.color = Color.white;
            GameObject texteGO = new GameObject("TexteTMP", typeof(RectTransform), typeof(TextMeshProUGUI));
            texteGO.transform.SetParent(boutonGO.transform, false);

            TextMeshProUGUI tmpText = texteGO.GetComponent<TextMeshProUGUI>();
            tmpText.text = info;
            tmpText.alignment = TextAlignmentOptions.Center;
            tmpText.fontSize = 24;
            
            RectTransform rtText = texteGO.GetComponent<RectTransform>();
            rtText.anchorMin = Vector2.zero;
            rtText.anchorMax = Vector2.one;
            rtText.offsetMin = Vector2.zero;
            rtText.offsetMax = Vector2.zero;

            // 6. Ajouter un listener au bouton
            Button btn = boutonGO.GetComponent<Button>();
            btn.onClick.AddListener(() => Debug.Log($"Bouton {info} cliqué !"));
        }
        */
    }

    private List<string> getInfos(int numQuestion)
    {
        return carnetManager.getInfos(numQuestion);
    }

    //Renvoie si la réponse est vraie ou fausse
    private bool checkTrue(Service service, Metier metier, int numQuestion, int numInfo)
    {
        string json = File.ReadAllText(this.fileTrue);
        JObject obj = JObject.Parse(json);

        string _service = service.ToString().ToLower();
        string _metier = metier.ToString().ToLower();
        
        JArray liste = (JArray) obj["verites"][_service][_metier][numQuestion];
        
        return liste.Contains(new JValue(numInfo));
    }
}
