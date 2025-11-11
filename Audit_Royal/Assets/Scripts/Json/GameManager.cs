using UnityEngine;
using TMPro;
using System.Collections.Generic;
using System.IO;
using Newtonsoft.Json;
using UnityEngine.EventSystems;

public class GameManager : MonoBehaviour
{

    private ScenarioRoot scenarioData; 

    // le ui du truc où les dialogues vont être affichés.
    public TextMeshProUGUI texteDialogue;  
    
    public List<int> scenariosDisponibles = new List<int> { 1, 2 }; // !! ajouter quand on a un autre scenario
    
    private JsonDialogueManager dialogueManager;
    private PersonnageManager personnageManager;
    
    private enum EtatJeu { ChoixScenario, ChoixService, ChoixPoste, ChoixQuestion, AffichageReponse }
    private EtatJeu etatActuel;
    
    private int scenarioSelectionne;
    private string serviceSelectionne;
    private string posteSelectionne;
    private string fichierPersonnageSelectionne;
    
    // Liste des services et postes dispo pour chaque scénario
    private List<string> servicesDisponibles = new List<string>();
    
    private Dictionary<string, List<PosteInfo>> postes = new Dictionary<string, List<PosteInfo>>();
    
    
    void Start()
    {
        // recup ou créer les managers
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
        
        // Initialiser les postes
        InitialiserPostes();
        
        // Commencer par le choix du scénario
        AfficherChoixScenario();
    }
    
    void InitialiserPostes()
    {
        // COMMUNICATION
        postes["communication"] = new List<PosteInfo>
        {
            new PosteInfo("Graphiste", "com_graphiste.json"),
            new PosteInfo("Responsable Réseaux Sociaux", "com_responsable_reseaux_sociaux.json"),
            new PosteInfo("Vidéaste", "com_video.json")
        };
        
        // COMPTABILITÉ
        postes["compta"] = new List<PosteInfo>
        {
            new PosteInfo("Comptable", "compta_comptable.json"),
            new PosteInfo("Patron", "compta_patron.json"),
            new PosteInfo("Secrétaire", "compta_secretaire.json")
        };
        
        // TECHNICIENS
        postes["techniciens"] = new List<PosteInfo>
        {
            new PosteInfo("Concierge", "gc_concierge.json"),
            new PosteInfo("Patron", "gc_patron.json"),
            new PosteInfo("Paysagiste", "gc_paysagiste.json"),
            new PosteInfo("Secrétaire", "gc_secretaire.json")
        };
        
        // INFORMATIQUE
        postes["info"] = new List<PosteInfo>
        {
            new PosteInfo("Patron", "info_patron.json"),
            new PosteInfo("Responsable Réseau", "info_responsable_reseau.json"),
            new PosteInfo("Secrétaire", "info_secretaire.json"),
            new PosteInfo("Technicien de Maintenance", "info_technicien_de_maintenance.json")
        };
        
        // RESTAURATION
        postes["restauration"] = new List<PosteInfo>
        {
            new PosteInfo("Cuisinier", "res_cuisinier.json"),
            new PosteInfo("Patron", "res_patron.json")
        };
    }
    
