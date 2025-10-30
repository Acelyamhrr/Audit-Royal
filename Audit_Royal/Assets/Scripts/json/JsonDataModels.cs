using System;
using System.Collections.Generic;
using UnityEngine; // Nécessaire pour [Serializable]

// ----------------------------------------------------------------------
// MODÈLES POUR LA LECTURE DES FICHIERS SOURCE (e.g., scenario1_compta.json)
// ----------------------------------------------------------------------

[Serializable]
public class DialogueVariation
{
    // Correspond à l'objet interne (un dialogue)
    public string normal;
    public string colere;
    public string anxieux;
    public string menteur;
    public string balance;
    public string info_cle;
    public int variation_id;
}

[Serializable]
public class ServiceData
{
    public string service;
    
    // CORRECTION : Dictionnaire des postes (clé: nom du poste) contenant 
    // le dictionnaire des questions (clé: "0", "1", etc.) contenant la List<DialogueVariation>.
    public Dictionary<string, Dictionary<string, List<DialogueVariation>>> postes; 
}

// ----------------------------------------------------------------------
// MODÈLES POUR L'ÉCRITURE DU FICHIER CIBLE (scenario_verites.json)
// ----------------------------------------------------------------------

[Serializable]
public class VeritesByPoste
{
    // Clé: numéro de la question ("0", "1", "2")
    // Valeur: Liste des 'variation_id' qui sont VRAIES pour cette question/poste
    public Dictionary<string, List<int>> verites; 
}

[Serializable]
public class VeritesByService
{
    // Clé: poste ("secrétaire", "patron")
    public Dictionary<string, VeritesByPoste> postes; 
}

[Serializable]
public class VeritesScenarioRoot
{
    public int scenario; // Le numéro du scénario (e.g., 1)
    // Clé: service ("communication", "compta")
    public Dictionary<string, VeritesByService> verites; 
}