using FalseWitness.Investigation;
using NUnit.Framework;
using UnityEngine;

namespace FalseWitness.Investigation.Tests
{
    public class InvestigationControllerTests
    {
        [Test]
        public void GetStatementStatus_NoAssociatedFact_ReturnsUnknown()
        {
            var evidence = InvestigationTestFactory.CreateEvidence("ev-1", "Evidence");
            var statement = InvestigationTestFactory.CreateStatement("st-1", "text", null);
            var controller = new InvestigationController(InvestigationTestFactory.CreateCase("case-1", evidence));

            controller.HearStatement(statement);
            controller.DiscoverEvidence(evidence);

            Assert.AreEqual(StatementStatus.Unknown, controller.GetStatementStatus(statement));
        }

        [Test]
        public void GetStatementStatus_StatementNotHeard_ReturnsUnknown()
        {
            var fact = InvestigationTestFactory.CreateFact("fact-1");
            var evidence = InvestigationTestFactory.CreateEvidence("ev-1", "Evidence", fact);
            var statement = InvestigationTestFactory.CreateStatement("st-1", "text", fact);
            var controller = new InvestigationController(InvestigationTestFactory.CreateCase("case-1", evidence));

            controller.DiscoverEvidence(evidence);
            // HearStatement intentionally not called.

            Assert.AreEqual(StatementStatus.Unknown, controller.GetStatementStatus(statement));
        }

        [Test]
        public void GetStatementStatus_HeardWithNoRelevantEvidence_ReturnsUnknown()
        {
            var statementFact = InvestigationTestFactory.CreateFact("fact-statement");
            var unrelatedFact = InvestigationTestFactory.CreateFact("fact-unrelated");
            var evidence = InvestigationTestFactory.CreateEvidence("ev-1", "Evidence", unrelatedFact);
            var statement = InvestigationTestFactory.CreateStatement("st-1", "text", statementFact);
            var controller = new InvestigationController(InvestigationTestFactory.CreateCase("case-1", evidence));

            controller.HearStatement(statement);
            controller.DiscoverEvidence(evidence);

            Assert.AreEqual(StatementStatus.Unknown, controller.GetStatementStatus(statement));
        }

        [Test]
        public void GetStatementStatus_DiscoveredEvidenceContainsStatementFact_ReturnsSupported()
        {
            var fact = InvestigationTestFactory.CreateFact("fact-1");
            var evidence = InvestigationTestFactory.CreateEvidence("ev-1", "Evidence", fact);
            var statement = InvestigationTestFactory.CreateStatement("st-1", "text", fact);
            var controller = new InvestigationController(InvestigationTestFactory.CreateCase("case-1", evidence));

            controller.HearStatement(statement);
            controller.DiscoverEvidence(evidence);

            Assert.AreEqual(StatementStatus.Supported, controller.GetStatementStatus(statement));
        }

        [Test]
        public void GetStatementStatus_DiscoveredEvidenceContainsConflictingFact_ReturnsContradicted()
        {
            var statementFact = InvestigationTestFactory.CreateFact("fact-statement");
            var conflictingFact = InvestigationTestFactory.CreateFact("fact-conflict");
            InvestigationTestFactory.SetConflictingFacts(statementFact, conflictingFact);
            InvestigationTestFactory.SetConflictingFacts(conflictingFact, statementFact);

            var evidence = InvestigationTestFactory.CreateEvidence("ev-1", "Evidence", conflictingFact);
            var statement = InvestigationTestFactory.CreateStatement("st-1", "text", statementFact);
            var controller = new InvestigationController(InvestigationTestFactory.CreateCase("case-1", evidence));

            controller.HearStatement(statement);
            controller.DiscoverEvidence(evidence);

            Assert.AreEqual(StatementStatus.Contradicted, controller.GetStatementStatus(statement));
        }

        [Test]
        public void GetStatementStatus_ConflictDeclaredOnlyOnStatementFact_IsDetected()
        {
            var statementFact = InvestigationTestFactory.CreateFact("fact-statement");
            var evidenceFact = InvestigationTestFactory.CreateFact("fact-evidence");
            InvestigationTestFactory.SetConflictingFacts(statementFact, evidenceFact);
            // evidenceFact.ConflictingFacts intentionally left empty - conflict declared one-sided.

            var evidence = InvestigationTestFactory.CreateEvidence("ev-1", "Evidence", evidenceFact);
            var statement = InvestigationTestFactory.CreateStatement("st-1", "text", statementFact);
            var controller = new InvestigationController(InvestigationTestFactory.CreateCase("case-1", evidence));

            controller.HearStatement(statement);
            controller.DiscoverEvidence(evidence);

            Assert.AreEqual(StatementStatus.Contradicted, controller.GetStatementStatus(statement));
        }

