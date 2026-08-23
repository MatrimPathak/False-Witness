using FalseWitness.Investigation;
using NUnit.Framework;

namespace FalseWitness.Investigation.Tests
{
    public class InvestigationRestartTests
    {
        [Test]
        public void Restart_FreshInvestigation_HasNoDiscoveredEvidence()
        {
            var evidence = InvestigationTestFactory.CreateEvidence("ev-1", "Evidence");
            var caseDefinition = InvestigationTestFactory.CreateCase("case-1", evidence);
            var investigation1 = new InvestigationController(caseDefinition);
            investigation1.DiscoverEvidence(evidence);

            var investigation2 = investigation1.Restart();

            Assert.IsFalse(investigation2.IsEvidenceDiscovered(evidence));
            Assert.AreEqual(0, investigation2.GetDiscoveredEvidence().Count);
        }

        [Test]
        public void Restart_FreshInvestigation_HasNoHeardStatements()
        {
            var fact = InvestigationTestFactory.CreateFact("fact-1");
            var statement = InvestigationTestFactory.CreateStatement("st-1", "text", fact);
            var caseDefinition = InvestigationTestFactory.CreateCase("case-1");
            var investigation1 = new InvestigationController(caseDefinition);
            investigation1.HearStatement(statement);

            var investigation2 = investigation1.Restart();

            Assert.IsFalse(investigation2.HasHeardStatement(statement));
        }

        [Test]
        public void Restart_FreshInvestigation_HasNoEstablishedFacts()
        {
            var fact = InvestigationTestFactory.CreateFact("fact-1");
            var caseDefinition = InvestigationTestFactory.CreateCase("case-1");
            var investigation1 = new InvestigationController(caseDefinition);
            investigation1.EstablishFact(fact);

            var investigation2 = investigation1.Restart();

            Assert.IsFalse(investigation2.IsFactEstablished(fact));
        }

        [Test]
        public void Restart_AfterEvidenceAndFactsDiscoveredInInvestigation1_Investigation2StartsCompletelyClean()
        {
            var evidence = InvestigationTestFactory.CreateEvidence("ev-1", "Evidence");
            var fact = InvestigationTestFactory.CreateFact("fact-1");
            var statement = InvestigationTestFactory.CreateStatement("st-1", "text", fact);
            var caseDefinition = InvestigationTestFactory.CreateCase("case-1", evidence);
            var investigation1 = new InvestigationController(caseDefinition);

            investigation1.HearStatement(statement);
            investigation1.DiscoverEvidence(evidence);
            investigation1.EstablishFact(fact);

            var investigation2 = investigation1.Restart();

            Assert.IsFalse(investigation2.HasHeardStatement(statement));
            Assert.IsFalse(investigation2.IsEvidenceDiscovered(evidence));
            Assert.IsFalse(investigation2.IsFactEstablished(fact));
            Assert.AreEqual(0, investigation2.GetDiscoveredEvidence().Count);
        }

        [Test]
        public void Restart_Investigation1RemainsUnchangedAfterInvestigation2Created()
        {
            var evidence = InvestigationTestFactory.CreateEvidence("ev-1", "Evidence");
            var fact = InvestigationTestFactory.CreateFact("fact-1");
            var statement = InvestigationTestFactory.CreateStatement("st-1", "text", fact);
            var caseDefinition = InvestigationTestFactory.CreateCase("case-1", evidence);
            var investigation1 = new InvestigationController(caseDefinition);

            investigation1.HearStatement(statement);
            investigation1.DiscoverEvidence(evidence);
            investigation1.EstablishFact(fact);

            investigation1.Restart();

            Assert.IsTrue(investigation1.HasHeardStatement(statement));
            Assert.IsTrue(investigation1.IsEvidenceDiscovered(evidence));
            Assert.IsTrue(investigation1.IsFactEstablished(fact));
            Assert.AreEqual(1, investigation1.GetDiscoveredEvidence().Count);
        }

        [Test]
        public void Restart_NewCaseResolutionController_StartsInProgress()
        {
            var caseDefinition = InvestigationTestFactory.CreateCase("case-1");
            var investigation1 = new InvestigationController(caseDefinition);

            var investigation2 = investigation1.Restart();
            var resolution2 = new CaseResolutionController(investigation2);

            Assert.AreEqual(CaseResolutionState.InProgress, resolution2.ResolutionState);
        }

