using UnityEngine;
using TMPro;
using System.Collections.Generic;
using System.IO;
using Newtonsoft.Json;
using UnityEngine.EventSystems;
using System.Linq;

public class GameManager : MonoBehaviour
{
    private ScenarioRoot scenarioData; 
    private ScenarioManager scenarioManager;

    public TextMeshProUGUI texteDialogue;  
    
    public List<int> scenariosDisponibles = new List<int> { 1, 2 };
    
    private JsonDialogueManager dialogueManager;
    private PersonnageManager personnageManager;
    
    private enum EtatJeu 
    { 
        ChoixScenario, 
        ChoixService, 
        ChoixPoste, 
        ChoixQuestion, 
        AffichageReponse, 
        AffichageVerites,
        FinNiveau
    }
    private EtatJeu etatActuel;
    
    private int scenarioSelectionne;
    private int niveauActuel = 1; // Commence au niveau 1
    private string serviceSelectionne;
    private string posteSelectionne;
    private string fichierPersonnageSelectionne;
    
    private List<string> servicesDisponibles = new List<string>();
    private Dictionary<string, List<PosteInfo>> postes = new Dictionary<string, List<PosteInfo>>();
    
    // Stocke les questions disponibles selon le niveau
    private List<string> questionsDisponiblesPourPersonnage = new List<string>();
    
    void Start()
    {
        dialogueManager = FindFirstObjectByType<JsonDialogueManager>();
        if (dialogueManager == null)
        {
            GameObject go = new GameObject("DialogueManager");
            dialogueManager = go.AddComponent<JsonDialogueManager>();
        }
        
        personnageManager = FindFirstObjectByType<PersonnageManager>();
        if (personnageManager == null)
        {
            GameObject go = new GameObject("PersonnageManager");
            personnageManager = go.AddComponent<PersonnageManager>();
        }
        
        scenarioManager = FindFirstObjectByType<ScenarioManager>();
        if (scenarioManager == null)
        {
            GameObject go = new GameObject("ScenarioManager");
            scenarioManager = go.AddComponent<ScenarioManager>();
        }
        
        InitialiserPostes();
        AfficherChoixScenario();
    }
    
