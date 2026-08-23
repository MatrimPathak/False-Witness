using System;
using System.Collections.Generic;
using System.Reflection;
using FalseWitness.Investigation;
using UnityEngine;

namespace FalseWitness.Investigation.Tests
{
    /// <summary>
    /// Builds Investigation data objects for tests. The authoring types expose only
    /// getters (they're meant to be populated via the Unity Inspector), so fixtures
    /// are wired up here via reflection onto their private serialized fields instead
    /// of adding test-only constructors/setters to production code.
    /// </summary>
    internal static class InvestigationTestFactory
    {
        public static FactDefinition CreateFact(string factId, string description = "")
        {
            var fact = ScriptableObject.CreateInstance<FactDefinition>();
            SetField(fact, "factId", factId);
            SetField(fact, "description", description);
            return fact;
        }

        public static void SetConflictingFacts(FactDefinition fact, params FactDefinition[] conflicts)
        {
            SetField(fact, "conflictingFacts", new List<FactDefinition>(conflicts));
        }

        public static EvidenceDefinition CreateEvidence(string evidenceId, string evidenceName, params FactDefinition[] facts)
        {
            var evidence = ScriptableObject.CreateInstance<EvidenceDefinition>();
            SetField(evidence, "evidenceId", evidenceId);
            SetField(evidence, "evidenceName", evidenceName);
            SetField(evidence, "associatedFacts", new List<FactDefinition>(facts));
            return evidence;
        }

        public static Statement CreateStatement(string statementId, string text, FactDefinition associatedFact)
        {
            var statement = new Statement();
            SetField(statement, "statementId", statementId);
            SetField(statement, "text", text);
            SetField(statement, "associatedFact", associatedFact);
            return statement;
        }

        public static CaseDefinition CreateCase(string caseId, params EvidenceDefinition[] evidence)
        {
            var caseDefinition = ScriptableObject.CreateInstance<CaseDefinition>();
            SetField(caseDefinition, "caseId", caseId);
            SetField(caseDefinition, "suspects", new List<SuspectDefinition>());
            SetField(caseDefinition, "evidence", new List<EvidenceDefinition>(evidence));
            return caseDefinition;
        }

        public static SuspectDefinition CreateSuspect(string suspectId, string suspectName, params Statement[] statements)
        {
            var suspect = ScriptableObject.CreateInstance<SuspectDefinition>();
            SetField(suspect, "suspectId", suspectId);
            SetField(suspect, "suspectName", suspectName);
            SetField(suspect, "statements", new List<Statement>(statements));
            return suspect;
        }

        public static CaseSolution CreateCaseSolution(SuspectDefinition correctSuspect, EvidenceDefinition[] requiredEvidence, FactDefinition[] requiredFacts)
        {
            var solution = ScriptableObject.CreateInstance<CaseSolution>();
            SetField(solution, "correctSuspect", correctSuspect);
            SetField(solution, "requiredEvidence", new List<EvidenceDefinition>(requiredEvidence));
            SetField(solution, "requiredFacts", new List<FactDefinition>(requiredFacts));
            return solution;
        }

        public static void AttachSolution(CaseDefinition caseDefinition, CaseSolution solution)
        {
            SetField(caseDefinition, "solution", solution);
        }

        private static void SetField(object target, string fieldName, object value)
        {
            var field = target.GetType().GetField(fieldName, BindingFlags.NonPublic | BindingFlags.Instance);
            if (field == null)
            {
                throw new MissingFieldException(target.GetType().Name, fieldName);
            }

            field.SetValue(target, value);
        }
    }
}