        [Test]
        public void Restart_AfterSolved_FollowedByCompletelyFreshInvestigation()
        {
            var correctSuspect = InvestigationTestFactory.CreateSuspect("sus-correct", "Correct");
            var evidence = InvestigationTestFactory.CreateEvidence("ev-1", "Evidence");
            var fact = InvestigationTestFactory.CreateFact("fact-1");
            var solution = InvestigationTestFactory.CreateCaseSolution(correctSuspect, new[] { evidence }, new[] { fact });
            var caseDefinition = InvestigationTestFactory.CreateCase("case-1", evidence);
            InvestigationTestFactory.AttachSolution(caseDefinition, solution);

            var investigation1 = new InvestigationController(caseDefinition);
            investigation1.DiscoverEvidence(evidence);
            investigation1.EstablishFact(fact);
            var resolution1 = new CaseResolutionController(investigation1);
            Assert.AreEqual(CaseResolutionResult.Solved, resolution1.ResolveAccusation(correctSuspect));

            var investigation2 = investigation1.Restart();
            var resolution2 = new CaseResolutionController(investigation2);

            Assert.IsFalse(investigation2.IsEvidenceDiscovered(evidence));
            Assert.IsFalse(investigation2.IsFactEstablished(fact));
            Assert.AreEqual(CaseResolutionState.InProgress, resolution2.ResolutionState);
            Assert.AreEqual(CaseResolutionResult.Invalid, resolution2.ResolveAccusation(correctSuspect));
        }

        [Test]
        public void Restart_AfterFailed_FollowedByCompletelyFreshInvestigation()
        {
            var correctSuspect = InvestigationTestFactory.CreateSuspect("sus-correct", "Correct");
            var wrongSuspect = InvestigationTestFactory.CreateSuspect("sus-wrong", "Wrong");
            var evidence = InvestigationTestFactory.CreateEvidence("ev-1", "Evidence");
            var fact = InvestigationTestFactory.CreateFact("fact-1");
            var solution = InvestigationTestFactory.CreateCaseSolution(correctSuspect, new[] { evidence }, new[] { fact });
            var caseDefinition = InvestigationTestFactory.CreateCase("case-1", evidence);
            InvestigationTestFactory.AttachSolution(caseDefinition, solution);

            var investigation1 = new InvestigationController(caseDefinition);
            investigation1.DiscoverEvidence(evidence);
            investigation1.EstablishFact(fact);
            var resolution1 = new CaseResolutionController(investigation1);
            Assert.AreEqual(CaseResolutionResult.Failed, resolution1.ResolveAccusation(wrongSuspect));

            var investigation2 = investigation1.Restart();
            var resolution2 = new CaseResolutionController(investigation2);

            Assert.IsFalse(investigation2.IsEvidenceDiscovered(evidence));
            Assert.IsFalse(investigation2.IsFactEstablished(fact));
            Assert.AreEqual(CaseResolutionState.InProgress, resolution2.ResolutionState);
            Assert.AreEqual(CaseResolutionResult.Invalid, resolution2.ResolveAccusation(wrongSuspect));
        }

        [Test]
        public void Restart_CaseDefinitionReferenceRemainsSameBetweenRestarts()
        {
            var caseDefinition = InvestigationTestFactory.CreateCase("case-1");
            var investigation1 = new InvestigationController(caseDefinition);

            var investigation2 = investigation1.Restart();

            Assert.AreSame(caseDefinition, investigation1.ActiveCase);
            Assert.AreSame(caseDefinition, investigation2.ActiveCase);
            Assert.AreSame(investigation1.ActiveCase, investigation2.ActiveCase);
        }

        [Test]
        public void Restart_DoesNotMutateCaseDefinitionAuthoringData()
        {
            var evidenceA = InvestigationTestFactory.CreateEvidence("ev-a", "A");
            var evidenceB = InvestigationTestFactory.CreateEvidence("ev-b", "B");
            var caseDefinition = InvestigationTestFactory.CreateCase("case-1", evidenceA, evidenceB);
            var investigation1 = new InvestigationController(caseDefinition);
            investigation1.DiscoverEvidence(evidenceA);

            investigation1.Restart();

            Assert.AreEqual(2, caseDefinition.Evidence.Count);
            CollectionAssert.Contains((System.Collections.ICollection)caseDefinition.Evidence, evidenceA);
            CollectionAssert.Contains((System.Collections.ICollection)caseDefinition.Evidence, evidenceB);
        }
    }
}
