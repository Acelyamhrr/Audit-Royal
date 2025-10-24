using System;
using prototype.Models;
using prototype.Controllers;
using prototype.Views;

class Program
{
    static void Main()
    {
        var dialogueController = new DialogueController("Data/dialogues.json");

        var p1 = new Personnage("Alice", Metier.SECRETAIRE, Service.COMPTABILITE, Caractere.COLERIQUE);
        var rep1 = dialogueController.DonnerReponse(p1);
        ConsoleView.AfficherReponse(p1, rep1);

        var p2 = new Personnage("Bob", Metier.CUISINIER, Service.RESTAURATION, Caractere.INSOUCIANT);
        var rep2 = dialogueController.DonnerReponse(p2);
        ConsoleView.AfficherReponse(p2, rep2);
    }
}
