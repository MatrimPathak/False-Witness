using System;

namespace FalseWitness.Investigation
{
    /// <summary>
    /// Plain C# runtime service that performs the final case-resolution action. Delegates the
    /// actual accusation judgement to <see cref="InvestigationController.EvaluateAccusation"/>
    /// rather than re-checking suspects/evidence/facts itself, and layers a permanent
    /// Solved/Failed outcome on top - once resolved, further accusations can no longer change
    /// the result. No Unity dependencies - not a MonoBehaviour, not a singleton, not scene-resident.
    /// </summary>
    public class CaseResolutionController
    {
        private readonly InvestigationController investigationController;

        public CaseResolutionController(InvestigationController investigationController)
        {
            this.investigationController = investigationController
                ?? throw new ArgumentNullException(nameof(investigationController));
        }

        public CaseResolutionState ResolutionState { get; private set; } = CaseResolutionState.InProgress;

        /// <summary>
        /// Judges the given suspect against the case's solution via EvaluateAccusation. Once
        /// the case has been resolved (Solved or Failed), that outcome is permanent: this
        /// returns the recorded result immediately without evaluating a new accusation. While
        /// still InProgress, an Invalid accusation records nothing; a Correct or Incorrect one
        /// resolves the case for good.
        /// </summary>
        public CaseResolutionResult ResolveAccusation(SuspectDefinition suspect)
        {
            if (ResolutionState == CaseResolutionState.Solved)
            {
                return CaseResolutionResult.Solved;
            }

            if (ResolutionState == CaseResolutionState.Failed)
            {
                return CaseResolutionResult.Failed;
            }

            switch (investigationController.EvaluateAccusation(suspect))
            {
                case AccusationResult.Correct:
                    ResolutionState = CaseResolutionState.Solved;
                    return CaseResolutionResult.Solved;
                case AccusationResult.Incorrect:
                    ResolutionState = CaseResolutionState.Failed;
                    return CaseResolutionResult.Failed;
                default:
                    return CaseResolutionResult.Invalid;
            }
        }
    }
}
