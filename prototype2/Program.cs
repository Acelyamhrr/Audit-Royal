using System;

class Program
{
    static void Main()
    {
        var carnet = new CarnetManager("carnet.json", "scenario_verites.json");
        carnet.ajoutInfo(Service.COMMUNICATION, Metier.RESPONSABLE_RESEAUX_SOCIAUX, "1", 2);
        carnet.ajoutInfo(Service.TECHNICIEN, Metier.CONCIERGE, "1", 1);
        carnet.ajoutInfo(Service.TECHNICIEN, Metier.CONCIERGE, "0", 1);
        Console.WriteLine(carnet.afficherCarnet());
    }
}