        [Test]
        public void GetStatementStatus_ConflictDeclaredOnlyOnEvidenceFact_IsDetected()
        {
            var statementFact = InvestigationTestFactory.CreateFact("fact-statement");
            var evidenceFact = InvestigationTestFactory.CreateFact("fact-evidence");
            InvestigationTestFactory.SetConflictingFacts(evidenceFact, statementFact);
            // statementFact.ConflictingFacts intentionally left empty - conflict declared one-sided.

            var evidence = InvestigationTestFactory.CreateEvidence("ev-1", "Evidence", evidenceFact);
            var statement = InvestigationTestFactory.CreateStatement("st-1", "text", statementFact);
            var controller = new InvestigationController(InvestigationTestFactory.CreateCase("case-1", evidence));

            controller.HearStatement(statement);
            controller.DiscoverEvidence(evidence);

            Assert.AreEqual(StatementStatus.Contradicted, controller.GetStatementStatus(statement));
        }

        [Test]
        public void GetStatementStatus_SupportingAndConflictingEvidenceBothDiscovered_ReturnsContradicted()
        {
            var statementFact = InvestigationTestFactory.CreateFact("fact-statement");
            var conflictingFact = InvestigationTestFactory.CreateFact("fact-conflict");
            InvestigationTestFactory.SetConflictingFacts(statementFact, conflictingFact);

            var supportingEvidence = InvestigationTestFactory.CreateEvidence("ev-support", "Supporting", statementFact);
            var conflictingEvidence = InvestigationTestFactory.CreateEvidence("ev-conflict", "Conflicting", conflictingFact);
            var statement = InvestigationTestFactory.CreateStatement("st-1", "text", statementFact);
            var controller = new InvestigationController(
                InvestigationTestFactory.CreateCase("case-1", supportingEvidence, conflictingEvidence));

            controller.HearStatement(statement);
            controller.DiscoverEvidence(supportingEvidence);
            controller.DiscoverEvidence(conflictingEvidence);

            Assert.AreEqual(StatementStatus.Contradicted, controller.GetStatementStatus(statement));
        }

        [Test]
        public void GetStatementStatus_UndiscoveredEvidenceDoesNotAffectResult()
        {
            var statementFact = InvestigationTestFactory.CreateFact("fact-statement");
            var conflictingFact = InvestigationTestFactory.CreateFact("fact-conflict");
            InvestigationTestFactory.SetConflictingFacts(statementFact, conflictingFact);

            var conflictingEvidence = InvestigationTestFactory.CreateEvidence("ev-conflict", "Conflicting", conflictingFact);
            var statement = InvestigationTestFactory.CreateStatement("st-1", "text", statementFact);
            var controller = new InvestigationController(InvestigationTestFactory.CreateCase("case-1", conflictingEvidence));

            controller.HearStatement(statement);
            // DiscoverEvidence intentionally not called for conflictingEvidence.

            Assert.AreEqual(StatementStatus.Unknown, controller.GetStatementStatus(statement));
        }

        [Test]
        public void GetStatementStatus_MultipleEvidenceItems_EvaluatedCorrectly()
        {
            var statementFact = InvestigationTestFactory.CreateFact("fact-statement");
            var unrelatedFactA = InvestigationTestFactory.CreateFact("fact-unrelated-a");
            var unrelatedFactB = InvestigationTestFactory.CreateFact("fact-unrelated-b");

            var unrelatedEvidenceA = InvestigationTestFactory.CreateEvidence("ev-a", "A", unrelatedFactA);
            var unrelatedEvidenceB = InvestigationTestFactory.CreateEvidence("ev-b", "B", unrelatedFactB);
            var supportingEvidence = InvestigationTestFactory.CreateEvidence("ev-c", "C", statementFact);

            var statement = InvestigationTestFactory.CreateStatement("st-1", "text", statementFact);
            var controller = new InvestigationController(
                InvestigationTestFactory.CreateCase("case-1", unrelatedEvidenceA, unrelatedEvidenceB, supportingEvidence));

            controller.HearStatement(statement);
            controller.DiscoverEvidence(unrelatedEvidenceA);
            controller.DiscoverEvidence(unrelatedEvidenceB);
            controller.DiscoverEvidence(supportingEvidence);

            Assert.AreEqual(StatementStatus.Supported, controller.GetStatementStatus(statement));

            var discovered = controller.GetDiscoveredEvidence();
            Assert.AreEqual(3, discovered.Count);
            CollectionAssert.Contains(discovered, unrelatedEvidenceA);
            CollectionAssert.Contains(discovered, unrelatedEvidenceB);
            CollectionAssert.Contains(discovered, supportingEvidence);
        }