    void InitialiserPostes()
    {
        postes["communication"] = new List<PosteInfo>
        {
            new PosteInfo("Graphiste", "com_graphiste.json"),
            new PosteInfo("Responsable Réseaux Sociaux", "com_responsable_reseaux_sociaux.json"),
            new PosteInfo("Technicien son/vidéo", "com_technicien_son_video.json")
        };
        
        postes["comptabilite"] = new List<PosteInfo>
        {
            new PosteInfo("Comptable", "compta_comptable.json"),
            new PosteInfo("Patron", "compta_patron.json"),
            new PosteInfo("Secrétaire", "compta_secretaire.json")
        };
        
        postes["technicien"] = new List<PosteInfo>
        {
            new PosteInfo("Concierge", "gc_concierge.json"),
            new PosteInfo("Patron", "gc_patron.json"),
            new PosteInfo("Paysagiste", "gc_paysagiste.json"),
            new PosteInfo("Secrétaire", "gc_secretaire.json")
        };
        
        postes["info"] = new List<PosteInfo>
        {
            new PosteInfo("Patron", "info_patron.json"),
            new PosteInfo("Responsable Réseau", "info_responsable_reseau.json"),
            new PosteInfo("Secrétaire", "info_secretaire.json"),
            new PosteInfo("Technicien de Maintenance", "info_technicien_de_maintenance.json")
        };
        
        postes["restauration"] = new List<PosteInfo>
        {
            new PosteInfo("Cuisinier", "res_cuisinier.json"),
            new PosteInfo("Patron", "res_patron.json")
        };
    }
    
    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Escape) && etatActuel != EtatJeu.ChoixScenario)
        {
            Application.Quit();
            #if UNITY_EDITOR
            UnityEditor.EditorApplication.isPlaying = false;
            #endif
            return;
        }
        
        switch (etatActuel)
        {
            case EtatJeu.ChoixScenario:
                GererChoixScenario();
                break;
                
            case EtatJeu.ChoixService:
                GererChoixService();
                break;
                
            case EtatJeu.ChoixPoste:
                GererChoixPoste();
                break;
                
            case EtatJeu.ChoixQuestion:
                GererChoixQuestion();
                break;
                
            case EtatJeu.AffichageReponse:
                GererAffichageReponse();
                break;
                
            case EtatJeu.AffichageVerites:
                GererAffichageVerites();
                break;
                
            case EtatJeu.FinNiveau:
                GererFinNiveau();
                break;
        }
    }
    
    // ========== CHOIX DU SCÉNARIO ==========
    
    void AfficherChoixScenario()
    {
        etatActuel = EtatJeu.ChoixScenario;
        niveauActuel = 1; // Reset au niveau 1
        
        string texte = "=== CHOISISSEZ UN SCÉNARIO ===\n\n";
        
        for (int i = 0; i < scenariosDisponibles.Count; i++)
        {
            texte += $"[{i + 1}] Scénario {scenariosDisponibles[i]}\n";
        }
        
        texteDialogue.text = texte;
        Debug.Log("Affichage du choix de scénario");
    }
    
    void GererChoixScenario()
    {
        for (int i = 0; i < scenariosDisponibles.Count; i++)
        {
            if (Input.GetKeyDown(KeyCode.Alpha1 + i))
            {
                scenarioSelectionne = scenariosDisponibles[i];
                Debug.Log($"Scénario {scenarioSelectionne} sélectionné");
                ChargerScenario(scenarioSelectionne);
                DetecterServicesDisponibles();
                AfficherChoixService();
            }
        }
    }

    void ChargerScenario(int numeroScenario)
    {
        string nomFichier = $"scenario{numeroScenario}.json";
        string filePath = Path.Combine(Application.streamingAssetsPath, nomFichier);
        
        if (!File.Exists(filePath))
        {
            Debug.LogError($"Fichier scénario introuvable : {filePath}");
            return;
        }
        
        string jsonContent = File.ReadAllText(filePath);
        scenarioData = Newtonsoft.Json.JsonConvert.DeserializeObject<ScenarioRoot>(jsonContent);
        
        Debug.Log($"Scénario '{scenarioData.titre}' chargé !");
        
        // Génération des vérités pour le niveau actuel
        Debug.Log($"Génération des vérités pour le scénario {numeroScenario}, niveau {niveauActuel}...");
        scenarioManager.GenerateVeritesFile(numeroScenario, niveauActuel);
    }
    
    void DetecterServicesDisponibles()
    {
        servicesDisponibles.Clear();
        
        string[] servicesAPotentiels = { "communication", "comptabilite", "info", "restauration", "technicien" };
        
        foreach (string service in servicesAPotentiels)
        {
            string nomFichier = $"scenario{scenarioSelectionne}_{service}.json";
            string cheminComplet = Path.Combine(Application.streamingAssetsPath, nomFichier);
            
            if (File.Exists(cheminComplet))
            {
                servicesDisponibles.Add(service);
                Debug.Log($"Service trouvé : {service}");
            }
        }
        
        if (servicesDisponibles.Count == 0)
        {
            Debug.LogError($"AUCUN service trouvé pour le scénario {scenarioSelectionne} !");
            texteDialogue.text = $"ERREUR : Aucun fichier de dialogue trouvé pour le scénario {scenarioSelectionne}\n\n[Échap] Quitter";
        }
    }
    
    // ========== CHOIX DU SERVICE ==========
    
    void AfficherChoixService()
    {
        etatActuel = EtatJeu.ChoixService;
        
        string texte = $"=== NIVEAU {niveauActuel} ===\n\n";
        texte += $"MISSION : Vous devez auditer le service {scenarioData.service_audite.ToUpper()}\n\n";
        texte += "Services disponibles :\n\n";
        
        for (int i = 0; i < servicesDisponibles.Count; i++)
        {
            string serviceNom = servicesDisponibles[i].ToUpper();
            
            // Vérifier si le service est accessible selon le niveau
            bool estAccessible = EstServiceAccessible(servicesDisponibles[i]);
            
            if (estAccessible)
            {
                texte += $"[{i + 1}] {serviceNom}";
                if (servicesDisponibles[i] == scenarioData.service_audite)
                {
                    texte += " ⭐ (SERVICE AUDITÉ)";
                }
                texte += "\n";
            }
            else
            {
                texte += $"[X] {serviceNom} (Verrouillé pour ce niveau)\n";
            }
        }
        
        texte += "\n[V] Voir les vérités\n";
        texte += "[F] Terminer le niveau\n";
        texte += "\n[0] Retour au choix du scénario\n";
        texte += "\n[Échap] Quitter\n";
        
        texteDialogue.text = texte;
    }
    
    bool EstServiceAccessible(string service)
    {
        // Niveau 1 et 2 : Seulement le service audité
        if (niveauActuel == 1 || niveauActuel == 2)
        {
            return service == scenarioData.service_audite;
        }
        
        // Niveaux 3, 4, 5 : Tous les services
        return true;
    }
    
    void GererChoixService()
    {
        if (Input.GetKeyDown(KeyCode.Alpha0))
        {
            AfficherChoixScenario();
            return;
        }
        
        if (Input.GetKeyDown(KeyCode.V))
        {
            AfficherVerites();
            return;
        }
        
        if (Input.GetKeyDown(KeyCode.F))
        {
            AfficherFinNiveau();
            return;
        }
        
        for (int i = 0; i < servicesDisponibles.Count; i++)
        {
            if (Input.GetKeyDown(KeyCode.Alpha1 + i))
            {
                string service = servicesDisponibles[i];
                
                if (!EstServiceAccessible(service))
                {
                    Debug.Log($"Service {service} non accessible pour le niveau {niveauActuel}");
                    return;
                }
                
                serviceSelectionne = service;
                Debug.Log($"Service sélectionné : {serviceSelectionne}");
                AfficherChoixPoste();
                return;
            }
        }
    }
    
    // ========== AFFICHAGE DES VÉRITÉS ==========
    
    void AfficherVerites()
    {
        etatActuel = EtatJeu.AffichageVerites;
        
        string filePath = Path.Combine(Application.persistentDataPath, "GameData", "scenario_verites.json");
        
        if (!File.Exists(filePath))
        {
            texteDialogue.text = "ERREUR : Fichier des vérités introuvable !\n\n[Espace] Retour";
            Debug.LogError($"Fichier vérités introuvable : {filePath}");
            return;
        }
        
        string jsonContent = File.ReadAllText(filePath);
        VeritesScenarioRoot verites = JsonConvert.DeserializeObject<VeritesScenarioRoot>(jsonContent);
        
        string texte = $"=== VÉRITÉS DU SCÉNARIO {verites.scenario} - NIVEAU {verites.niveau} ===\n\n";
        
        foreach (var serviceEntry in verites.verites)
        {
            texte += $"--- {serviceEntry.Key.ToUpper()} ---\n";
            
            foreach (var posteEntry in serviceEntry.Value.postes)
            {
                texte += $"  • {posteEntry.Key}\n";
                
                foreach (var questionEntry in posteEntry.Value.verites)
                {
                    string variationsStr = string.Join(", ", questionEntry.Value);
                    texte += $"    Q{questionEntry.Key}: [{variationsStr}]\n";
                }
                
                texte += "\n";
            }
        }
        
        texte += "[Espace] Retour\n";
        texte += "\n[Échap] Quitter\n";
        
        texteDialogue.text = texte;
    }
    
    void GererAffichageVerites()
    {
        if (Input.GetKeyDown(KeyCode.Space))
        {
            AfficherChoixService();
        }
    }
    
    // ========== FIN DE NIVEAU ==========
    
    void AfficherFinNiveau()
    {
        etatActuel = EtatJeu.FinNiveau;
        
        string texte = $"=== FIN DU NIVEAU {niveauActuel} ===\n\n";
        texte += "Félicitations ! Vous avez terminé ce niveau.\n\n";
        
        if (niveauActuel < 5)
        {
            texte += $"Passer au niveau {niveauActuel + 1} ?\n\n";
            texte += "[O] Oui, niveau suivant\n";
            texte += "[N] Non, retour au menu\n";
        }
        else
        {
            texte += "🎉 Vous avez terminé tous les niveaux ! 🎉\n\n";
            texte += "[Espace] Retour au menu\n";
        }
        
        texte += "\n[Échap] Quitter\n";
        
        texteDialogue.text = texte;
    }
    
    void GererFinNiveau()
    {
        if (niveauActuel < 5)
        {
            if (Input.GetKeyDown(KeyCode.O))
            {
                niveauActuel++;
                Debug.Log($"Passage au niveau {niveauActuel}");
                ChargerScenario(scenarioSelectionne);
                DetecterServicesDisponibles();
                AfficherChoixService();
                return;
            }
            
            if (Input.GetKeyDown(KeyCode.N))
            {
                AfficherChoixScenario();
                return;
            }
        }
        else
        {
            if (Input.GetKeyDown(KeyCode.Space))
            {
                AfficherChoixScenario();
                return;
            }
        }
    }
    
    // ========== CHOIX DU POSTE ==========
    
    void AfficherChoixPoste()
    {
        etatActuel = EtatJeu.ChoixPoste;
        
        if (!postes.ContainsKey(serviceSelectionne))
        {
            texteDialogue.text = $"ERREUR : Service '{serviceSelectionne}' non configuré\n\n[0] Retour";
            return;
        }
        
        List<PosteInfo> postesDisponibles = postes[serviceSelectionne];
        
        string texte = $"=== {serviceSelectionne.ToUpper()} - CHOISISSEZ UN POSTE ===\n\n";
        
        for (int i = 0; i < postesDisponibles.Count; i++)
        {
            texte += $"[{i + 1}] {postesDisponibles[i].nomPoste}\n";
        }
        
        texte += "\n[0] Retour\n";
        texte += "\n[Échap] Quitter\n";
        
        texteDialogue.text = texte;
    }
    
    void GererChoixPoste()
    {
        if (Input.GetKeyDown(KeyCode.Alpha0))
        {
            AfficherChoixService();
            return;
        }
        
        List<PosteInfo> postesDisponibles = postes[serviceSelectionne];
        
        for (int i = 0; i < postesDisponibles.Count; i++)
        {
            if (Input.GetKeyDown(KeyCode.Alpha1 + i))
            {
                posteSelectionne = postesDisponibles[i].nomPoste;
                fichierPersonnageSelectionne = postesDisponibles[i].fichierJson;
                Debug.Log($"Poste sélectionné : {posteSelectionne} ({fichierPersonnageSelectionne})");
                AfficherChoixQuestion();
                return;
            }
        }
    }
    
    // ========== CHOIX DE LA QUESTION ==========
    
    void AfficherChoixQuestion()
    {
        etatActuel = EtatJeu.ChoixQuestion;

        PlayerData perso = dialogueManager.ObtenirInfosPersonnage(fichierPersonnageSelectionne);

        string texte = $"=== {perso.prenom} {perso.nom} ===\n";
        texte += $"{perso.metier} - Service {perso.service}\n\n";
        
        // Déterminer les questions disponibles selon le niveau
        questionsDisponiblesPourPersonnage = ObtenirQuestionsDisponibles(perso);
        
        if (questionsDisponiblesPourPersonnage.Count == 0)
        {
            texte += "Aucune question disponible pour ce niveau.\n\n";
            texte += "[R] Retour\n";
        }
        else if (questionsDisponiblesPourPersonnage.Count == 1)
        {
            // Niveau 1 et 2 : 1 seule question, pas de choix
            texte += "Question :\n\n";
            texte += $"{questionsDisponiblesPourPersonnage[0]}\n\n";
            texte += "[Espace] Poser la question\n";
            texte += "\n[R] Retour\n";
        }
        else
        {
            // Niveaux 3+ : Plusieurs questions disponibles
            texte += "Questions disponibles :\n\n";
            for (int i = 0; i < questionsDisponiblesPourPersonnage.Count; i++)
            {
                texte += $"[{i}] {questionsDisponiblesPourPersonnage[i]}\n";
            }
            texte += "\n[R] Retour\n";
        }

        texte += "\n[Échap] Quitter\n";
        texteDialogue.text = texte;
    }
    
    List<string> ObtenirQuestionsDisponibles(PlayerData perso)
    {
        List<string> questions = new List<string>();
        
        // Charger le fichier de vérités pour savoir quelles questions sont disponibles
        string filePath = Path.Combine(Application.persistentDataPath, "GameData", "scenario_verites.json");
        
        if (!File.Exists(filePath))
        {
            Debug.LogError("Fichier vérités introuvable");
            return questions;
        }
        
        string jsonContent = File.ReadAllText(filePath);
        VeritesScenarioRoot verites = JsonConvert.DeserializeObject<VeritesScenarioRoot>(jsonContent);
        
        Debug.Log($"=== DEBUG ObtenirQuestionsDisponibles ===");
        Debug.Log($"Perso: {perso.prenom} {perso.nom}, Service: {perso.service}, Métier: {perso.metier}");
        Debug.Log($"Services dans vérités: {string.Join(", ", verites.verites.Keys)}");
        
        // Récupérer la liste de questions depuis le scénario
        List<string> listeQuestionsScenario = null;
        bool estServiceAudite = perso.service.Trim().ToLower() == scenarioData.service_audite.Trim().ToLower();
        
        if (estServiceAudite && scenarioData.questions.service_technicien != null)
        {
            listeQuestionsScenario = scenarioData.questions.service_technicien.liste;
            Debug.Log($"Service audité - {listeQuestionsScenario.Count} questions dans le scénario");
        }
        else if (!estServiceAudite && scenarioData.questions.autres_services != null)
        {
            listeQuestionsScenario = scenarioData.questions.autres_services.liste;
            Debug.Log($"Autre service - {listeQuestionsScenario.Count} questions dans le scénario");
        }
        
        if (listeQuestionsScenario == null)
        {
            Debug.LogError("Liste questions scénario est null!");
            return questions;
        }
        
        // Récupérer les IDs des questions disponibles depuis les vérités
        if (verites.verites.ContainsKey(perso.service))
        {
            Debug.Log($"Service {perso.service} trouvé dans les vérités");
            var serviceVerites = verites.verites[perso.service];
            
            Debug.Log($"Postes disponibles: {string.Join(", ", serviceVerites.postes.Keys)}");
            
            if (serviceVerites.postes.ContainsKey(perso.metier))
            {
                Debug.Log($"Métier {perso.metier} trouvé!");
                var posteVerites = serviceVerites.postes[perso.metier];
                
                Debug.Log($"Nombre de questions dans vérités: {posteVerites.verites.Count}");
                
                // Les clés du dictionnaire verites sont les IDs des questions disponibles
                foreach (string questionId in posteVerites.verites.Keys)
                {
                    Debug.Log($"Question ID trouvé: {questionId}");
                    int index = int.Parse(questionId);
                    if (index < listeQuestionsScenario.Count)
                    {
                        questions.Add(listeQuestionsScenario[index]);
                        Debug.Log($"  -> Ajouté: {listeQuestionsScenario[index]}");
                    }
                }
            }
            else
            {
                Debug.LogError($"Métier {perso.metier} NON trouvé dans les vérités!");
            }
        }
        else
        {
            Debug.LogError($"Service {perso.service} NON trouvé dans les vérités!");
        }
        
        Debug.Log($"Total questions trouvées: {questions.Count}");
        return questions;
    }

    void GererChoixQuestion()
    {
        if (Input.GetKeyDown(KeyCode.R))
        {
            AfficherChoixPoste();
            return;
        }
        
        // Si une seule question : Espace pour poser
        if (questionsDisponiblesPourPersonnage.Count == 1)
        {
            if (Input.GetKeyDown(KeyCode.Space))
            {
                // Trouver l'index de cette question dans la liste complète
                string numeroQuestion = TrouverNumeroQuestion(questionsDisponiblesPourPersonnage[0]);
                AfficherReponse(numeroQuestion);
                return;
            }
        }
        else
        {
            // Plusieurs questions : touches 0-9
            for (int i = 0; i < Mathf.Min(questionsDisponiblesPourPersonnage.Count, 10); i++)
            {
                if (Input.GetKeyDown(KeyCode.Alpha0 + i))
                {
                    string numeroQuestion = TrouverNumeroQuestion(questionsDisponiblesPourPersonnage[i]);
                    AfficherReponse(numeroQuestion);
                    return;
                }
            }
        }
    }
    
    string TrouverNumeroQuestion(string texteQuestion)
    {
        PlayerData perso = dialogueManager.ObtenirInfosPersonnage(fichierPersonnageSelectionne);
        bool estServiceAudite = perso.service.Trim().ToLower() == scenarioData.service_audite.Trim().ToLower();
        
        List<string> listeComplete = null;
        
        if (estServiceAudite && scenarioData.questions.service_technicien != null)
        {
            listeComplete = scenarioData.questions.service_technicien.liste;
        }
        else if (!estServiceAudite && scenarioData.questions.autres_services != null)
        {
            listeComplete = scenarioData.questions.autres_services.liste;
        }
        
        if (listeComplete != null)
        {
            int index = listeComplete.IndexOf(texteQuestion);
            if (index >= 0)
            {
                return index.ToString();
            }
        }
        
        return "0";
    }
    
    // ========== AFFICHAGE DE LA RÉPONSE ==========
    
    void AfficherReponse(string numeroQuestion)
    {
        etatActuel = EtatJeu.AffichageReponse;
        
        Debug.Log($"Récupération du dialogue : Scénario {scenarioSelectionne}, Personnage {fichierPersonnageSelectionne}, Question {numeroQuestion}");
        
        string reponse = dialogueManager.ObtenirDialogue(
            scenarioSelectionne,
            fichierPersonnageSelectionne,
            numeroQuestion
        );
        
        PlayerData perso = dialogueManager.ObtenirInfosPersonnage(fichierPersonnageSelectionne);
        
        string texte = $"=== {perso.prenom} {perso.nom} ===\n\n";
        texte += $"\"{reponse}\"\n\n";
        texte += "[Espace] Continuer\n";
        texte += "\n[Échap] Quitter\n";
        
        texteDialogue.text = texte;
    }
    
    void GererAffichageReponse()
    {
        if (!EventSystem.current || !EventSystem.current.currentSelectedGameObject)
        {
            if (Input.GetKeyDown(KeyCode.Space))
                AfficherChoixQuestion();
        }
    }
}