using UnityEngine;
using System;
using Newtonsoft.Json.Linq;
using System.Linq;
using System.IO;
using System.Collections.Generic;
using TMPro;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class RapportManager : MonoBehaviour
{
    //private int nbInfosVraies;          //Nombre total d'infos vraies
    private int scoreTotal;

    private string fileTrue;
    private CarnetManager carnetManager;
    public GameObject reponsesContent;
	public GameObject content;
	private Dictionary<string, string> questions;
	public GameObject nameAudit;
    public GameObject btnValider;
    
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        this.fileTrue = Path.Combine(Application.streamingAssetsPath, "JSON/scenario_verites.json");

        //Récupère le carnetManager
        this.carnetManager = FindFirstObjectByType<CarnetManager>();

		if(carnetManager == null){
			GameObject go = new GameObject("CarnetManager");
			carnetManager = go.AddComponent<CarnetManager>();
		}
        
        /*
        //Compte le nombre de vérités
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
        */

        //Affiche le nom de l'audit
        TextMeshProUGUI tmp = this.nameAudit.GetComponent<TextMeshProUGUI>();
        
        if (tmp != null)
        {
	        tmp.text = this.carnetManager.getNameAudit();
        }
        else
        {
	        Debug.LogWarning("Pas de TextMeshProUGUI trouvé sur " + this.nameAudit.name);
        }

        // Ajout du listener sur le bouton
        Button btn = btnValider.GetComponent<Button>();
        btn.onClick.AddListener(() => OnValidationClicked());

		//Stretch du content des questions
		RectTransform contentRT = content.GetComponent<RectTransform>();

		// Stretch horizontal
		contentRT.anchorMin = new Vector2(0, 1);
		contentRT.anchorMax = new Vector2(1, 1);

		// Left = 0, Right = 0
		contentRT.offsetMin = new Vector2(0, contentRT.offsetMin.y);
		contentRT.offsetMax = new Vector2(0, contentRT.offsetMax.y);
		
        //Affiche les questions
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
			tmp.enableAutoSizing = true;
            tmp.fontSizeMin = 20;
            tmp.fontSizeMax = 50;
        	tmp.alignment = TextAlignmentOptions.Left;
        	tmp.color = Color.black;
            tmp.fontStyle = FontStyles.Bold;
			tmp.textWrappingMode = TextWrappingModes.Normal;

			//Rectransform
			RectTransform rt = textObj.GetComponent<RectTransform>();
        	rt.anchorMin = new Vector2(0, 1);   // stretch horizontal, aligné en haut
        	rt.anchorMax = new Vector2(1, 1);
        	rt.pivot = new Vector2(0.5f, 1f);   // pivot en haut-centre
        	rt.offsetMin = new Vector2(0, 0);   // Left = 0
        	rt.offsetMax = new Vector2(0, 0);   // Right = 0
        	rt.sizeDelta = new Vector2(0, 60);  // hauteur fixe
	        
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
        	tmp2.text = "";

        	// Ajuster la police, la taille, l’alignement
        	tmp2.fontSize = 20;
			tmp2.enableAutoSizing = true;
            tmp2.fontSizeMin = 20;
            tmp2.fontSizeMax = 50;
        	tmp2.alignment = TextAlignmentOptions.Left;
        	tmp2.color = Color.black;
			tmp2.textWrappingMode = TextWrappingModes.Normal;

			//Rectransform
			RectTransform rt2 = repObj.GetComponent<RectTransform>();
        	rt2.anchorMin = new Vector2(0, 1);   // stretch horizontal, aligné en haut
        	rt2.anchorMax = new Vector2(1, 1);
        	rt2.pivot = new Vector2(0.5f, 1f);   // pivot en haut-centre
        	rt2.offsetMin = new Vector2(0, 0);   // Left = 0
        	rt2.offsetMax = new Vector2(0, 0);   // Right = 0
        	rt2.sizeDelta = new Vector2(0, 60);  // hauteur fixe

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
            //Créer un bouton de la réponse
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
            rt.sizeDelta = new Vector2(900, 100);       // largeur/hauteur visibles
            rt.anchoredPosition = Vector2.zero;        // au centre du Canvas

            GameObject texteGO = new GameObject("TexteTMP", typeof(RectTransform), typeof(TextMeshProUGUI));
            texteGO.transform.SetParent(boutonGO.transform, false);

            // Texte
            TextMeshProUGUI tmpText = texteGO.GetComponent<TextMeshProUGUI>();
            tmpText.text = reponse;
            tmpText.color = Color.black;
            tmpText.alignment = TextAlignmentOptions.Left;
            tmpText.enableAutoSizing = true;
            tmpText.fontSizeMin = 8;
            tmpText.fontSizeMax = 30;
            tmpText.textWrappingMode = TextWrappingModes.Normal; // autorise le retour à la ligne
            tmpText.overflowMode = TextOverflowModes.Ellipsis;   // ajoute "..." si trop long

            RectTransform rtText = texteGO.GetComponent<RectTransform>();
            rtText.anchorMin = Vector2.zero;
            rtText.anchorMax = Vector2.one;
            rtText.offsetMin = Vector2.zero;
            rtText.offsetMax = Vector2.zero;

            // Bouton : taille imposée
            LayoutElement le = boutonGO.AddComponent<LayoutElement>();
            le.preferredHeight = 100;   // fixe la hauteur
            le.preferredWidth = 900;   // fixe la largeur


            // Ajout du listener sur le bouton
            Button btn = boutonGO.GetComponent<Button>();
            btn.onClick.AddListener(() => OnReponseClicked(boutonGO.name, reponse));
        }
    }

    //Méthode appelée quand on clique sur une réponse : changer le txtreponse par la reponse cliquée
    private void OnReponseClicked(string idReponse, string reponse)
    {
        //Chercher le text de la page correspondant
        string service = idReponse.Substring(8, idReponse.Length-10);
        string numQuestion = idReponse.Substring(idReponse.Length-1);
        string idQuestion = $"Panel_{service}_{numQuestion}";
        Transform child = content.transform.Find(idQuestion);

        if(child != null)
        {
            GameObject panel = child.gameObject;

            Transform child2 = panel.transform.Find("ReponseText");

            if(child2 != null)
            {
                TextMeshProUGUI tmp = child2.GetComponent<TextMeshProUGUI>();
                tmp.text = "-> " + reponse;
                
                //TODO : changer la couleur du bouton
                /*Transform childBouton = reponsesContent.transform.Find(idReponse);
                if(childBouton != null)
                {
                    GameObject goBouton = childBouton.gameObject;
                    Image img = goBouton.GetComponent<Image>();
                    img.color = Color.gray;
                }
                else
                {
                    Debug.LogError($"Bouton {idReponse} non trouvé.");
                }*/
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

    private List<string> getInfos(string service, string numQuestion)
    {
        return carnetManager.getInfos(service, numQuestion);
    }

    private void OnValidationClicked()
    {
        int nbQuestions = this.questions.Count;
        int score=0;
        //Pour chaque question
        Transform contentTransform = content.transform;
        foreach(Transform panel in contentTransform)
        {
            //Debug.Log("Panel trouvé : " + panel.name);
            string service = panel.name.Substring(6, panel.name.Length-8);
	        string numQuestion = panel.name.Substring(panel.name.Length-1);
            //Debug.Log($"Service {service} : numQuestion {numQuestion}");

            // Récupère tous les TMP dans le panel
            TextMeshProUGUI[] texts = panel.GetComponentsInChildren<TextMeshProUGUI>();
            TextMeshProUGUI secondText = texts[1];

            //Récupère le service, métier et numéro de l'info
            string info = secondText.text;
            info = info.Replace("->", "");
            if(info == "")
            {
                //Si l'on a pas mis d'info, on passe à la suivante
                continue;
            }

            string numInfo = "";
            string metier = "";

            if(service == carnetManager.getServiceAudite())
            {
                string retour = carnetManager.getNumInfo(service, info);
                metier = retour.Split(";")[0];
                numInfo = retour.Split(";")[1];
            }
            else
            {
                List<string> services = carnetManager.getServices();
                services.Remove(carnetManager.getServiceAudite());

                foreach(string serv in services)
                {
                    string retour = carnetManager.getNumInfo(serv, info);
                    if(retour != "-1")
                    {
                        service = serv;
                        metier = retour.Split(";")[0];
                        numInfo = retour.Split(";")[1];
                        break;
                    }
                }
            }
            
            //Si elle est vraie, score+1
            if(checkTrue(service, metier, numQuestion, int.Parse(numInfo)))
            {
                score++;
            }

        }

        //Calcul du score total
        this.scoreTotal = score*100/nbQuestions;
        Debug.Log($"Score {scoreTotal}% !");
    }

    //Renvoie si la réponse est vraie ou fausse
    private bool checkTrue(string service, string metier, string numQuestion, int numInfo)
    {
        string json = File.ReadAllText(this.fileTrue);
        JObject obj = JObject.Parse(json);
        
        JArray liste = (JArray) obj["verites"][service]["postes"][metier]["verites"][numQuestion];
        
	    return liste.Any(x => (int)x == numInfo);
    }
}
