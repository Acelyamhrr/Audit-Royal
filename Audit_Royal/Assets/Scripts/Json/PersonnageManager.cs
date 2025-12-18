using UnityEngine;
using System.IO;
using System.Collections.Generic;

/// <summary>
/// Structure de données représentant un personnage.
/// </summary>
/// <remarks>
/// Cette classe est utilisée pour la sérialisation et la désérialisation
/// des fichiers JSON contenant les informations des personnages.
/// </remarks>
[System.Serializable]
public class DataPlayer
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
    /// Service auquel appartient le personnage.
    /// </summary>
    public string service;

    /// <summary>
    /// Caractère attribué au personnage.
    /// </summary>
    public string caractere;

    /// <summary>
    /// Métier exercé par le personnage.
    /// </summary>
    public string metier;

    /// <summary>
    /// Taux du caractère du personnage.
    /// </summary>
    public double taux;

}

/// <summary>
/// Gère l’attribution aléatoire des caractères aux personnages
/// et la mise à jour de leurs fichiers JSON.
/// </summary>
/// <remarks>
/// Le <see cref="PersonnageManager"/> copie les fichiers JSON depuis
/// le dossier StreamingAssets vers le dossier persistant si nécessaire,
/// puis attribue un caractère à chaque personnage en respectant des limites.
/// </remarks>
public class PersonnageManager : MonoBehaviour
{
    /// <summary>
    /// Chemin du fichier source dans StreamingAssets.
    /// </summary>
    private string sourcePath;

    /// <summary>
    /// Chemin du fichier de sauvegarde persistant.
    /// </summary>
    private string savePath;

    /// <summary>
    /// Données du personnage actuellement traité.
    /// </summary>
    public DataPlayer data;
    private const string DOSSIER_PERSONNAGES = "personnes_json";
    
    /// <summary>
    /// Liste des caractères possibles.
    /// </summary>
    private string[] caractere = {"colere", "anxieux", "menteur", "balance", "insouciant"};
    
    /// <summary>
    /// Compteur du nombre de personnages par caractère.
    /// </summary>
    private int[] nbCaractere = {0,0,0,0,0};
    
    /// <summary>
    /// Liste des indices de caractères devenus indisponibles.
    /// </summary>
    private List<int> caractereBanned = new List<int>();

    /// <summary>
    /// Liste des fichiers JSON des personnages.
    /// </summary>
    private string[] peroJson = { 
        "compta_comptable.json", "compta_patron.json", "compta_secretaire.json", 
        "com_graphiste.json", "com_responsable_reseaux_sociaux.json", "com_technicien_son_video.json", 
        "gc_concierge.json", "gc_patron.json", "gc_paysagiste.json", "gc_secretaire.json", 
        "info_patron.json", "info_responsable_reseau.json", "info_secretaire.json", "info_technicien_de_maintenance.json", 
        "res_cuisinier.json", "res_patron.json" 
    };

    /// <summary>
    /// Initialise les personnages au lancement de la scène.
    /// </summary>
    /// <remarks>
    /// Pour chaque fichier JSON :
    /// <list type="bullet">
    /// <item><description>Copie le fichier vers le dossier persistant si nécessaire</description></item>
    /// <item><description>Charge les données du personnage</description></item>
    /// <item><description>Attribue un caractère aléatoire avec des contraintes</description></item>
    /// <item><description>Sauvegarde les données mises à jour</description></item>
    /// </list>
    /// </remarks>
    void Start()
    {
        for (int i = 0; i < 16; i++)
        {

            sourcePath = Path.Combine(Application.streamingAssetsPath, DOSSIER_PERSONNAGES, peroJson[i]);
            savePath = Path.Combine(Application.persistentDataPath, peroJson[i]);

            Debug.Log("source path" + sourcePath);
            if (!File.Exists(savePath))
            {
                string creatJson = File.ReadAllText(sourcePath);
                File.WriteAllText(savePath, creatJson);
                Debug.Log("Copie du JSON vers le dossier de sauvegarde : " + savePath);
            }

            string savedJson = File.ReadAllText(savePath);
            data = JsonUtility.FromJson<DataPlayer>(savedJson);

            int idCaractere = RandomNb();
            

            switch (caractere[idCaractere])
            {
                case "balance":
                    if (nbCaractere[idCaractere] < 3)
                    {
                        data.taux = 0.85;
                        data.caractere = caractere[idCaractere];
                        nbCaractere[idCaractere]++;
                    }
                    if(nbCaractere[idCaractere] == 3){
                        caractereBanned.Add(idCaractere);
                    }
                    break;
                case "menteur":
                    if (nbCaractere[idCaractere] < 3)
                    {
                        data.taux = 0.3;
                        data.caractere = caractere[idCaractere];

                        nbCaractere[idCaractere]++;
                    }
                    if(nbCaractere[idCaractere] == 3){
                        caractereBanned.Add(idCaractere);
                    }
                    break;
                case "anxieux":
                    if (nbCaractere[idCaractere] < 4)
                    {
                        data.taux = 0.65;
                        data.caractere = caractere[idCaractere];
                        nbCaractere[idCaractere]++;
                    }
                    if(nbCaractere[idCaractere] == 4){
                        caractereBanned.Add(idCaractere);
                    }
                    break;
                case "colere":
                    if (nbCaractere[idCaractere] < 4)
                    {
                        data.taux = 0.75;
                        data.caractere = caractere[idCaractere];
                        nbCaractere[idCaractere]++;
                    }
                    if(nbCaractere[idCaractere] == 4){
                        caractereBanned.Add(idCaractere);
                    }
                    break;
                case "insouciant":
                    if (nbCaractere[idCaractere] < 2)
                    {
                        data.taux = 0.6;
                        data.caractere = caractere[idCaractere];
                        nbCaractere[idCaractere]++;
                    }
                    if(nbCaractere[idCaractere] == 2){
                        caractereBanned.Add(idCaractere);
                    }
                    break;
            }

            string json = JsonUtility.ToJson(data, true);
            File.WriteAllText(savePath, json);

            Debug.Log($"nom : {data.nom}, prénom : {data.prenom}, caractère : {data.caractere}, taux : {data.taux}");

        }
    }
    
    /// <summary>
    /// Génère un indice de caractère aléatoire valide.
    /// </summary>
    /// <returns>
    /// Indice correspondant à un caractère non interdit.
    /// </returns>
    /// <remarks>
    /// Cette méthode évite les caractères ayant atteint leur limite maximale
    /// d’attribution.
    /// </remarks>
    private int RandomNb()
    {
        int nb;
        while(caractereBanned.Contains(nb = Random.Range(0, 5)))
        {
            continue;
        }
        return nb;

    }
}
