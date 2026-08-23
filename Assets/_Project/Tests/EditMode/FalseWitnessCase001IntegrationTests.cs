using System.Linq;
using FalseWitness.Investigation;
using NUnit.Framework;
using UnityEditor;

namespace FalseWitness.Investigation.Tests
{
    /// <summary>
    /// End-to-end integration tests for the real Case 001 content. Unlike the unit tests
    /// elsewhere in this suite, these load the actual authored ScriptableObject assets from
    /// disk and drive them exclusively through the public runtime API (DiscoverEvidence,
    /// HearStatement, PresentEvidence, CanAccuse, EvaluateAccusation, ResolveAccusation,
    /// Restart) - the same sequence of calls the eventual player-facing UI will make.
    /// </summary>
    public class FalseWitnessCase001IntegrationTests
    {
        private const string CasePath = "Assets/_Project/Data/Investigation/Case001/Case/case_001_1015_murder.asset";

        private CaseDefinition caseDefinition;
        private SuspectDefinition daniel;
        private SuspectDefinition evelyn;
        private Statement luggageStatement;
        private Statement corridorStatement;
        private Statement officeStatement;
        private EvidenceDefinition luggageEvidence;
        private EvidenceDefinition corridorEvidence;
        private EvidenceDefinition officeEvidence;

        [SetUp]
        public void SetUp()
        {
            caseDefinition = AssetDatabase.LoadAssetAtPath<CaseDefinition>(CasePath);
            Assert.IsNotNull(caseDefinition, $"Could not load the real Case 001 asset at {CasePath}");

            daniel = caseDefinition.Suspects.First(s => s.SuspectId == "daniel_reed");
            evelyn = caseDefinition.Suspects.First(s => s.SuspectId == "evelyn_carter");

            luggageStatement = daniel.Statements.First(s => s.StatementId == "daniel_01");
            corridorStatement = daniel.Statements.First(s => s.StatementId == "daniel_02");
            officeStatement = daniel.Statements.First(s => s.StatementId == "daniel_03");

            luggageEvidence = caseDefinition.Evidence.First(e => e.EvidenceId == "evidence_wilson_luggage_record");
            corridorEvidence = caseDefinition.Evidence.First(e => e.EvidenceId == "evidence_service_corridor_camera");
            officeEvidence = caseDefinition.Evidence.First(e => e.EvidenceId == "evidence_office_key_log");
        }

        /// <summary>
        /// Walks a fresh InvestigationController through the exact statement/evidence
        /// presentations a player would make, asserting each fact becomes established along
        /// the way rather than only checking the final state.
        /// </summary>
        private void CompleteInvestigation(InvestigationController investigation)
        {
            var solution = caseDefinition.Solution;

            Assert.IsFalse(investigation.CanAccuse());

            investigation.HearStatement(luggageStatement);
            investigation.DiscoverEvidence(luggageEvidence);
            var luggageResult = investigation.PresentEvidence(luggageStatement, luggageEvidence);
            Assert.AreEqual(PresentationResult.Contradicts, luggageResult);
            Assert.IsTrue(solution.RequiredFacts.Any(investigation.IsFactEstablished));

            investigation.HearStatement(corridorStatement);
            investigation.DiscoverEvidence(corridorEvidence);
            var corridorResult = investigation.PresentEvidence(corridorStatement, corridorEvidence);
            Assert.AreEqual(PresentationResult.Contradicts, corridorResult);

            investigation.HearStatement(officeStatement);
            investigation.DiscoverEvidence(officeEvidence);
            var officeResult = investigation.PresentEvidence(officeStatement, officeEvidence);
            Assert.AreEqual(PresentationResult.Contradicts, officeResult);

            foreach (var fact in solution.RequiredFacts)
            {
                Assert.IsTrue(investigation.IsFactEstablished(fact), $"Expected required fact '{fact.FactId}' to be established");
            }

            foreach (var evidence in solution.RequiredEvidence)
            {
                Assert.IsTrue(investigation.IsEvidenceDiscovered(evidence), $"Expected required evidence '{evidence.EvidenceId}' to be discovered");
            }

            Assert.IsTrue(investigation.CanAccuse());
        }

        [Test]
        public void CompleteInvestigation_CorrectAccusation_ResolvesSolved()
        {
            var investigation = new InvestigationController(caseDefinition);

            CompleteInvestigation(investigation);

            var resolution = new CaseResolutionController(investigation);
            var result = resolution.ResolveAccusation(daniel);

            Assert.AreEqual(CaseResolutionResult.Solved, result);
            Assert.AreEqual(CaseResolutionState.Solved, resolution.ResolutionState);
        }

