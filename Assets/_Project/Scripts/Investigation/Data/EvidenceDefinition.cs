using System.Collections.Generic;
using UnityEngine;

namespace FalseWitness.Investigation
{
    [CreateAssetMenu(fileName = "Evidence", menuName = "False Witness/Investigation/Evidence")]
    public class EvidenceDefinition : ScriptableObject
    {
        [SerializeField] private string evidenceId;
        [SerializeField] private string evidenceName;
        [SerializeField, TextArea] private string description;
        [SerializeField] private List<FactDefinition> associatedFacts = new();

        public string EvidenceId => evidenceId;
        public string EvidenceName => evidenceName;
        public string Description => description;
        public IReadOnlyList<FactDefinition> AssociatedFacts => associatedFacts;
    }
}
