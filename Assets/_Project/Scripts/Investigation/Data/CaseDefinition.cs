using System.Collections.Generic;
using UnityEngine;

namespace FalseWitness.Investigation
{
    [CreateAssetMenu(fileName = "Case", menuName = "False Witness/Investigation/Case")]
    public class CaseDefinition : ScriptableObject
    {
        [SerializeField] private string caseId;
        [SerializeField] private string title;
        [SerializeField, TextArea] private string description;
        [SerializeField] private List<SuspectDefinition> suspects = new();
        [SerializeField] private List<EvidenceDefinition> evidence = new();
        [SerializeField] private CaseSolution solution;

        public string CaseId => caseId;
        public string Title => title;
        public string Description => description;
        public IReadOnlyList<SuspectDefinition> Suspects => suspects;
        public IReadOnlyList<EvidenceDefinition> Evidence => evidence;
        public CaseSolution Solution => solution;
    }
}
