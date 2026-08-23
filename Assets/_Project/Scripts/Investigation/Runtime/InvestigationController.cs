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

        /// <summary>
        /// Creates a brand new InvestigationController for this same CaseDefinition, backed by
        /// its own fresh InvestigationState - no evidence discovered, no statements heard, no
        /// facts established. This instance (and its state) is left completely untouched; a
        /// restart is a fresh instance to switch to, never an in-place reset. Callers pair it
        /// with a new CaseResolutionController to get a clean resolution state as well.
        /// </summary>
        public InvestigationController Restart()
        {
            return new InvestigationController(ActiveCase);
        }

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

        public void EstablishFact(FactDefinition fact)
        {
            if (fact == null) throw new ArgumentNullException(nameof(fact));
            state.MarkFactEstablished(fact.FactId);
        }

        public bool IsFactEstablished(FactDefinition fact) =>
            fact != null && state.IsFactEstablished(fact.FactId);

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

        /// <summary>
        /// Evaluates the relationship between a single statement and a single piece of
        /// evidence, for the player-driven "present evidence against statement" action.
        /// Unlike <see cref="GetStatementStatus"/>, this does not aggregate over all
        /// discovered evidence - it answers only for the exact pairing presented, so the
        /// deduction stays with the player rather than being surfaced automatically.
        /// A contradiction takes priority over a supporting match if both are present.
        /// </summary>
        public PresentationResult EvaluatePresentation(Statement statement, EvidenceDefinition evidence)
        {
            if (statement?.AssociatedFact == null || evidence == null ||
                !HasHeardStatement(statement) || !IsEvidenceDiscovered(evidence))
            {
                return PresentationResult.Invalid;
            }

            var statementFact = statement.AssociatedFact;
            var supported = false;

            foreach (var fact in evidence.AssociatedFacts)
            {
                if (fact == null) continue;

                if (fact == statementFact)
                {
                    supported = true;
                }
                else if (FactsConflict(fact, statementFact))
                {
                    return PresentationResult.Contradicts;
                }
            }

            return supported ? PresentationResult.Supports : PresentationResult.Unknown;
        }

        /// <summary>
        /// Commits the result of a deliberate player presentation: evaluates the pairing via
        /// <see cref="EvaluatePresentation"/> (unchanged, side-effect free) and then, based on
        /// that result alone, records what the presentation established. Supports establishes
        /// the statement's own fact; Contradicts establishes the evidence-side fact(s) that
        /// conflicted with it, never the statement's fact; Unknown and Invalid establish nothing.
        /// </summary>
        public PresentationResult PresentEvidence(Statement statement, EvidenceDefinition evidence)
        {
            var result = EvaluatePresentation(statement, evidence);

            switch (result)
            {
                case PresentationResult.Supports:
                    EstablishFact(statement.AssociatedFact);
                    break;
                case PresentationResult.Contradicts:
                    EstablishConflictingEvidenceFacts(statement.AssociatedFact, evidence);
                    break;
            }

            return result;
        }

        /// <summary>
        /// Reports whether the player currently has everything needed to make an accusation:
        /// an active case with a solution, every required evidence item discovered, and every
        /// required fact established. A pure query - checks the same requirements
        /// <see cref="EvaluateAccusation"/> does, but without judging any particular suspect, so
        /// callers can gate the accusation action before a suspect is even chosen.
        /// </summary>
        public bool CanAccuse()
        {
            var activeCase = state.ActiveCase;
            var solution = activeCase?.Solution;

            if (activeCase == null || solution == null)
            {
                return false;
            }

            foreach (var evidence in solution.RequiredEvidence)
            {
                if (!IsEvidenceDiscovered(evidence))
                {
                    return false;
                }
            }

            foreach (var fact in solution.RequiredFacts)
            {
                if (!IsFactEstablished(fact))
                {
                    return false;
                }
            }

            return true;
        }

        /// <summary>
        /// Evaluates a player's final accusation against the case's solution. A pure query -
        /// it never marks evidence discovered, establishes facts, or records the accusation.
        /// Invalid whenever the accusation can't yet be judged (no suspect, no solution, no
        /// correct suspect configured, or the required evidence/facts aren't all in hand yet);
        /// otherwise Correct or Incorrect based on whether the accused suspect is the solution's
        /// correct suspect, compared by object reference rather than id or name.
        /// </summary>
        public AccusationResult EvaluateAccusation(SuspectDefinition suspect)
        {
            var solution = ActiveCase.Solution;

            if (suspect == null || solution == null || solution.CorrectSuspect == null)
            {
                return AccusationResult.Invalid;
            }

            foreach (var fact in solution.RequiredFacts)
            {
                if (!IsFactEstablished(fact))
                {
                    return AccusationResult.Invalid;
                }
            }

            foreach (var evidence in solution.RequiredEvidence)
            {
                if (!IsEvidenceDiscovered(evidence))
                {
                    return AccusationResult.Invalid;
                }
            }

            return suspect == solution.CorrectSuspect ? AccusationResult.Correct : AccusationResult.Incorrect;
        }

        /// <summary>
        /// Establishes every fact on the evidence that conflicts with the statement's fact.
        /// EvaluatePresentation short-circuits on the first conflict it finds; this scans all
        /// of evidence's facts independently so a Contradicts result establishes every
        /// conflicting evidence-side fact, not just whichever one evaluation happened to hit first.
        /// </summary>
        private void EstablishConflictingEvidenceFacts(FactDefinition statementFact, EvidenceDefinition evidence)
        {
            foreach (var fact in evidence.AssociatedFacts)
            {
                if (fact == null || fact == statementFact) continue;

                if (FactsConflict(fact, statementFact))
                {
                    EstablishFact(fact);
                }
            }
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
