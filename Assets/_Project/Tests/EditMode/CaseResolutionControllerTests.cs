using FalseWitness.Investigation;
using NUnit.Framework;

namespace FalseWitness.Investigation.Tests
{
    public class CaseResolutionControllerTests
    {
        [Test]
        public void ResolutionState_FreshController_IsInProgress()
        {
            var caseDefinition = InvestigationTestFactory.CreateCase("case-1");
            var investigationController = new InvestigationController(caseDefinition);
            var resolutionController = new CaseResolutionController(investigationController);

            Assert.AreEqual(CaseResolutionState.InProgress, resolutionController.ResolutionState);
        }

        [Test]
        public void ResolveAccusation_BeforeRequirementsSatisfied_ReturnsInvalidAndStaysInProgress()
        {
            var correctSuspect = InvestigationTestFactory.CreateSuspect("sus-correct", "Correct");
            var evidence = InvestigationTestFactory.CreateEvidence("ev-1", "Evidence");
            var fact = InvestigationTestFactory.CreateFact("fact-1");
            var solution = InvestigationTestFactory.CreateCaseSolution(correctSuspect, new[] { evidence }, new[] { fact });
            var caseDefinition = InvestigationTestFactory.CreateCase("case-1", evidence);
            InvestigationTestFactory.AttachSolution(caseDefinition, solution);
            var investigationController = new InvestigationController(caseDefinition);
            var resolutionController = new CaseResolutionController(investigationController);

            // Nothing discovered or established yet.
            var result = resolutionController.ResolveAccusation(correctSuspect);

            Assert.AreEqual(CaseResolutionResult.Invalid, result);
            Assert.AreEqual(CaseResolutionState.InProgress, resolutionController.ResolutionState);
        }

        [Test]
        public void ResolveAccusation_WrongSuspectAfterRequirementsSatisfied_ReturnsFailedAndRecordsFailed()
        {
            var correctSuspect = InvestigationTestFactory.CreateSuspect("sus-correct", "Correct");
            var wrongSuspect = InvestigationTestFactory.CreateSuspect("sus-wrong", "Wrong");
            var evidence = InvestigationTestFactory.CreateEvidence("ev-1", "Evidence");
            var fact = InvestigationTestFactory.CreateFact("fact-1");
            var solution = InvestigationTestFactory.CreateCaseSolution(correctSuspect, new[] { evidence }, new[] { fact });
            var caseDefinition = InvestigationTestFactory.CreateCase("case-1", evidence);
            InvestigationTestFactory.AttachSolution(caseDefinition, solution);
            var investigationController = new InvestigationController(caseDefinition);
            var resolutionController = new CaseResolutionController(investigationController);

            investigationController.DiscoverEvidence(evidence);
            investigationController.EstablishFact(fact);

            var result = resolutionController.ResolveAccusation(wrongSuspect);

            Assert.AreEqual(CaseResolutionResult.Failed, result);
            Assert.AreEqual(CaseResolutionState.Failed, resolutionController.ResolutionState);
        }

        [Test]
        public void ResolveAccusation_CorrectSuspectAfterRequirementsSatisfied_ReturnsSolvedAndRecordsSolved()
        {
            var correctSuspect = InvestigationTestFactory.CreateSuspect("sus-correct", "Correct");
            var evidence = InvestigationTestFactory.CreateEvidence("ev-1", "Evidence");
            var fact = InvestigationTestFactory.CreateFact("fact-1");
            var solution = InvestigationTestFactory.CreateCaseSolution(correctSuspect, new[] { evidence }, new[] { fact });
            var caseDefinition = InvestigationTestFactory.CreateCase("case-1", evidence);
            InvestigationTestFactory.AttachSolution(caseDefinition, solution);
            var investigationController = new InvestigationController(caseDefinition);
            var resolutionController = new CaseResolutionController(investigationController);

            investigationController.DiscoverEvidence(evidence);
            investigationController.EstablishFact(fact);

            var result = resolutionController.ResolveAccusation(correctSuspect);

            Assert.AreEqual(CaseResolutionResult.Solved, result);
            Assert.AreEqual(CaseResolutionState.Solved, resolutionController.ResolutionState);
        }

        [Test]
        public void ResolveAccusation_AfterSolved_FurtherAccusationsCannotChangeResult()
        {
            var correctSuspect = InvestigationTestFactory.CreateSuspect("sus-correct", "Correct");
            var wrongSuspect = InvestigationTestFactory.CreateSuspect("sus-wrong", "Wrong");
            var evidence = InvestigationTestFactory.CreateEvidence("ev-1", "Evidence");
            var fact = InvestigationTestFactory.CreateFact("fact-1");
            var solution = InvestigationTestFactory.CreateCaseSolution(correctSuspect, new[] { evidence }, new[] { fact });
            var caseDefinition = InvestigationTestFactory.CreateCase("case-1", evidence);
            InvestigationTestFactory.AttachSolution(caseDefinition, solution);
            var investigationController = new InvestigationController(caseDefinition);
            var resolutionController = new CaseResolutionController(investigationController);

            investigationController.DiscoverEvidence(evidence);
            investigationController.EstablishFact(fact);

            var firstResult = resolutionController.ResolveAccusation(correctSuspect);
            Assert.AreEqual(CaseResolutionResult.Solved, firstResult);

            var secondResult = resolutionController.ResolveAccusation(wrongSuspect);

            Assert.AreEqual(CaseResolutionResult.Solved, secondResult);
            Assert.AreEqual(CaseResolutionState.Solved, resolutionController.ResolutionState);
        }

