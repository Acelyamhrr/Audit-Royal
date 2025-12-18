using System;
using System.Collections.Generic;
using UnityEngine; 

/// <summary>
/// Représente une variation de dialogue selon l’émotion ou le comportement du personnage.
/// </summary>
[Serializable]
public class DialogueVariation
{
    /// <summary>
    /// Dialogue dans un état normal.
    /// </summary>
    public string normal;
    
    /// <summary>
    /// Dialogue exprimant la colère.
    /// </summary>
    public string colere;
    
    /// <summary>
    /// Dialogue exprimant l’anxiété.
    /// </summary>
    public string anxieux;
    
    /// <summary>
    /// Dialogue exprimant le mensonge.
    /// </summary>
    public string menteur;
    
    /// <summary>
    /// Dialogue où le personnage dit la vérité.
    /// </summary>
    public string balance;
    
    /// <summary>
    /// Information clé du dialogue.
    /// </summary>
    public string info_cle;
    
    /// <summary>
    /// Identifiant de la variation.
    /// </summary>
    public int variation_id;
}

/// <summary>
/// Contient les données d’un service avec ses postes et dialogues associés.
/// </summary>
[Serializable]
public class ServiceData
{
    /// <summary>
    /// Nom du service.
    /// </summary>
    public string service;
    
    /// <summary>
    /// Dictionnaire des postes et de leurs dialogues.
    /// Clé 1 : poste, Clé 2 : identifiant, Valeur : liste de variations.
    /// </summary>
    public Dictionary<string, Dictionary<string, List<DialogueVariation>>> postes; 
}

/// <summary>
/// Contient les vérités associées à un poste.
/// </summary>
[Serializable]
public class VeritesByPoste
{
    /// <summary>
    /// Dictionnaire des vérités.
    /// Clé : numéro de question, Valeur : liste d’identifiants d’informations vraies.
    /// </summary>
    public Dictionary<string, List<int>> verites; 
}

/// <summary>
/// Regroupe les vérités par service.
/// </summary>
[Serializable]
public class VeritesByService
{
    /// <summary>
    /// Dictionnaire des postes du service.
    /// </summary>
    public Dictionary<string, VeritesByPoste> postes; 
}

/// <summary>
/// Données de vérités pour un scénario.
/// </summary>
[Serializable]
public class VeritesScenarioRoot
{
    /// <summary>
    /// Identifiant du scénario.
    /// </summary>
    public int scenario;
    
    /// <summary>
    /// Niveau associé au scénario.
    /// </summary>
    public int niveau;
    
    /// <summary>
    /// Dictionnaire des vérités par service.
    /// </summary>
    public Dictionary<string, VeritesByService> verites; 
}

/// <summary>
/// Représente les données d'un personnage.
/// </summary>
[Serializable]
public class PlayerData
{
    /// <summary>
    /// Nom du personnage.
    /// </summary>
    public string nom;
    
    /// <summary>
    /// Prénom du personnage.
    /// </summary>
    public string prenom;
    
    /// <summary>
    /// Service du personnage.
    /// </summary>
    public string service;
    
    /// <summary>
    /// Métier du personnage.
    /// </summary>
    public string metier;
    
    /// <summary>
    /// Caractère du personnage.
    /// </summary>
    public string caractere;
    
    /// <summary>
    /// Taux de caractère du personnage.
    /// </summary>
    public double taux;
}

/// <summary>
/// Bloc de questions utilisé dans un scénario.
/// </summary>
[Serializable]
public class QuestionBloc
{
    /// <summary>
    /// Description du bloc de questions.
    /// </summary>
    public string description;
    
    /// <summary>
    /// Liste des questions associées.
    /// </summary>
    public List<string> liste;
}

/// <summary>
/// Contient l’ensemble des questions d’un scénario.
/// </summary>
[Serializable]
public class ScenarioQuestions
{
    /// <summary>
    /// Questions liées au service audité.
    /// </summary>
    public QuestionBloc service_audite;
    
    /// <summary>
    /// Questions liées aux autres services.
    /// </summary>
    public QuestionBloc autres_services;
}