        [Test]
        public void EvaluatePresentation_NullStatement_ReturnsInvalid()
        {
            var fact = InvestigationTestFactory.CreateFact("fact-1");
            var evidence = InvestigationTestFactory.CreateEvidence("ev-1", "Evidence", fact);
            var controller = new InvestigationController(InvestigationTestFactory.CreateCase("case-1", evidence));

            controller.DiscoverEvidence(evidence);

            Assert.AreEqual(PresentationResult.Invalid, controller.EvaluatePresentation(null, evidence));
        }

        [Test]
        public void EvaluatePresentation_NullEvidence_ReturnsInvalid()
        {
            var fact = InvestigationTestFactory.CreateFact("fact-1");
            var statement = InvestigationTestFactory.CreateStatement("st-1", "text", fact);
            var controller = new InvestigationController(InvestigationTestFactory.CreateCase("case-1"));

            controller.HearStatement(statement);

            Assert.AreEqual(PresentationResult.Invalid, controller.EvaluatePresentation(statement, null));
        }

        [Test]
        public void EvaluatePresentation_StatementNotHeard_ReturnsInvalid()
        {
            var fact = InvestigationTestFactory.CreateFact("fact-1");
            var evidence = InvestigationTestFactory.CreateEvidence("ev-1", "Evidence", fact);
            var statement = InvestigationTestFactory.CreateStatement("st-1", "text", fact);
            var controller = new InvestigationController(InvestigationTestFactory.CreateCase("case-1", evidence));

            controller.DiscoverEvidence(evidence);
            // HearStatement intentionally not called.

            Assert.AreEqual(PresentationResult.Invalid, controller.EvaluatePresentation(statement, evidence));
        }

        [Test]
        public void EvaluatePresentation_EvidenceNotDiscovered_ReturnsInvalid()
        {
            var fact = InvestigationTestFactory.CreateFact("fact-1");
            var evidence = InvestigationTestFactory.CreateEvidence("ev-1", "Evidence", fact);
            var statement = InvestigationTestFactory.CreateStatement("st-1", "text", fact);
            var controller = new InvestigationController(InvestigationTestFactory.CreateCase("case-1", evidence));

            controller.HearStatement(statement);
            // DiscoverEvidence intentionally not called.

            Assert.AreEqual(PresentationResult.Invalid, controller.EvaluatePresentation(statement, evidence));
        }

        [Test]
        public void EvaluatePresentation_StatementWithoutFact_ReturnsInvalid()
        {
            var fact = InvestigationTestFactory.CreateFact("fact-1");
            var evidence = InvestigationTestFactory.CreateEvidence("ev-1", "Evidence", fact);
            var statement = InvestigationTestFactory.CreateStatement("st-1", "text", null);
            var controller = new InvestigationController(InvestigationTestFactory.CreateCase("case-1", evidence));

            controller.HearStatement(statement);
            controller.DiscoverEvidence(evidence);

            Assert.AreEqual(PresentationResult.Invalid, controller.EvaluatePresentation(statement, evidence));
        }

        [Test]
        public void EvaluatePresentation_MatchingFact_ReturnsSupports()
        {
            var fact = InvestigationTestFactory.CreateFact("fact-1");
            var evidence = InvestigationTestFactory.CreateEvidence("ev-1", "Evidence", fact);
            var statement = InvestigationTestFactory.CreateStatement("st-1", "text", fact);
            var controller = new InvestigationController(InvestigationTestFactory.CreateCase("case-1", evidence));

            controller.HearStatement(statement);
            controller.DiscoverEvidence(evidence);

            Assert.AreEqual(PresentationResult.Supports, controller.EvaluatePresentation(statement, evidence));
        }

        [Test]
        public void EvaluatePresentation_ConflictingFact_ReturnsContradicts()
        {
            var statementFact = InvestigationTestFactory.CreateFact("fact-statement");
            var conflictingFact = InvestigationTestFactory.CreateFact("fact-conflict");
            InvestigationTestFactory.SetConflictingFacts(statementFact, conflictingFact);
            InvestigationTestFactory.SetConflictingFacts(conflictingFact, statementFact);

            var evidence = InvestigationTestFactory.CreateEvidence("ev-1", "Evidence", conflictingFact);
            var statement = InvestigationTestFactory.CreateStatement("st-1", "text", statementFact);
            var controller = new InvestigationController(InvestigationTestFactory.CreateCase("case-1", evidence));

            controller.HearStatement(statement);
            controller.DiscoverEvidence(evidence);

            Assert.AreEqual(PresentationResult.Contradicts, controller.EvaluatePresentation(statement, evidence));
        }

