using System;
using System.Collections.Generic;

namespace FalseWitness.Investigation
{
    /// <summary>
    /// Plain C# runtime service for a single active case. Owns an <see cref="InvestigationState"/>
    /// and exposes the operations a future UI layer will call. No Unity UI or GameObject
    /// dependencies, and no assumption about how many statements end up false.
    /// </summary>
    public class InvestigationController
    {
        private readonly InvestigationState state;

        public InvestigationController(CaseDefinition activeCase)
        {
            state = new InvestigationState(activeCase);
        }

        public CaseDefinition ActiveCase => state.ActiveCase;

        public void DiscoverEvidence(EvidenceDefinition evidence)
        {
            if (evidence == null) throw new ArgumentNullException(nameof(evidence));
            state.MarkEvidenceDiscovered(evidence.EvidenceId);
        }

        public bool IsEvidenceDiscovered(EvidenceDefinition evidence) =>
            evidence != null && state.IsEvidenceDiscovered(evidence.EvidenceId);

        public void HearStatement(Statement statement)
        {
            if (statement == null) throw new ArgumentNullException(nameof(statement));
            state.MarkStatementHeard(statement.StatementId);
        }

        public bool HasHeardStatement(Statement statement) =>
            statement != null && state.HasHeardStatement(statement.StatementId);

        public IReadOnlyList<EvidenceDefinition> GetDiscoveredEvidence()
        {
            var discovered = new List<EvidenceDefinition>();
            foreach (var evidence in state.ActiveCase.Evidence)
            {
                if (state.IsEvidenceDiscovered(evidence.EvidenceId))
                {
                    discovered.Add(evidence);
                }
            }
            return discovered;
        }

        /// <summary>
        /// Determines the statement's status from currently discovered evidence alone -
        /// nothing here assumes any fixed number of statements are false.
        /// Unknown until the statement has been heard and discovered evidence bears on its fact.
        /// A contradiction takes priority over a supporting match if both are present.
        /// </summary>
        public StatementStatus GetStatementStatus(Statement statement)
        {
            if (statement?.AssociatedFact == null || !HasHeardStatement(statement))
            {
                return StatementStatus.Unknown;
            }

            var statementFact = statement.AssociatedFact;
            var supported = false;

            foreach (var evidence in GetDiscoveredEvidence())
            {
                foreach (var fact in evidence.AssociatedFacts)
                {
                    if (fact == null) continue;

                    if (fact == statementFact)
                    {
                        supported = true;
                    }
                    else if (FactsConflict(fact, statementFact))
                    {
                        return StatementStatus.Contradicted;
                    }
                }
            }

            return supported ? StatementStatus.Supported : StatementStatus.Unknown;
        }

        private static bool FactsConflict(FactDefinition a, FactDefinition b)
        {
            return ContainsFact(a.ConflictingFacts, b) || ContainsFact(b.ConflictingFacts, a);
        }

        private static bool ContainsFact(IReadOnlyList<FactDefinition> facts, FactDefinition target)
        {
            for (var i = 0; i < facts.Count; i++)
            {
                if (facts[i] == target) return true;
            }
            return false;
        }
    }
}