    void Update()
    {
        // Échap pour quitter (à enlever après implémentation graphique)
        if (Input.GetKeyDown(KeyCode.Escape) && etatActuel != EtatJeu.ChoixScenario)
        {
            Application.Quit();
            #if UNITY_EDITOR
            UnityEditor.EditorApplication.isPlaying = false;
            #endif
            return;
        }
        
        // touches selon l'état
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
        }
    }
    
    // CHOIX DU SCÉNARIO
    
    void AfficherChoixScenario()
    {
        etatActuel = EtatJeu.ChoixScenario;
        
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
    }
    
    // DÉTECTION DES SERVICES DISPONIBLES
    
    void DetecterServicesDisponibles()
    {
        servicesDisponibles.Clear();
        
        // Liste des services possibles
        string[] servicesAPotentiels = { "communication", "compta", "info", "restauration", "techniciens" };
        
        foreach (string service in servicesAPotentiels)
        {
            // Vérifier si le fichier existe
            string nomFichier = $"scenario{scenarioSelectionne}_{service}.json";
            string cheminComplet = Path.Combine(Application.streamingAssetsPath, nomFichier);
            
            if (File.Exists(cheminComplet))
            {
                servicesDisponibles.Add(service);
                Debug.Log($"Service trouvé : {service}");
            }
            else
            {
                Debug.LogWarning($"Service manquant : {nomFichier}");
            }
        }
        
        if (servicesDisponibles.Count == 0)
        {
            Debug.LogError($"AUCUN service trouvé pour le scénario {scenarioSelectionne} !");
            texteDialogue.text = $"ERREUR : Aucun fichier de dialogue trouvé pour le scénario {scenarioSelectionne}\n\n[Échap] Quitter";
        }
    }
    
    // CHOIX DU SERVICE
    
    void AfficherChoixService()
    {
        etatActuel = EtatJeu.ChoixService;
        
        string texte = $"=== SCÉNARIO {scenarioSelectionne} - CHOISISSEZ UN SERVICE ===\n\n";
        
        for (int i = 0; i < servicesDisponibles.Count; i++)
        {
            string serviceNom = servicesDisponibles[i].ToUpper();
            texte += $"[{i + 1}] {serviceNom}\n";
        }
        
        texte += "\n[0] Retour\n";
        texte += "\n[Échap] Quitter\n";
        
        texteDialogue.text = texte;
    }
    
    void GererChoixService()
    {
        // Retour
        if (Input.GetKeyDown(KeyCode.Alpha0))
        {
            AfficherChoixScenario();
            return;
        }
        
        // Sélection d'un service
        for (int i = 0; i < servicesDisponibles.Count; i++)
        {
            if (Input.GetKeyDown(KeyCode.Alpha1 + i))
            {
                serviceSelectionne = servicesDisponibles[i];
                Debug.Log($"Service sélectionné : {serviceSelectionne}");
                AfficherChoixPoste();
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
        // Retour
        if (Input.GetKeyDown(KeyCode.Alpha0))
        {
            AfficherChoixService();
            return;
        }
        
        List<PosteInfo> postesDisponibles = postes[serviceSelectionne];
        
        // Sélection d'un poste
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

        // Récupérer les infos du personnage
        PlayerData perso = dialogueManager.ObtenirInfosPersonnage(fichierPersonnageSelectionne);

        string texte = $"=== {perso.prenom} {perso.nom} ===\n";
        texte += $"{perso.metier} - Service {perso.service}\n\n";
        texte += "Questions disponibles :\n\n";

        // Déterminer quelle liste de questions utiliser :
        // Si le service du personnage correspond au service audité, on prend les questions spécifiques.
        string cleQuestions;

        if (perso.service.Trim().ToLower() == scenarioData.service_audite.Trim().ToLower())
        {
            cleQuestions = "service_technicien"; // la clé JSON reste "service_technicien"
        }
        else
        {
            cleQuestions = "autres_services";
        }

        // Sélection de la bonne liste (selon la structure de ton JSON)
        List<string> listeQuestions = null;

        if (cleQuestions == "service_technicien" && scenarioData.questions.service_technicien != null)
        {
            listeQuestions = scenarioData.questions.service_technicien.liste;
        }
        else if (cleQuestions == "autres_services" && scenarioData.questions.autres_services != null)
        {
            listeQuestions = scenarioData.questions.autres_services.liste;
        }

        if (listeQuestions != null)
        {
            for (int i = 0; i < listeQuestions.Count; i++)
            {
                texte += $"[{i}] {listeQuestions[i]}\n";
            }
        }
        else
        {
            Debug.LogError($"Questions introuvables pour la clé '{cleQuestions}' !");
            texte += "Erreur : Questions introuvables\n";
        }

        texte += "\n[R] Retour\n";
        texte += "\n[Échap] Quitter\n";

        texteDialogue.text = texte;
    }


    
    void GererChoixQuestion()
    {
        Debug.Log("GererChoixQuestion appelé");
    
        // Retour
        if (Input.GetKeyDown(KeyCode.R))
        {
            AfficherChoixPoste();
            return;
        }
        
        // Questions 0-9
        for (int i = 0; i <= 9; i++)
        {
            if (Input.GetKeyDown(KeyCode.Alpha0 + i))
            {
                Debug.Log($"Touche {i} détectée !");
                AfficherReponse(i.ToString());
                return;
            }
        }
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
        
        // Récupérer le nom du personnage
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