        [Test]
        public void EvaluatePresentation_NeitherMatchingNorConflicting_ReturnsUnknown()
        {
            var statementFact = InvestigationTestFactory.CreateFact("fact-statement");
            var unrelatedFact = InvestigationTestFactory.CreateFact("fact-unrelated");
            var evidence = InvestigationTestFactory.CreateEvidence("ev-1", "Evidence", unrelatedFact);
            var statement = InvestigationTestFactory.CreateStatement("st-1", "text", statementFact);
            var controller = new InvestigationController(InvestigationTestFactory.CreateCase("case-1", evidence));

            controller.HearStatement(statement);
            controller.DiscoverEvidence(evidence);

            Assert.AreEqual(PresentationResult.Unknown, controller.EvaluatePresentation(statement, evidence));
        }

        [Test]
        public void EvaluatePresentation_ConflictDeclaredOnlyOnStatementFact_ReturnsContradicts()
        {
            var statementFact = InvestigationTestFactory.CreateFact("fact-statement");
            var evidenceFact = InvestigationTestFactory.CreateFact("fact-evidence");
            InvestigationTestFactory.SetConflictingFacts(statementFact, evidenceFact);
            // evidenceFact.ConflictingFacts intentionally left empty - conflict declared one-sided.

            var evidence = InvestigationTestFactory.CreateEvidence("ev-1", "Evidence", evidenceFact);
            var statement = InvestigationTestFactory.CreateStatement("st-1", "text", statementFact);
            var controller = new InvestigationController(InvestigationTestFactory.CreateCase("case-1", evidence));

            controller.HearStatement(statement);
            controller.DiscoverEvidence(evidence);

            Assert.AreEqual(PresentationResult.Contradicts, controller.EvaluatePresentation(statement, evidence));
        }

        [Test]
        public void EvaluatePresentation_ConflictDeclaredOnlyOnEvidenceFact_ReturnsContradicts()
        {
            var statementFact = InvestigationTestFactory.CreateFact("fact-statement");
            var evidenceFact = InvestigationTestFactory.CreateFact("fact-evidence");
            InvestigationTestFactory.SetConflictingFacts(evidenceFact, statementFact);
            // statementFact.ConflictingFacts intentionally left empty - conflict declared one-sided.

            var evidence = InvestigationTestFactory.CreateEvidence("ev-1", "Evidence", evidenceFact);
            var statement = InvestigationTestFactory.CreateStatement("st-1", "text", statementFact);
            var controller = new InvestigationController(InvestigationTestFactory.CreateCase("case-1", evidence));

            controller.HearStatement(statement);
            controller.DiscoverEvidence(evidence);

            Assert.AreEqual(PresentationResult.Contradicts, controller.EvaluatePresentation(statement, evidence));
        }

        [Test]
        public void EvaluatePresentation_SupportingAndConflictingFactsOnSameEvidence_ReturnsContradicts()
        {
            var statementFact = InvestigationTestFactory.CreateFact("fact-statement");
            var conflictingFact = InvestigationTestFactory.CreateFact("fact-conflict");
            InvestigationTestFactory.SetConflictingFacts(statementFact, conflictingFact);
            InvestigationTestFactory.SetConflictingFacts(conflictingFact, statementFact);

            var evidence = InvestigationTestFactory.CreateEvidence("ev-1", "Evidence", statementFact, conflictingFact);
            var statement = InvestigationTestFactory.CreateStatement("st-1", "text", statementFact);
            var controller = new InvestigationController(InvestigationTestFactory.CreateCase("case-1", evidence));

            controller.HearStatement(statement);
            controller.DiscoverEvidence(evidence);

            Assert.AreEqual(PresentationResult.Contradicts, controller.EvaluatePresentation(statement, evidence));
        }

        [Test]
        public void NewController_NoFactsEstablished()
        {
            var fact = InvestigationTestFactory.CreateFact("fact-1");
            var controller = new InvestigationController(InvestigationTestFactory.CreateCase("case-1"));

            Assert.IsFalse(controller.IsFactEstablished(fact));
        }

        [Test]
        public void EstablishFact_MarksFactAsEstablished()
        {
            var fact = InvestigationTestFactory.CreateFact("fact-1");
            var controller = new InvestigationController(InvestigationTestFactory.CreateCase("case-1"));

            controller.EstablishFact(fact);

            Assert.IsTrue(controller.IsFactEstablished(fact));
        }

        [Test]
        public void IsFactEstablished_NullFact_ReturnsFalse()
        {
            var controller = new InvestigationController(InvestigationTestFactory.CreateCase("case-1"));

            Assert.IsFalse(controller.IsFactEstablished(null));
        }

