using System;
using prototype.Models;

namespace prototype.Views
{
    public static class ConsoleView
    {
        public static void AfficherReponse(Personnage perso, string reponse)
        {
            Console.WriteLine($"[{perso.Nom}] {reponse}");
        }
    }
}
