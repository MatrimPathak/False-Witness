namespace FalseWitness.Investigation
{
    /// <summary>
    /// Whether the case itself is still active, or has been permanently resolved. Distinct
    /// from InvestigationState, which tracks investigation progress (evidence discovered,
    /// statements heard, facts established) rather than the outcome of an accusation.
    /// </summary>
    public enum CaseResolutionState
    {
        InProgress,
        Solved,
        Failed
    }
}
