namespace prototype.Models
{
    public class Personnage
    {
        public string Nom { get; }
        public Metier Job { get; }
        public Service Service { get; }
        public Caractere Caractere { get; }

        public Personnage(string nom, Metier job, Service service, Caractere caractere)
        {
            Nom = nom;
            Job = job;
            Service = service;
            Caractere = caractere;
        }
    }
}