        [Test]
        public void EvaluatePresentation_DoesNotEstablishFacts()
        {
            var fact = InvestigationTestFactory.CreateFact("fact-1");
            var evidence = InvestigationTestFactory.CreateEvidence("ev-1", "Evidence", fact);
            var statement = InvestigationTestFactory.CreateStatement("st-1", "text", fact);
            var controller = new InvestigationController(InvestigationTestFactory.CreateCase("case-1", evidence));

            controller.HearStatement(statement);
            controller.DiscoverEvidence(evidence);

            var result = controller.EvaluatePresentation(statement, evidence);

            Assert.AreEqual(PresentationResult.Supports, result);
            Assert.IsFalse(controller.IsFactEstablished(fact));
        }

        [Test]
        public void PresentEvidence_Supports_EstablishesStatementFact()
        {
            var fact = InvestigationTestFactory.CreateFact("fact-1");
            var evidence = InvestigationTestFactory.CreateEvidence("ev-1", "Evidence", fact);
            var statement = InvestigationTestFactory.CreateStatement("st-1", "text", fact);
            var controller = new InvestigationController(InvestigationTestFactory.CreateCase("case-1", evidence));

            controller.HearStatement(statement);
            controller.DiscoverEvidence(evidence);

            var result = controller.PresentEvidence(statement, evidence);

            Assert.AreEqual(PresentationResult.Supports, result);
            Assert.IsTrue(controller.IsFactEstablished(fact));
        }

        [Test]
        public void PresentEvidence_Contradicts_EstablishesConflictingEvidenceFactNotStatementFact()
        {
            var statementFact = InvestigationTestFactory.CreateFact("fact-statement");
            var conflictingFact = InvestigationTestFactory.CreateFact("fact-conflict");
            InvestigationTestFactory.SetConflictingFacts(statementFact, conflictingFact);
            InvestigationTestFactory.SetConflictingFacts(conflictingFact, statementFact);

            var evidence = InvestigationTestFactory.CreateEvidence("ev-1", "Evidence", conflictingFact);
            var statement = InvestigationTestFactory.CreateStatement("st-1", "text", statementFact);
            var controller = new InvestigationController(InvestigationTestFactory.CreateCase("case-1", evidence));

            controller.HearStatement(statement);
            controller.DiscoverEvidence(evidence);

            var result = controller.PresentEvidence(statement, evidence);

            Assert.AreEqual(PresentationResult.Contradicts, result);
            Assert.IsTrue(controller.IsFactEstablished(conflictingFact));
            Assert.IsFalse(controller.IsFactEstablished(statementFact));
        }

        [Test]
        public void PresentEvidence_Unknown_EstablishesNothing()
        {
            var statementFact = InvestigationTestFactory.CreateFact("fact-statement");
            var unrelatedFact = InvestigationTestFactory.CreateFact("fact-unrelated");
            var evidence = InvestigationTestFactory.CreateEvidence("ev-1", "Evidence", unrelatedFact);
            var statement = InvestigationTestFactory.CreateStatement("st-1", "text", statementFact);
            var controller = new InvestigationController(InvestigationTestFactory.CreateCase("case-1", evidence));

            controller.HearStatement(statement);
            controller.DiscoverEvidence(evidence);

            var result = controller.PresentEvidence(statement, evidence);

            Assert.AreEqual(PresentationResult.Unknown, result);
            Assert.IsFalse(controller.IsFactEstablished(statementFact));
            Assert.IsFalse(controller.IsFactEstablished(unrelatedFact));
        }

        [Test]
        public void PresentEvidence_Invalid_EstablishesNothing()
        {
            var fact = InvestigationTestFactory.CreateFact("fact-1");
            var evidence = InvestigationTestFactory.CreateEvidence("ev-1", "Evidence", fact);
            var statement = InvestigationTestFactory.CreateStatement("st-1", "text", fact);
            var controller = new InvestigationController(InvestigationTestFactory.CreateCase("case-1", evidence));

            // Neither HearStatement nor DiscoverEvidence called - result is Invalid.

            var result = controller.PresentEvidence(statement, evidence);

            Assert.AreEqual(PresentationResult.Invalid, result);
            Assert.IsFalse(controller.IsFactEstablished(fact));
        }

