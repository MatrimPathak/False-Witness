using System.Collections.Generic;
using UnityEngine;

namespace FalseWitness.Investigation
{
    [CreateAssetMenu(fileName = "Fact", menuName = "False Witness/Investigation/Fact")]
    public class FactDefinition : ScriptableObject
    {
        [SerializeField] private string factId;
        [SerializeField, TextArea] private string description;
        [SerializeField] private List<FactDefinition> conflictingFacts = new();

        public string FactId => factId;
        public string Description => description;

        /// <summary>
        /// Facts that cannot be true at the same time as this one. Authors only need to
        /// declare a conflict on one side of the pair; lookups treat it as symmetric.
        /// </summary>
        public IReadOnlyList<FactDefinition> ConflictingFacts => conflictingFacts;
    }
}
