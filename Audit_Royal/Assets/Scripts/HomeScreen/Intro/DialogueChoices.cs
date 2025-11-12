using UnityEngine;
using UnityEngine.EventSystems;
using TMPro;

public class DialogueChoices : MonoBehaviour, IPointerClickHandler
{
    public TextMeshProUGUI dialogueText; // DialogueText qui contient les <link>
    public DialogueManager dialogueManager; // pour appeler la suite

    // appellé quand on clique sur un lien
    public void OnPointerClick(PointerEventData eventData)
    {
        int linkIndex = TMP_TextUtilities.FindIntersectingLink(dialogueText, Input.mousePosition, null);
        if (linkIndex != -1)
        {
            TMP_LinkInfo linkInfo = dialogueText.textInfo.linkInfo[linkIndex];
            string linkID = linkInfo.GetLinkID();
            string linkText = linkInfo.GetLinkText();

            dialogueManager.OnChoiceSelected(linkID);
        }
    }
}
