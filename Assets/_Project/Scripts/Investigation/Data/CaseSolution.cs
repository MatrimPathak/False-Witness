using System.Collections.Generic;
using UnityEngine;

namespace FalseWitness.Investigation
{
    /// <summary>
    /// Authoring-only data describing the solution to a case: who did it, and which
    /// evidence/facts establish that. Kept as its own asset (rather than fields on
    /// CaseDefinition) so a case's playable content and its solution stay modular -
    /// this class has no behavior and is not read by any runtime logic yet.
    /// </summary>
    [CreateAssetMenu(fileName = "CaseSolution", menuName = "False Witness/Investigation/Case Solution")]
    public class CaseSolution : ScriptableObject
    {
        [SerializeField] private SuspectDefinition correctSuspect;
        [SerializeField] private List<EvidenceDefinition> requiredEvidence = new();
        [SerializeField] private List<FactDefinition> requiredFacts = new();

        public SuspectDefinition CorrectSuspect => correctSuspect;
        public IReadOnlyList<EvidenceDefinition> RequiredEvidence => requiredEvidence;
        public IReadOnlyList<FactDefinition> RequiredFacts => requiredFacts;
    }
}
