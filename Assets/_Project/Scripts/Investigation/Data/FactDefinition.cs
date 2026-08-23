using UnityEngine;

namespace FalseWitness.Investigation
{
    [CreateAssetMenu(fileName = "Fact", menuName = "False Witness/Investigation/Fact")]
    public class FactDefinition : ScriptableObject
    {
        [SerializeField] private string factId;
        [SerializeField, TextArea] private string description;

        public string FactId => factId;
        public string Description => description;
    }
}