        [Test]
        public void CompleteInvestigation_WrongAccusation_ResolvesFailed()
        {
            var investigation = new InvestigationController(caseDefinition);

            CompleteInvestigation(investigation);

            var resolution = new CaseResolutionController(investigation);
            var result = resolution.ResolveAccusation(evelyn);

            Assert.AreEqual(CaseResolutionResult.Failed, result);
            Assert.AreEqual(CaseResolutionState.Failed, resolution.ResolutionState);
        }

        [Test]
        public void IncompleteInvestigation_CannotResolve()
        {
            var investigation = new InvestigationController(caseDefinition);
            var resolution = new CaseResolutionController(investigation);

            var result = resolution.ResolveAccusation(daniel);

            Assert.AreEqual(CaseResolutionResult.Invalid, result);
            Assert.AreEqual(CaseResolutionState.InProgress, resolution.ResolutionState);
            Assert.IsFalse(investigation.CanAccuse());
        }

        [Test]
        public void FailedCase_SubsequentAccusationCannotChangeResult()
        {
            var investigation = new InvestigationController(caseDefinition);
            CompleteInvestigation(investigation);

            var resolution = new CaseResolutionController(investigation);
            var firstResult = resolution.ResolveAccusation(evelyn);
            Assert.AreEqual(CaseResolutionResult.Failed, firstResult);

            var secondResult = resolution.ResolveAccusation(daniel);

            Assert.AreEqual(CaseResolutionResult.Failed, secondResult);
            Assert.AreEqual(CaseResolutionState.Failed, resolution.ResolutionState);
        }

        [Test]
        public void SolvedCase_SubsequentAccusationCannotChangeResult()
        {
            var investigation = new InvestigationController(caseDefinition);
            CompleteInvestigation(investigation);

            var resolution = new CaseResolutionController(investigation);
            var firstResult = resolution.ResolveAccusation(daniel);
            Assert.AreEqual(CaseResolutionResult.Solved, firstResult);

            var secondResult = resolution.ResolveAccusation(evelyn);

            Assert.AreEqual(CaseResolutionResult.Solved, secondResult);
            Assert.AreEqual(CaseResolutionState.Solved, resolution.ResolutionState);
        }

        [Test]
        public void RestartAfterSolvedCase_ProducesFreshInvestigation()
        {
            var investigation1 = new InvestigationController(caseDefinition);
            CompleteInvestigation(investigation1);

            var resolution1 = new CaseResolutionController(investigation1);
            Assert.AreEqual(CaseResolutionResult.Solved, resolution1.ResolveAccusation(daniel));

            var investigation2 = investigation1.Restart();
            var resolution2 = new CaseResolutionController(investigation2);

            Assert.AreEqual(0, investigation2.GetDiscoveredEvidence().Count);
            Assert.IsTrue(caseDefinition.Evidence.All(e => !investigation2.IsEvidenceDiscovered(e)));
            Assert.IsFalse(investigation2.HasHeardStatement(luggageStatement));
            Assert.IsFalse(investigation2.HasHeardStatement(corridorStatement));
            Assert.IsFalse(investigation2.HasHeardStatement(officeStatement));
            Assert.IsTrue(caseDefinition.Solution.RequiredFacts.All(f => !investigation2.IsFactEstablished(f)));
            Assert.IsFalse(investigation2.CanAccuse());
            Assert.AreEqual(CaseResolutionState.InProgress, resolution2.ResolutionState);

            // The original, already-resolved investigation is untouched by the restart.
            Assert.AreEqual(CaseResolutionState.Solved, resolution1.ResolutionState);
            Assert.AreEqual(3, investigation1.GetDiscoveredEvidence().Count);
            Assert.IsTrue(caseDefinition.Solution.RequiredFacts.All(investigation1.IsFactEstablished));
        }

        [Test]
        public void RestartAfterFailedCase_ProducesFreshInvestigation()
        {
            var investigation1 = new InvestigationController(caseDefinition);
            CompleteInvestigation(investigation1);

            var resolution1 = new CaseResolutionController(investigation1);
            Assert.AreEqual(CaseResolutionResult.Failed, resolution1.ResolveAccusation(evelyn));

            var investigation2 = investigation1.Restart();
            var resolution2 = new CaseResolutionController(investigation2);

            Assert.AreEqual(0, investigation2.GetDiscoveredEvidence().Count);
            Assert.IsTrue(caseDefinition.Evidence.All(e => !investigation2.IsEvidenceDiscovered(e)));
            Assert.IsFalse(investigation2.HasHeardStatement(luggageStatement));
            Assert.IsFalse(investigation2.HasHeardStatement(corridorStatement));
            Assert.IsFalse(investigation2.HasHeardStatement(officeStatement));
            Assert.IsTrue(caseDefinition.Solution.RequiredFacts.All(f => !investigation2.IsFactEstablished(f)));
            Assert.IsFalse(investigation2.CanAccuse());
            Assert.AreEqual(CaseResolutionState.InProgress, resolution2.ResolutionState);
        }
    }
}
