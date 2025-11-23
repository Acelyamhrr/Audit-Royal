using System;
using System.Collections.Generic;
using UnityEngine; 

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
    public Dictionary<string, Dictionary<string, List<DialogueVariation>>> postes; 
}


[Serializable]
public class VeritesByPoste
{
    // clé / valeur
    public Dictionary<string, List<int>> verites; 
}

[Serializable]
public class VeritesByService
{
    // ("secrétaire", "patron")
    public Dictionary<string, VeritesByPoste> postes; 
}

[Serializable]
public class VeritesScenarioRoot
{
    public int scenario; // a implémenter plus tard
    public Dictionary<string, VeritesByService> verites; 
}