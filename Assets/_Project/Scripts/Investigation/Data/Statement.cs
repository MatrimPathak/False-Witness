using System;
using UnityEngine;

namespace FalseWitness.Investigation
{
    [Serializable]
    public class Statement
    {
        [SerializeField] private string statementId;
        [SerializeField, TextArea] private string text;
        [SerializeField] private FactDefinition associatedFact;

        public string StatementId => statementId;
        public string Text => text;
        public FactDefinition AssociatedFact => associatedFact;
    }
}
