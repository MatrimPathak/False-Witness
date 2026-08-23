using FalseWitness.Investigation;
using NUnit.Framework;
using UnityEngine;

namespace FalseWitness.Investigation.Tests
{
    public class CaseSolutionTests
    {
        [Test]
        public void CaseSolution_ExposesConfiguredCorrectSuspect()
        {
            var suspect = InvestigationTestFactory.CreateSuspect("sus-1", "Suspect One");
            var solution = InvestigationTestFactory.CreateCaseSolution(suspect, new EvidenceDefinition[0], new FactDefinition[0]);

            Assert.AreSame(suspect, solution.CorrectSuspect);
        }

        [Test]
        public void CaseSolution_ExposesConfiguredRequiredEvidence()
        {
            var suspect = InvestigationTestFactory.CreateSuspect("sus-1", "Suspect One");
            var evidenceA = InvestigationTestFactory.CreateEvidence("ev-a", "Evidence A");
            var evidenceB = InvestigationTestFactory.CreateEvidence("ev-b", "Evidence B");
            var solution = InvestigationTestFactory.CreateCaseSolution(suspect, new[] { evidenceA, evidenceB }, new FactDefinition[0]);

            Assert.AreEqual(2, solution.RequiredEvidence.Count);
            CollectionAssert.Contains((System.Collections.ICollection)solution.RequiredEvidence, evidenceA);
            CollectionAssert.Contains((System.Collections.ICollection)solution.RequiredEvidence, evidenceB);
        }

        [Test]
        public void CaseSolution_ExposesConfiguredRequiredFacts()
        {
            var suspect = InvestigationTestFactory.CreateSuspect("sus-1", "Suspect One");
            var factA = InvestigationTestFactory.CreateFact("fact-a");
            var factB = InvestigationTestFactory.CreateFact("fact-b");
            var solution = InvestigationTestFactory.CreateCaseSolution(suspect, new EvidenceDefinition[0], new[] { factA, factB });

            Assert.AreEqual(2, solution.RequiredFacts.Count);
            CollectionAssert.Contains((System.Collections.ICollection)solution.RequiredFacts, factA);
            CollectionAssert.Contains((System.Collections.ICollection)solution.RequiredFacts, factB);
        }

        [Test]
        public void CaseSolution_UnconfiguredInstance_HasNoSuspectAndEmptyLists()
        {
            var solution = ScriptableObject.CreateInstance<CaseSolution>();

            Assert.IsNull(solution.CorrectSuspect);
            Assert.AreEqual(0, solution.RequiredEvidence.Count);
            Assert.AreEqual(0, solution.RequiredFacts.Count);
        }

        [Test]
        public void CaseDefinition_ExposesConfiguredSolution()
        {
            var evidence = InvestigationTestFactory.CreateEvidence("ev-1", "Evidence");
            var caseDefinition = InvestigationTestFactory.CreateCase("case-1", evidence);
            var suspect = InvestigationTestFactory.CreateSuspect("sus-1", "Suspect One");
            var solution = InvestigationTestFactory.CreateCaseSolution(suspect, new[] { evidence }, new FactDefinition[0]);

            InvestigationTestFactory.AttachSolution(caseDefinition, solution);

            Assert.AreSame(solution, caseDefinition.Solution);
        }

        [Test]
        public void CaseDefinition_SolutionDefaultsToNull()
        {
            var caseDefinition = InvestigationTestFactory.CreateCase("case-1");

            Assert.IsNull(caseDefinition.Solution);
        }
    }
}
