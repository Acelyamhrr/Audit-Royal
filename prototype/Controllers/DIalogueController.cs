using System;
using System.IO;
using System.Text.Json;
using prototype.Models;

namespace prototype.Controllers
{
    public class DialogueController
    {
        private DialogueData _dialogueData;     // continent tt les données de dialogues chargées depuis le json
        private Random _rnd = new Random();     // génère un nb alétoire (pour choisir une phrase au hasard)

        public DialogueController(string chemin)
        {
            string json = File.ReadAllText(chemin);     // lit tt le contenu du fichier JSON sous forme de texte
            _dialogueData = JsonSerializer.Deserialize<DialogueData>(json) ?? new DialogueData();   
            // convertit le texte json en objet DialogueData (dictionnaire)
            // si ça échoue, ca créer un objet vide pour pas faire d'erreur
        }

        public string DonnerReponse(Personnage perso)
        {
            string key = $"{perso.Job}_{perso.Caractere}";

            if (_dialogueData.Dialogues.ContainsKey(key))
            {
                var options = _dialogueData.Dialogues[key];
                int index = _rnd.Next(options.Length);
                return options[index];
            }

            return "Je n'ai rien à dire pour le moment.";
        }
    }
}
