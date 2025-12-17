using UnityEngine;
using System.IO;
using System.Collections.Generic;


[System.Serializable]
public class DataPlayer
{
    public string nom;
    public string prenom;
    public string service;
    public string caractere;
    public string metier;
    public double taux;

}
public class PersonnageManager : MonoBehaviour
{
    private string sourcePath;
    private string savePath;
    public DataPlayer data;
    private string[] caractere = {"colere", "anxieux", "menteur", "balance", "insouciant"};
    private int[] nbCaractere = {0,0,0,0,0};
    private List<int> caractereBanned = new List<int>();

    private string[] peroJson = { 
        "compta_comptable.json", "compta_patron.json", "compta_secretaire.json", 
        "com_graphiste.json", "com_responsable_reseaux_sociaux.json", "com_video.json", 
        "gc_concierge.json", "gc_patron.json", "gc_paysagiste.json", "gc_secretaire.json", 
        "info_patron.json", "info_responsable_reseau.json", "info_secretaire.json", "info_technicien_de_maintenance.json", 
        "res_cuisinier.json", "res_patron.json" 
    };

    void Start()
    {
        for (int i = 0; i < 16; i++)
        {

            sourcePath = Path.Combine(Application.streamingAssetsPath, peroJson[i]);
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
