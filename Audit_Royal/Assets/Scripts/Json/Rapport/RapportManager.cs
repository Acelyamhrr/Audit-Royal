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
    public GameObject reponsesContent;
	public GameObject content;
	private Dictionary<string, string> questions;
	public GameObject nameAudit;
    
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

        TextMeshProUGUI tmp = this.nameAudit.GetComponent<TextMeshProUGUI>();
        
        if (tmp != null)
        {
	        tmp.text = this.carnetManager.getNameAudit();
        }
        else
        {
	        Debug.LogWarning("Pas de TextMeshProUGUI trouvé sur " + this.nameAudit.name);
        }
		
        this.questions = carnetManager.getAllQuestions();
		createTextQuestionsContent();
    }

	private void createTextQuestionsContent(){
		// Nettoyer le contenu avant de recréer les textes
    	foreach (Transform child in content.transform)
    	{
        	Destroy(child.gameObject);
    	}

    	foreach (KeyValuePair<string, string> question in this.questions)
    	{
			//Créer un panel pour la question et la réponse 
			GameObject panelObj = new GameObject("Panel");
			string service = question.Key.Split(";")[0];
			string numQuestion = question.Key.Split(";")[1];
			panelObj.name = $"Panel_{service}_{numQuestion}";
			panelObj.AddComponent<VerticalLayoutGroup>();
			panelObj.transform.SetParent(content.transform, false);

        	// Créer un nouvel objet texte question
        	GameObject textObj = new GameObject("QuestionText");
        	textObj.transform.SetParent(panelObj.transform, false);

        	// Ajouter le composant TextMeshProUGUI
        	TextMeshProUGUI tmp = textObj.AddComponent<TextMeshProUGUI>();
        	tmp.text = question.Value;

        	// Ajuster la police, la taille, l’alignement
        	tmp.fontSize = 20;
        	tmp.alignment = TextAlignmentOptions.Left;
        	tmp.color = Color.black;
	        
	        // Ajouter un EventTrigger pour gérer le clic
	        EventTrigger trigger = textObj.AddComponent<EventTrigger>();

	        EventTrigger.Entry entry = new EventTrigger.Entry();
	        entry.eventID = EventTriggerType.PointerClick;
	        entry.callback.AddListener((data) => { OnQuestionClicked(panelObj.name); });

	        trigger.triggers.Add(entry);

			// Créer le texte pour la réponse
			GameObject repObj = new GameObject("ReponseText");
			// Ajouter le composant TextMeshProUGUI
        	TextMeshProUGUI tmp2 = repObj.AddComponent<TextMeshProUGUI>();
        	tmp2.text = "-";

        	// Ajuster la police, la taille, l’alignement
        	tmp2.fontSize = 20;
        	tmp2.alignment = TextAlignmentOptions.Left;
        	tmp2.color = Color.black;

			repObj.transform.SetParent(panelObj.transform, false);
    	}
	}

    // Update is called once per frame
    void Update()
    {
		
    }
    
    // Méthode appelée quand on clique sur une question
    private void OnQuestionClicked(string idQuestion)
    {   
        // Nettoyer le contenu avant de recréer les textes
    	foreach (Transform child in reponsesContent.transform)
    	{
        	Destroy(child.gameObject);
    	}

	    //Chercher les infos dans le carnet et les mettre dans reponses
	    string service = idQuestion.Substring(6, idQuestion.Length-8);
	    string question = idQuestion.Substring(idQuestion.Length-1);
	    List<string> reponses = getInfos(service, question);

        foreach(string reponse in reponses)
        {
            GameObject boutonGO = new GameObject("BoutonTMP", typeof(RectTransform), typeof(CanvasRenderer), typeof(Image), typeof(Button));
            boutonGO.name = $"Reponse_{service}_{question}";
            boutonGO.transform.SetParent(reponsesContent.transform, false);

            Image img = boutonGO.GetComponent<Image>();
            img.color = Color.white;
            img.raycastTarget = true;

            RectTransform rt = boutonGO.GetComponent<RectTransform>();
            rt.anchorMin = new Vector2(0.5f, 0.5f);    // centre
            rt.anchorMax = new Vector2(0.5f, 0.5f);
            rt.pivot = new Vector2(0.5f, 0.5f);
            rt.sizeDelta = new Vector2(300, 50);       // largeur/hauteur visibles
            rt.anchoredPosition = Vector2.zero;        // au centre du Canvas

            ContentSizeFitter fitter = boutonGO.AddComponent<ContentSizeFitter>();
            fitter.horizontalFit = ContentSizeFitter.FitMode.Unconstrained; // largeur contrôlée par le layout
            fitter.verticalFit = ContentSizeFitter.FitMode.PreferredSize;   // hauteur auto

            GameObject texteGO = new GameObject("TexteTMP", typeof(RectTransform), typeof(TextMeshProUGUI));
            texteGO.transform.SetParent(boutonGO.transform, false);

            TextMeshProUGUI tmpText = texteGO.GetComponent<TextMeshProUGUI>();
            tmpText.text = reponse;
            tmpText.alignment = TextAlignmentOptions.Left;
            tmpText.fontSize = 15;
            tmpText.color = Color.black;

            RectTransform rtText = texteGO.GetComponent<RectTransform>();
            rtText.anchorMin = Vector2.zero;
            rtText.anchorMax = Vector2.one;
            rtText.offsetMin = Vector2.zero;
            rtText.offsetMax = Vector2.zero;

            LayoutElement le = boutonGO.AddComponent<LayoutElement>();
            le.preferredHeight = tmpText.preferredHeight;
            le.preferredWidth = tmpText.preferredWidth;

            // Ajout du listener sur le bouton
            Button btn = boutonGO.GetComponent<Button>();
            btn.onClick.AddListener(() => OnReponseClicked(boutonGO.name));
        }
    }

    //Méthode appelée quand on clique sur une réponse
    private void OnReponseClicked(string idReponse)
    {
        // Changer le txtreponse par la reponse cliquée
        Transform child = reponsesContent.transform.Find(idReponse);
        if (child != null)
        {
            GameObject obj = child.gameObject;

            //Récupérer le texte de la réponse
            Button btn = obj.GetComponent<Button>();
            TextMeshProUGUI txt = obj.GetComponentInChildren<TextMeshProUGUI>();
            string reponse = txt.text;
            Debug.Log(reponse);

            //Chercher le text de la page correspondant
            string service = idReponse.Substring(8, idReponse.Length-10);
            string numQuestion = idReponse.Substring(idReponse.Length-1);
            string idQuestion = $"Panel_{service}_{numQuestion}";
            Transform child2 = content.transform.Find(idQuestion);

            if(child2 != null)
            {
                GameObject panel = child2.gameObject;

                Transform child3 = panel.transform.Find("ReponseText");

                if(child3 != null)
                {
                    TextMeshProUGUI tmp = child3.GetComponent<TextMeshProUGUI>();
                    tmp.text = reponse;
                    
                    //TODO : changer la couleur du bouton
                }
                else
                {
                    Debug.LogError($"TextMeshPro ReponseText non trouvé.");
                }
            }
            else
            {
                Debug.LogError($"Panel {idQuestion} non trouvé.");
            }
        }
        else
        {
            Debug.LogError($"Bouton {idReponse} non trouvé.");
        }

    }

    private List<string> getInfos(string service, string numQuestion)
    {
        return carnetManager.getInfos(service, numQuestion);
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