        [Test]
        public void PresentEvidence_MultipleConflictingEvidenceFacts_AllEstablished()
        {
            var statementFact = InvestigationTestFactory.CreateFact("fact-statement");
            var conflictA = InvestigationTestFactory.CreateFact("fact-conflict-a");
            var conflictB = InvestigationTestFactory.CreateFact("fact-conflict-b");
            InvestigationTestFactory.SetConflictingFacts(statementFact, conflictA, conflictB);
            InvestigationTestFactory.SetConflictingFacts(conflictA, statementFact);
            InvestigationTestFactory.SetConflictingFacts(conflictB, statementFact);

            var evidence = InvestigationTestFactory.CreateEvidence("ev-1", "Evidence", conflictA, conflictB);
            var statement = InvestigationTestFactory.CreateStatement("st-1", "text", statementFact);
            var controller = new InvestigationController(InvestigationTestFactory.CreateCase("case-1", evidence));

            controller.HearStatement(statement);
            controller.DiscoverEvidence(evidence);

            var result = controller.PresentEvidence(statement, evidence);

            Assert.AreEqual(PresentationResult.Contradicts, result);
            Assert.IsTrue(controller.IsFactEstablished(conflictA));
            Assert.IsTrue(controller.IsFactEstablished(conflictB));
            Assert.IsFalse(controller.IsFactEstablished(statementFact));
        }

        [Test]
        public void EvaluateAccusation_NullSuspect_ReturnsInvalid()
        {
            var correctSuspect = InvestigationTestFactory.CreateSuspect("sus-correct", "Correct");
            var solution = InvestigationTestFactory.CreateCaseSolution(
                correctSuspect, new EvidenceDefinition[0], new FactDefinition[0]);
            var caseDefinition = InvestigationTestFactory.CreateCase("case-1");
            InvestigationTestFactory.AttachSolution(caseDefinition, solution);
            var controller = new InvestigationController(caseDefinition);

            Assert.AreEqual(AccusationResult.Invalid, controller.EvaluateAccusation(null));
        }

        [Test]
        public void EvaluateAccusation_NoSolution_ReturnsInvalid()
        {
            var suspect = InvestigationTestFactory.CreateSuspect("sus-1", "Suspect");
            var caseDefinition = InvestigationTestFactory.CreateCase("case-1");
            // AttachSolution intentionally not called - the case has no solution.
            var controller = new InvestigationController(caseDefinition);

            Assert.AreEqual(AccusationResult.Invalid, controller.EvaluateAccusation(suspect));
        }

        [Test]
        public void EvaluateAccusation_CorrectSuspectWithNoRequirementsMet_ReturnsInvalid()
        {
            var correctSuspect = InvestigationTestFactory.CreateSuspect("sus-correct", "Correct");
            var evidence = InvestigationTestFactory.CreateEvidence("ev-1", "Evidence");
            var fact = InvestigationTestFactory.CreateFact("fact-1");
            var solution = InvestigationTestFactory.CreateCaseSolution(correctSuspect, new[] { evidence }, new[] { fact });
            var caseDefinition = InvestigationTestFactory.CreateCase("case-1", evidence);
            InvestigationTestFactory.AttachSolution(caseDefinition, solution);
            var controller = new InvestigationController(caseDefinition);

            // Nothing discovered or established yet.

            Assert.AreEqual(AccusationResult.Invalid, controller.EvaluateAccusation(correctSuspect));
        }

        [Test]
        public void EvaluateAccusation_CorrectSuspectWithOnlySomeRequiredEvidence_ReturnsInvalid()
        {
            var correctSuspect = InvestigationTestFactory.CreateSuspect("sus-correct", "Correct");
            var evidenceA = InvestigationTestFactory.CreateEvidence("ev-a", "A");
            var evidenceB = InvestigationTestFactory.CreateEvidence("ev-b", "B");
            var solution = InvestigationTestFactory.CreateCaseSolution(
                correctSuspect, new[] { evidenceA, evidenceB }, new FactDefinition[0]);
            var caseDefinition = InvestigationTestFactory.CreateCase("case-1", evidenceA, evidenceB);
            InvestigationTestFactory.AttachSolution(caseDefinition, solution);
            var controller = new InvestigationController(caseDefinition);

            controller.DiscoverEvidence(evidenceA);
            // evidenceB intentionally not discovered.

            Assert.AreEqual(AccusationResult.Invalid, controller.EvaluateAccusation(correctSuspect));
        }

        [Test]
        public void EvaluateAccusation_CorrectSuspectWithAllEvidenceButMissingFact_ReturnsInvalid()
        {
            var correctSuspect = InvestigationTestFactory.CreateSuspect("sus-correct", "Correct");
            var evidence = InvestigationTestFactory.CreateEvidence("ev-1", "Evidence");
            var fact = InvestigationTestFactory.CreateFact("fact-1");
            var solution = InvestigationTestFactory.CreateCaseSolution(correctSuspect, new[] { evidence }, new[] { fact });
            var caseDefinition = InvestigationTestFactory.CreateCase("case-1", evidence);
            InvestigationTestFactory.AttachSolution(caseDefinition, solution);
            var controller = new InvestigationController(caseDefinition);

            controller.DiscoverEvidence(evidence);
            // fact intentionally not established.

            Assert.AreEqual(AccusationResult.Invalid, controller.EvaluateAccusation(correctSuspect));
        }

