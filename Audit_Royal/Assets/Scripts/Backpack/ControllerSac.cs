using UnityEngine;
using UnityEngine.UI;
using System.IO;
using Newtonsoft.Json;
using TMPro;
using System.Text;

/// <summary>
/// Contrôle l'ouverture et la fermeture du sac à dos du joueur via l'Animator.
/// </summary>
public class ControllerSac : MonoBehaviour
{
	/// <summary>
	/// Référence à l'Animator attaché au sac à dos.
	/// </summary>
	public Animator backPackAnimator;
	
	/// <summary>
	/// Indique si le sac est ouvert (true) ou fermé (false).
	/// </summary>
	private bool isOpen=false;

	/// <summary>
	/// Référence vers la feuille pour afficher l'audit antérieur.
	/// </summary>
	public GameObject auditSheetPanel;

	/// <summary>
	/// Référence vers le texte de la feuille.
	/// </summary>
	public TextMeshProUGUI auditSheetText;

	/// <summary>
	/// Bouton pour fermer l'audit.
	/// </summary>
	public Button btnClose;

	/// <summary>
	/// Bascule l'état du sac à dos : ouvert ou fermé.
	/// Met à jour le bool "isOpen" dans l'Animator.
	/// </summary>
	public void ToggleMenu() {
		isOpen = !isOpen;
		backPackAnimator.SetBool("isOpen", isOpen);
	}

	public void displayAudit()
	{
		int numeroScenario = GameStateManager.Instance.ScenarioActuel;
		string nomFichier = $"scenario{numeroScenario}_audit_anterieur.json";
		string filePath = Path.Combine(Application.streamingAssetsPath, nomFichier);

		Debug.Log($"File : {filePath}");
		string json = File.ReadAllText(filePath);
		AuditWrapper wrapper = JsonUtility.FromJson<AuditWrapper>(json);
		Audit audit = wrapper.audit_anterieur;

		auditSheetText.text = BuildFormattedAudit(audit);
		
		auditSheetPanel.SetActive(true);
	}

	public void closeAudit()
	{
		auditSheetPanel.SetActive(false);
	}
	
	private string BuildFormattedAudit(Audit audit)
	{
		StringBuilder sb = new StringBuilder();

		sb.AppendLine("<size=140%><b>" + audit.titre + "</b></size>\n");
		sb.AppendLine("<b>Établissement :</b> " + audit.etablissement);
		sb.AppendLine("<b>Date :</b> " + audit.date);
		sb.AppendLine("<b>Service audité :</b> " + audit.service_audite + "\n");

		sb.AppendLine("<size=120%><b>Objectifs</b></size>");
		foreach (string obj in audit.objectifs)
			sb.AppendLine("• " + obj);

		sb.AppendLine("\n<size=120%><b>Constatations</b></size>");
		AddSection(sb, "Points conformes", audit.constatations.points_conformes);
		AddSection(sb, "Points de vigilance", audit.constatations.points_vigilance);
		AddSection(sb, "Non-conformités", audit.constatations.non_conformites);

		sb.AppendLine("\n<size=120%><b>Analyse des causes</b></size>");
		foreach (string cause in audit.analyse_causes)
			sb.AppendLine("• " + cause);

		sb.AppendLine("\n<size=120%><b>Recommandations</b></size>");
		foreach (Recommendation r in audit.recommandations)
		{
			sb.AppendLine(
				$"• <b>{r.description}</b>\n" +
				$"  Priorité : {r.priorite} | État : {r.etat_mise_en_oeuvre}"
			);
		}

		sb.AppendLine("\n<size=120%><b>Conclusion</b></size>");
		sb.AppendLine(audit.conclusion.resume);
		sb.AppendLine("\n<b>Risques identifiés :</b>");
		foreach (string risk in audit.conclusion.risques_identifies)
			sb.AppendLine("• " + risk);

		sb.AppendLine("\n<b>Niveau de risque :</b> " + audit.conclusion.niveau_risque);

		return sb.ToString();
	}

	private void AddSection(StringBuilder sb, string title, string[] items)
	{
		sb.AppendLine($"\n<b>{title}</b>");
		foreach (string item in items)
			sb.AppendLine("• " + item);
	}
}
