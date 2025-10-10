using UnityEngine;
using UnityEngine.SceneManagement;

public class DialogueAutoStart : MonoBehaviour
{
    public DialogueManager dialogueManager;
    public string speaker = "Chef";
    public string[] lines = new string[]
    {
        "Bonjour. Merci d’être venu.",
        "Nous avons décidé de vous confier un audit interne sur le service informatique.",
        "Ces derniers temps, plusieurs plaintes et rumeurs circulent sur la sécurité des serveurs et la qualité du support technique.",
        "Nous devons savoir ce qu’il en est réellement.",
        "*Le chef marque une pause et regarde le joueur attentivement.*",
        "Votre mission est simple :",
        "– Interrogez les membres du personnel et les utilisateurs,",
        "– Recueillez leurs témoignages,",
        "– Faites la part entre les faits, les exagérations et les rumeurs,",
        "– Et enfin, rédigez un rapport fiable que je pourrai présenter au conseil de direction."
    };
    public float delay = 2f;

    void Start()
    {
        StartCoroutine(StartAfterDelay());
    }

    System.Collections.IEnumerator StartAfterDelay()
    {
        yield return new WaitForSeconds(delay);
        dialogueManager.speaker = speaker;
        dialogueManager.isIntroDialogue = true;
        dialogueManager.StartDialogue(lines);
    }
}