        [Test]
        public void EvaluateAccusation_WrongSuspectWithRequirementsSatisfied_ReturnsIncorrect()
        {
            var correctSuspect = InvestigationTestFactory.CreateSuspect("sus-correct", "Correct");
            var wrongSuspect = InvestigationTestFactory.CreateSuspect("sus-wrong", "Wrong");
            var evidence = InvestigationTestFactory.CreateEvidence("ev-1", "Evidence");
            var fact = InvestigationTestFactory.CreateFact("fact-1");
            var solution = InvestigationTestFactory.CreateCaseSolution(correctSuspect, new[] { evidence }, new[] { fact });
            var caseDefinition = InvestigationTestFactory.CreateCase("case-1", evidence);
            InvestigationTestFactory.AttachSolution(caseDefinition, solution);
            var controller = new InvestigationController(caseDefinition);

            controller.DiscoverEvidence(evidence);
            controller.EstablishFact(fact);

            Assert.AreEqual(AccusationResult.Incorrect, controller.EvaluateAccusation(wrongSuspect));
        }

        [Test]
        public void EvaluateAccusation_CorrectSuspectWithAllRequirementsSatisfied_ReturnsCorrect()
        {
            var correctSuspect = InvestigationTestFactory.CreateSuspect("sus-correct", "Correct");
            var evidence = InvestigationTestFactory.CreateEvidence("ev-1", "Evidence");
            var fact = InvestigationTestFactory.CreateFact("fact-1");
            var solution = InvestigationTestFactory.CreateCaseSolution(correctSuspect, new[] { evidence }, new[] { fact });
            var caseDefinition = InvestigationTestFactory.CreateCase("case-1", evidence);
            InvestigationTestFactory.AttachSolution(caseDefinition, solution);
            var controller = new InvestigationController(caseDefinition);

            controller.DiscoverEvidence(evidence);
            controller.EstablishFact(fact);

            Assert.AreEqual(AccusationResult.Correct, controller.EvaluateAccusation(correctSuspect));
        }

        [Test]
        public void EvaluateAccusation_DoesNotMutateInvestigationState()
        {
            var correctSuspect = InvestigationTestFactory.CreateSuspect("sus-correct", "Correct");
            var evidence = InvestigationTestFactory.CreateEvidence("ev-1", "Evidence");
            var fact = InvestigationTestFactory.CreateFact("fact-1");
            var solution = InvestigationTestFactory.CreateCaseSolution(correctSuspect, new[] { evidence }, new[] { fact });
            var caseDefinition = InvestigationTestFactory.CreateCase("case-1", evidence);
            InvestigationTestFactory.AttachSolution(caseDefinition, solution);
            var controller = new InvestigationController(caseDefinition);

            controller.EvaluateAccusation(correctSuspect);

            Assert.IsFalse(controller.IsEvidenceDiscovered(evidence));
            Assert.IsFalse(controller.IsFactEstablished(fact));
            Assert.AreEqual(0, controller.GetDiscoveredEvidence().Count);

            // Repeated evaluation is still side-effect free.
            Assert.AreEqual(AccusationResult.Invalid, controller.EvaluateAccusation(correctSuspect));
            Assert.IsFalse(controller.IsEvidenceDiscovered(evidence));
            Assert.IsFalse(controller.IsFactEstablished(fact));
        }

        [Test]
        public void EvaluateAccusation_SuspectComparisonUsesReferenceNotIdOrName_DifferentInstanceIsIncorrect()
        {
            var correctSuspect = InvestigationTestFactory.CreateSuspect("sus-1", "Same Name");
            var lookalikeSuspect = InvestigationTestFactory.CreateSuspect("sus-1", "Same Name");
            var evidence = InvestigationTestFactory.CreateEvidence("ev-1", "Evidence");
            var fact = InvestigationTestFactory.CreateFact("fact-1");
            var solution = InvestigationTestFactory.CreateCaseSolution(correctSuspect, new[] { evidence }, new[] { fact });
            var caseDefinition = InvestigationTestFactory.CreateCase("case-1", evidence);
            InvestigationTestFactory.AttachSolution(caseDefinition, solution);
            var controller = new InvestigationController(caseDefinition);

            controller.DiscoverEvidence(evidence);
            controller.EstablishFact(fact);

            Assert.AreEqual(AccusationResult.Incorrect, controller.EvaluateAccusation(lookalikeSuspect));
            Assert.AreEqual(AccusationResult.Correct, controller.EvaluateAccusation(correctSuspect));
        }