/// <summary>
/// Données d’un scénario.
/// </summary>
[Serializable]
public class ScenarioRoot
{
    /// <summary>
    /// Identifiant du scénario.
    /// </summary>
    public int scenario;
    
    /// <summary>
    /// Titre du scénario.
    /// </summary>
    public string titre;
    
    /// <summary>
    /// Titre du scénario.
    /// </summary>
    public string theme;
    
    /// <summary>
    /// Service audité dans le scénario.
    /// </summary>
    public string service_audite;
    
    /// <summary>
    /// Problématique principale du scénario.
    /// </summary>
    public string problematique;
    
    /// <summary>
    /// Ensemble des questions du scénario.
    /// </summary>
    public ScenarioQuestions questions;
}

/// <summary>
/// Informations associées à un poste et son fichier de données.
/// </summary>
public class PosteInfo
{
    /// <summary>
    /// Nom du poste.
    /// </summary>
    public string nomPoste;
    
    /// <summary>
    /// Chemin vers le fichier JSON associé.
    /// </summary>
    public string fichierJson;
    
    /// <summary>
    /// Initialise une nouvelle instance de la classe <see cref="PosteInfo"/>.
    /// </summary>
    /// <param name="nom">Nom du poste.</param>
    /// <param name="fichier">Chemin du fichier JSON.</param>
    public PosteInfo(string nom, string fichier)
    {
        nomPoste = nom;
        fichierJson = fichier;
    }
}

/// <summary>
/// Utilisé pour la récupération de l'audit antérieur.
/// </summary>
[System.Serializable]
public class AuditWrapper
{
    /// <summary>
    /// Scenario lié.
    /// </summary>
    public int scenario;
    
    /// <summary>
    /// Contenu de l'audit.
    /// </summary>
    public Audit audit_anterieur;
}

/// <summary>
/// Informations associées à un audit antérieur.
/// </summary>
[System.Serializable]
public class Audit
{
    /// <summary>
    /// Titre de l'audit.
    /// </summary>
    public string titre;
    
    /// <summary>
    /// Etablissement concerné.
    /// </summary>
    public string etablissement;
    
    /// <summary>
    /// Date de réalisation.
    /// </summary>
    public string date;
    
    /// <summary>
    /// Service audité.
    /// </summary>
    public string service_audite;

    
    /// <summary>
    /// Objectifs de l'audit.
    /// </summary>
    public string[] objectifs;
    
    /// <summary>
    /// Contastations de l'audit.
    /// </summary>
    public Constatations constatations;
    
    /// <summary>
    /// Causes du problème.
    /// </summary>
    public string[] analyse_causes;
    
    /// <summary>
    /// Recommandations formulées.
    /// </summary>
    public Recommendation[] recommandations;
    
    /// <summary>
    /// Conclusion de l'audit.
    /// </summary>
    public Conclusion conclusion;
}

/// <summary>
/// Constatations de l'audit.
/// </summary>
[System.Serializable]
public class Constatations
{
    /// <summary>
    /// Points étant conformes.
    /// </summary>
    public string[] points_conformes;
    
    /// <summary>
    /// Points de vigilance.
    /// </summary>
    public string[] points_vigilance;
    
    /// <summary>
    /// Points non conformes.
    /// </summary>
    public string[] non_conformites;
}

/// <summary>
/// Recommandations formulées.
/// </summary>
[System.Serializable]
public class Recommendation
{
    /// <summary>
    /// Id de la recommandation.
    /// </summary>
    public int id;
    
    /// <summary>
    /// Description de la recommandation.
    /// </summary>
    public string description;
    
    /// <summary>
    /// Priorité de la recommandation.
    /// </summary>
    public string priorite;
    
    /// <summary>
    /// A quel point la recommendation a été mise en oeuvre.
    /// </summary>
    public string etat_mise_en_oeuvre;
}

/// <summary>
/// Conclusion de l'audit
/// </summary>
[System.Serializable]
public class Conclusion
{
    /// <summary>
    /// Résumé de l'audit.
    /// </summary>
    public string resume;
    
    /// <summary>
    /// Risques identifiés au cours de l'audit.
    /// </summary>
    public string[] risques_identifies;
    
    /// <summary>
    /// Niveau de risque du problème audité.
    /// </summary>
    public string niveau_risque;
}
