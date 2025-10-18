using Caractere;
using Metier;
using Service;

public class Personnage
{
    private string name;
    private Metier job;
    private Service service;
    private Caractere caractere;

    public Personnage(string nom, Metier job, Service service, Caractere caractere)
    {
        this.name = nom;
        this.job = job;
        this.service = service;
        this.caractere = caractere;
    }


    public string getName()
    {
        return this.name;
    }

    public Metier getJob()
    {
        return this.job;
    }

    public Service getService()
    {
        return this.service;
    }

    public Caractere getCaractere()
    {
        return this.getCaractere;
    }

    public string repondre(Dialogue d)
    {
        return d.donnerReponse(this);
    }
}