        [Test]
        public void CanAccuse_NoActiveCase_ReturnsFalse()
        {
            // A bare, unconfigured case - no solution, no evidence - stands in for "no active
            // case" since InvestigationController requires a non-null CaseDefinition.
            var bareCase = ScriptableObject.CreateInstance<CaseDefinition>();
            var controller = new InvestigationController(bareCase);

            Assert.IsFalse(controller.CanAccuse());
        }

        [Test]
        public void CanAccuse_ActiveCaseWithNoSolution_ReturnsFalse()
        {
            var caseDefinition = InvestigationTestFactory.CreateCase("case-1");
            // AttachSolution intentionally not called - the case has no solution.
            var controller = new InvestigationController(caseDefinition);

            Assert.IsFalse(controller.CanAccuse());
        }

        [Test]
        public void CanAccuse_MissingRequiredEvidence_ReturnsFalse()
        {
            var correctSuspect = InvestigationTestFactory.CreateSuspect("sus-correct", "Correct");
            var evidenceA = InvestigationTestFactory.CreateEvidence("ev-a", "A");
            var evidenceB = InvestigationTestFactory.CreateEvidence("ev-b", "B");
            var fact = InvestigationTestFactory.CreateFact("fact-1");
            var solution = InvestigationTestFactory.CreateCaseSolution(
                correctSuspect, new[] { evidenceA, evidenceB }, new[] { fact });
            var caseDefinition = InvestigationTestFactory.CreateCase("case-1", evidenceA, evidenceB);
            InvestigationTestFactory.AttachSolution(caseDefinition, solution);
            var controller = new InvestigationController(caseDefinition);

            controller.DiscoverEvidence(evidenceA);
            // evidenceB intentionally not discovered.
            controller.EstablishFact(fact);

            Assert.IsFalse(controller.CanAccuse());
        }

        [Test]
        public void CanAccuse_MissingRequiredFact_ReturnsFalse()
        {
            var correctSuspect = InvestigationTestFactory.CreateSuspect("sus-correct", "Correct");
            var evidence = InvestigationTestFactory.CreateEvidence("ev-1", "Evidence");
            var fact = InvestigationTestFactory.CreateFact("fact-1");
            var solution = InvestigationTestFactory.CreateCaseSolution(correctSuspect, new[] { evidence }, new[] { fact });
            var caseDefinition = InvestigationTestFactory.CreateCase("case-1", evidence);
            InvestigationTestFactory.AttachSolution(caseDefinition, solution);
            var controller = new InvestigationController(caseDefinition);

            controller.DiscoverEvidence(evidence);
            // fact intentionally not established.

            Assert.IsFalse(controller.CanAccuse());
        }

        [Test]
        public void CanAccuse_AllRequiredEvidenceAndFactsSatisfied_ReturnsTrue()
        {
            var correctSuspect = InvestigationTestFactory.CreateSuspect("sus-correct", "Correct");
            var evidence = InvestigationTestFactory.CreateEvidence("ev-1", "Evidence");
            var fact = InvestigationTestFactory.CreateFact("fact-1");
            var solution = InvestigationTestFactory.CreateCaseSolution(correctSuspect, new[] { evidence }, new[] { fact });
            var caseDefinition = InvestigationTestFactory.CreateCase("case-1", evidence);
            InvestigationTestFactory.AttachSolution(caseDefinition, solution);
            var controller = new InvestigationController(caseDefinition);

            controller.DiscoverEvidence(evidence);
            controller.EstablishFact(fact);

            Assert.IsTrue(controller.CanAccuse());
        }

        [Test]
        public void CanAccuse_DoesNotMutateInvestigationState()
        {
            var correctSuspect = InvestigationTestFactory.CreateSuspect("sus-correct", "Correct");
            var evidence = InvestigationTestFactory.CreateEvidence("ev-1", "Evidence");
            var fact = InvestigationTestFactory.CreateFact("fact-1");
            var solution = InvestigationTestFactory.CreateCaseSolution(correctSuspect, new[] { evidence }, new[] { fact });
            var caseDefinition = InvestigationTestFactory.CreateCase("case-1", evidence);
            InvestigationTestFactory.AttachSolution(caseDefinition, solution);
            var controller = new InvestigationController(caseDefinition);

            controller.CanAccuse();

            Assert.IsFalse(controller.IsEvidenceDiscovered(evidence));
            Assert.IsFalse(controller.IsFactEstablished(fact));
            Assert.AreEqual(0, controller.GetDiscoveredEvidence().Count);

            // Repeated evaluation is still side-effect free.
            Assert.IsFalse(controller.CanAccuse());
            Assert.IsFalse(controller.IsEvidenceDiscovered(evidence));
            Assert.IsFalse(controller.IsFactEstablished(fact));
        }
    }
}
