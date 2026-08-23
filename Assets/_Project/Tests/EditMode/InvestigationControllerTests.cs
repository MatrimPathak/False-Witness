using FalseWitness.Investigation;
using NUnit.Framework;

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
    }
}
