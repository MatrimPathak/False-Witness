using System.Collections.Generic;
using UnityEngine;

namespace FalseWitness.Investigation
{
    [CreateAssetMenu(fileName = "Suspect", menuName = "False Witness/Investigation/Suspect")]
    public class SuspectDefinition : ScriptableObject
    {
        [SerializeField] private string suspectId;
        [SerializeField] private string suspectName;
        [SerializeField, TextArea] private string description;
        [SerializeField] private Sprite portrait;
        [SerializeField] private List<Statement> statements = new();

        public string SuspectId => suspectId;
        public string SuspectName => suspectName;
        public string Description => description;
        public Sprite Portrait => portrait;
        public IReadOnlyList<Statement> Statements => statements;
    }
}
