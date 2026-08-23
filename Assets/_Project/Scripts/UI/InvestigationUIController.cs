using System.Collections.Generic;
using FalseWitness.Investigation;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace FalseWitness.UI
{
    /// <summary>
    /// Investigation screen UI. Renders the active case's suspects, the selected
    /// suspect's statements, and the case's evidence; tracks selection; and drives
    /// the existing <see cref="InvestigationController"/> to evaluate a presented
    /// statement/evidence pair. All deduction logic stays in the domain controller -
    /// this class only forwards the player's selection and displays the result.
    /// </summary>
    public class InvestigationUIController : MonoBehaviour
    {
        [Header("Case Data")]
        [SerializeField] private CaseDefinition activeCase;

        [Header("Header")]
        [SerializeField] private TMP_Text caseTitleText;
        [SerializeField] private TMP_Text caseDescriptionText;

        [Header("Lists")]
        [SerializeField] private Transform suspectListContainer;
        [SerializeField] private Transform statementListContainer;
        [SerializeField] private Transform evidenceListContainer;
        [SerializeField] private SelectableListItem listItemPrefab;

        [Header("Actions")]
        [SerializeField] private Button presentEvidenceButton;

        [Header("Result")]
        [SerializeField] private GameObject resultModal;
        [SerializeField] private TMP_Text resultText;
        [SerializeField] private Button resultCloseButton;

        private readonly List<SelectableListItem> suspectItems = new();
        private readonly List<SelectableListItem> statementItems = new();
        private readonly List<SelectableListItem> evidenceItems = new();

        private InvestigationController investigation;

        private SuspectDefinition selectedSuspect;
        private Statement selectedStatement;
        private EvidenceDefinition selectedEvidence;

        private void Start()
        {
            if (presentEvidenceButton != null)
            {
                presentEvidenceButton.interactable = false;
                presentEvidenceButton.onClick.AddListener(HandlePresentEvidenceClicked);
            }

            if (resultCloseButton != null)
            {
                resultCloseButton.onClick.AddListener(HideResult);
            }

            if (resultModal != null)
            {
                resultModal.SetActive(false);
            }

            if (activeCase == null)
            {
                Debug.LogWarning("InvestigationUIController has no active CaseDefinition assigned.");
                return;
            }

            investigation = new InvestigationController(activeCase);

            // This prototype has no separate evidence-discovery interaction yet - every
            // evidence item is already presented to the player in the Evidence panel, so
            // mark it discovered via the existing controller API rather than bypassing it.
            foreach (var evidence in activeCase.Evidence)
            {
                investigation.DiscoverEvidence(evidence);
            }

            RenderHeader();
            RenderSuspects();
            RenderEvidence();

            if (activeCase.Suspects.Count > 0)
            {
                SelectSuspect(activeCase.Suspects[0]);
            }
        }

        private void RenderHeader()
        {
            if (caseTitleText != null) caseTitleText.text = activeCase.Title;
            if (caseDescriptionText != null) caseDescriptionText.text = activeCase.Description;
        }

        private void RenderSuspects()
        {
            ClearItems(suspectListContainer, suspectItems);

            foreach (var suspect in activeCase.Suspects)
            {
                var item = CreateItem(suspectListContainer, suspect.SuspectName, () => SelectSuspect(suspect));
                suspectItems.Add(item);
            }
        }

        private void RenderStatements()
        {
            ClearItems(statementListContainer, statementItems);

            if (selectedSuspect == null) return;

            foreach (var statement in selectedSuspect.Statements)
            {
                var item = CreateItem(statementListContainer, statement.Text, () => SelectStatement(statement));
                statementItems.Add(item);
            }
        }

        private void RenderEvidence()
        {
            ClearItems(evidenceListContainer, evidenceItems);

            foreach (var evidence in activeCase.Evidence)
            {
                var item = CreateItem(evidenceListContainer, evidence.EvidenceName, () => SelectEvidence(evidence));
                evidenceItems.Add(item);
            }
        }

        private void SelectSuspect(SuspectDefinition suspect)
        {
            selectedSuspect = suspect;
            selectedStatement = null;

            for (var i = 0; i < activeCase.Suspects.Count && i < suspectItems.Count; i++)
            {
                suspectItems[i].SetSelected(activeCase.Suspects[i] == suspect);
            }

            RenderStatements();
            UpdatePresentEvidenceAvailability();
        }

        private void SelectStatement(Statement statement)
        {
            selectedStatement = statement;

            // Selecting a statement in the UI is how the player "hears" it - mark it
            // through the existing controller API so EvaluatePresentation sees real state.
            investigation.HearStatement(statement);

            var statements = selectedSuspect.Statements;
            for (var i = 0; i < statements.Count && i < statementItems.Count; i++)
            {
                statementItems[i].SetSelected(statements[i] == statement);
            }

            UpdatePresentEvidenceAvailability();
        }

        private void SelectEvidence(EvidenceDefinition evidence)
        {
            selectedEvidence = evidence;

            var evidenceList = activeCase.Evidence;
            for (var i = 0; i < evidenceList.Count && i < evidenceItems.Count; i++)
            {
                evidenceItems[i].SetSelected(evidenceList[i] == evidence);
            }

            UpdatePresentEvidenceAvailability();
        }

        private void UpdatePresentEvidenceAvailability()
        {
            if (presentEvidenceButton == null) return;

            var canPresent = selectedStatement != null && selectedEvidence != null &&
                              investigation.HasHeardStatement(selectedStatement) &&
                              investigation.IsEvidenceDiscovered(selectedEvidence);

            presentEvidenceButton.interactable = canPresent;
        }

        private void HandlePresentEvidenceClicked()
        {
            if (selectedStatement == null || selectedEvidence == null) return;

            var result = investigation.EvaluatePresentation(selectedStatement, selectedEvidence);
            ShowResult(result);
        }

        private void ShowResult(PresentationResult result)
        {
            if (resultText != null)
            {
                resultText.text = result switch
                {
                    PresentationResult.Contradicts => "Contradiction Found",
                    PresentationResult.Supports => "The evidence supports this statement.",
                    PresentationResult.Unknown => "The evidence does not establish a connection.",
                    PresentationResult.Invalid => "This evidence cannot be presented.",
                    _ => string.Empty
                };
            }

            if (resultModal != null) resultModal.SetActive(true);
        }

        private void HideResult()
        {
            if (resultModal != null) resultModal.SetActive(false);
        }

        private SelectableListItem CreateItem(Transform container, string label, System.Action onClick)
        {
            var item = Instantiate(listItemPrefab, container);
            item.Setup(label, onClick);
            return item;
        }

        private static void ClearItems(Transform container, List<SelectableListItem> items)
        {
            foreach (var item in items)
            {
                if (item != null) Destroy(item.gameObject);
            }
            items.Clear();

            if (container == null) return;

            for (var i = container.childCount - 1; i >= 0; i--)
            {
                Destroy(container.GetChild(i).gameObject);
            }
        }
    }
}
