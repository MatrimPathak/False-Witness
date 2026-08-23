using System;
using System.Collections.Generic;

namespace FalseWitness.Investigation
{
    public class InvestigationState
    {
        private readonly CaseDefinition activeCase;
        private readonly HashSet<string> discoveredEvidenceIds = new();
        private readonly HashSet<string> heardStatementIds = new();

        public InvestigationState(CaseDefinition activeCase)
        {
            this.activeCase = activeCase ?? throw new ArgumentNullException(nameof(activeCase));
        }

        public CaseDefinition ActiveCase => activeCase;

        public bool IsEvidenceDiscovered(string evidenceId) => discoveredEvidenceIds.Contains(evidenceId);
        public void MarkEvidenceDiscovered(string evidenceId) => discoveredEvidenceIds.Add(evidenceId);

        public bool HasHeardStatement(string statementId) => heardStatementIds.Contains(statementId);
        public void MarkStatementHeard(string statementId) => heardStatementIds.Add(statementId);
    }
}
