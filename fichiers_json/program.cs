// program.cs
// Simulation interactive d'une partie d'audit
// Compatible Mono / mcs
//
// Compile :
//   mcs program.cs -r:System.Web.Extensions
// Exécute :
//   mono program.exe
//
// Prérequis : scenario2_partie.json + dialogues_*.json générés par ScenarioManager

using System;
using System.IO;
using System.Text;
using System.Web.Script.Serialization;
using System.Collections;
using System.Collections.Generic;

class Program
{
    static void Main()
    {
        Console.OutputEncoding = Encoding.UTF8;

        const string SCENARIO_FILE = "scenario2_partie.json";
        if (!File.Exists(SCENARIO_FILE))
        {
            Console.WriteLine("❌ Fichier scénario non trouvé : " + SCENARIO_FILE);
            Console.WriteLine("➡️  Lance d'abord ScenarioManager pour le générer !");
            return;
        }

        var serializer = new JavaScriptSerializer();
        serializer.MaxJsonLength = Int32.MaxValue;

        // Charger le scénario global
        var scenarioData = serializer.Deserialize<Dictionary<string, object>>(File.ReadAllText(SCENARIO_FILE, Encoding.UTF8));

        // Charger la clé "verites" (contient les faits vrais de la partie)
        if (!scenarioData.ContainsKey("verites"))
        {
            Console.WriteLine("❌ Ce scénario ne contient pas de clé 'verites'.");
            return;
        }
        var verites = (Dictionary<string, object>)scenarioData["verites"];

        // Boucle principale : permet d'enchaîner plusieurs services/personnages
        bool continuer = true;
        while (continuer)
        {
            Console.Clear();
            Console.WriteLine("=== MENU PRINCIPAL ===");
            Console.WriteLine("1. Interroger un service");
            Console.WriteLine("2. Quitter");
            Console.Write("👉 Ton choix : ");
            int choixMenu = SafeIntInput(1, 2);
            if (choixMenu == 2) break;

            // === Choix du service ===
            var services = new List<string>(verites.Keys);
            Console.Clear();
            Console.WriteLine("=== Choisis un service ===");
            for (int i = 0; i < services.Count; i++)
                Console.WriteLine($"{i + 1}. {services[i]}");
            Console.Write("👉 Ton choix : ");
            int idxService = SafeIntInput(1, services.Count) - 1;
            string service = services[idxService];

            // Charger le fichier de dialogues correspondant
            string fichierDialogue = $"dialogues_{service}.json";
            if (!File.Exists(fichierDialogue))
            {
                Console.WriteLine($"❌ Fichier de dialogues introuvable pour le service {service}.");
                Console.ReadLine();
                continue;
            }
            var dialogueData = serializer.Deserialize<Dictionary<string, object>>(File.ReadAllText(fichierDialogue, Encoding.UTF8));

            // Extraire les postes (personnages)
            Dictionary<string, object> postes = null;
            if (dialogueData.ContainsKey("postes"))
                postes = (Dictionary<string, object>)dialogueData["postes"];
            else if (dialogueData.ContainsKey("dialogues"))
                postes = (Dictionary<string, object>)dialogueData["dialogues"];
            else
            {
                Console.WriteLine("❌ Format de dialogue invalide.");
                Console.ReadLine();
                continue;
            }

            // Choix du poste
            var nomsPostes = new List<string>(postes.Keys);
            Console.Clear();
            Console.WriteLine($"=== Service : {service.ToUpper()} ===");
            for (int i = 0; i < nomsPostes.Count; i++)
                Console.WriteLine($"{i + 1}. {nomsPostes[i]}");
            Console.Write("👉 Choisis un poste : ");
            int idxPoste = SafeIntInput(1, nomsPostes.Count) - 1;
            string poste = nomsPostes[idxPoste];

            // Récupérer les vérités correspondantes
            var serviceVerites = verites.ContainsKey(service)
                ? (Dictionary<string, object>)verites[service]
                : new Dictionary<string, object>();
            var veritesPoste = serviceVerites.ContainsKey(poste)
                ? (ArrayList)serviceVerites[poste]
                : new ArrayList();

            // Lancer la simulation pour ce poste
            SimulerDialogue(service, poste, postes[poste], veritesPoste);

            // Retour au menu ?
            Console.WriteLine("\nAppuie sur Entrée pour revenir au menu principal...");
            Console.ReadLine();
        }

        Console.WriteLine("\n👋 Fin de la session. Merci d'avoir joué !");
    }

    static void SimulerDialogue(string service, string poste, object contenuPoste, ArrayList veritesPoste)
    {
        var dialogues = contenuPoste as Dictionary<string, object>;
        if (dialogues == null)
        {
            Console.WriteLine("⚠️ Format inattendu pour les dialogues de ce poste.");
            return;
        }

        var random = new Random();
        string[] tons = { "normal", "colere", "anxieux", "menteur", "balance" };

        // On affiche les dialogues selon les étapes
        foreach (var etape in dialogues)
        {
            string etapeId = etape.Key;
            ArrayList variations = ToArrayList(etape.Value);
            if (variations == null || variations.Count == 0) continue;

            // Choisit une variation au hasard
            var variation = (Dictionary<string, object>)variations[random.Next(variations.Count)];

            // Tire une tonalité (invisible pour le joueur)
            string ton = tons[random.Next(tons.Length)];
            string reponse = variation.ContainsKey(ton) ? variation[ton].ToString() : "(pas de réponse)";
            string info = variation.ContainsKey("info_cle") ? variation["info_cle"].ToString() : "(aucune info)";

            // Affiche la question/réponse
            Console.WriteLine($"🟦 Étape {etapeId}");
            Console.WriteLine($"🗣️  {poste} : {reponse}");
            Console.WriteLine();

            // Le joueur ne sait pas si c'est vrai ou faux — mais le moteur oui.
            /*bool estVrai = false;
            foreach (Dictionary<string, object> v in veritesPoste)
            {
                if (v["etape"].ToString() == etapeId && v["info_cle"].ToString() == info)
                {
                    estVrai = true;
                    break;
                }
            }*/

            // (facultatif) — tu peux décommenter pour le mode debug
            // Console.WriteLine($"DEBUG → Vérité : {estVrai}");

            Console.Write("➡️  Appuie sur Entrée pour continuer...");
            Console.ReadLine();
            Console.WriteLine();
        }

        Console.WriteLine($"=== Fin de l'interrogatoire : {poste} ({service}) ===");
    }

    // Lecture sécurisée d'un entier
    static int SafeIntInput(int min, int max)
    {
        while (true)
        {
            string s = Console.ReadLine();
            int val;
            if (int.TryParse(s, out val) && val >= min && val <= max)
                return val;
            Console.Write($"❌ Choix invalide, entre {min} et {max} : ");
        }
    }

    // Convertit un objet en ArrayList
    static ArrayList ToArrayList(object value)
    {
        if (value == null) return null;
        if (value is ArrayList) return (ArrayList)value;
        if (value is object[]) return new ArrayList((object[])value);
        if (value is IList)
        {
            var list = (IList)value;
            var arr = new ArrayList();
            foreach (var item in list) arr.Add(item);
            return arr;
        }
        return null;
    }
}