        [Test]
        public void ResolveAccusation_AfterFailed_FurtherAccusationsCannotChangeResult()
        {
            var correctSuspect = InvestigationTestFactory.CreateSuspect("sus-correct", "Correct");
            var wrongSuspect = InvestigationTestFactory.CreateSuspect("sus-wrong", "Wrong");
            var evidence = InvestigationTestFactory.CreateEvidence("ev-1", "Evidence");
            var fact = InvestigationTestFactory.CreateFact("fact-1");
            var solution = InvestigationTestFactory.CreateCaseSolution(correctSuspect, new[] { evidence }, new[] { fact });
            var caseDefinition = InvestigationTestFactory.CreateCase("case-1", evidence);
            InvestigationTestFactory.AttachSolution(caseDefinition, solution);
            var investigationController = new InvestigationController(caseDefinition);
            var resolutionController = new CaseResolutionController(investigationController);

            investigationController.DiscoverEvidence(evidence);
            investigationController.EstablishFact(fact);

            var firstResult = resolutionController.ResolveAccusation(wrongSuspect);
            Assert.AreEqual(CaseResolutionResult.Failed, firstResult);

            var secondResult = resolutionController.ResolveAccusation(correctSuspect);

            Assert.AreEqual(CaseResolutionResult.Failed, secondResult);
            Assert.AreEqual(CaseResolutionState.Failed, resolutionController.ResolutionState);
        }

        [Test]
        public void ResolveAccusation_DoesNotModifyInvestigationStateBeyondWhatAlreadyExisted()
        {
            var correctSuspect = InvestigationTestFactory.CreateSuspect("sus-correct", "Correct");
            var requiredEvidence = InvestigationTestFactory.CreateEvidence("ev-required", "Required");
            var requiredFact = InvestigationTestFactory.CreateFact("fact-required");
            var unrelatedEvidence = InvestigationTestFactory.CreateEvidence("ev-unrelated", "Unrelated");
            var unrelatedFact = InvestigationTestFactory.CreateFact("fact-unrelated");
            var solution = InvestigationTestFactory.CreateCaseSolution(
                correctSuspect, new[] { requiredEvidence }, new[] { requiredFact });
            var caseDefinition = InvestigationTestFactory.CreateCase("case-1", requiredEvidence, unrelatedEvidence);
            InvestigationTestFactory.AttachSolution(caseDefinition, solution);
            var investigationController = new InvestigationController(caseDefinition);
            var resolutionController = new CaseResolutionController(investigationController);

            investigationController.DiscoverEvidence(requiredEvidence);
            investigationController.EstablishFact(requiredFact);

            resolutionController.ResolveAccusation(correctSuspect);

            Assert.IsTrue(investigationController.IsEvidenceDiscovered(requiredEvidence));
            Assert.IsTrue(investigationController.IsFactEstablished(requiredFact));
            Assert.IsFalse(investigationController.IsEvidenceDiscovered(unrelatedEvidence));
            Assert.IsFalse(investigationController.IsFactEstablished(unrelatedFact));
            Assert.AreEqual(1, investigationController.GetDiscoveredEvidence().Count);
        }

        [Test]
        public void ResolveAccusation_NullSuspect_ReturnsInvalid()
        {
            var caseDefinition = InvestigationTestFactory.CreateCase("case-1");
            var investigationController = new InvestigationController(caseDefinition);
            var resolutionController = new CaseResolutionController(investigationController);

            var result = resolutionController.ResolveAccusation(null);

            Assert.AreEqual(CaseResolutionResult.Invalid, result);
            Assert.AreEqual(CaseResolutionState.InProgress, resolutionController.ResolutionState);
        }

        [Test]
        public void ResolveAccusation_DelegatesToEvaluateAccusation_ReferenceComparisonIsHonored()
        {
            var correctSuspect = InvestigationTestFactory.CreateSuspect("sus-1", "Same Name");
            var lookalikeSuspect = InvestigationTestFactory.CreateSuspect("sus-1", "Same Name");
            var evidence = InvestigationTestFactory.CreateEvidence("ev-1", "Evidence");
            var fact = InvestigationTestFactory.CreateFact("fact-1");
            var solution = InvestigationTestFactory.CreateCaseSolution(correctSuspect, new[] { evidence }, new[] { fact });
            var caseDefinition = InvestigationTestFactory.CreateCase("case-1", evidence);
            InvestigationTestFactory.AttachSolution(caseDefinition, solution);
            var investigationController = new InvestigationController(caseDefinition);
            var resolutionController = new CaseResolutionController(investigationController);

            investigationController.DiscoverEvidence(evidence);
            investigationController.EstablishFact(fact);

            // Same id/name as the correct suspect but a distinct SuspectDefinition instance.
            // This only comes back Failed (not Solved) if resolution genuinely delegates to
            // EvaluateAccusation's reference-based comparison instead of re-implementing its own.
            var result = resolutionController.ResolveAccusation(lookalikeSuspect);

            Assert.AreEqual(CaseResolutionResult.Failed, result);
            Assert.AreEqual(CaseResolutionState.Failed, resolutionController.ResolutionState);
        }
    }
}
