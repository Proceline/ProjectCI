using ProjectCI.CoreSystem.Runtime.Services;
using ProjectCI.CoreSystem.Runtime.Attributes;
using UnityEngine;

namespace ProjectCI.TacticTool.Formula.Concrete
{
    [CreateAssetMenu(fileName = "FormulaCollection", menuName = "ProjectCI/Attributes/Create FormulaCollection")]
    public class FormulaCollection : ScriptableObject, IService
    {
        [SerializeField]
        private AttributeType m_HealthAttributeType;

        [SerializeField]
        private AttributeType m_MovementAttributeType;
        [SerializeField]
        private FormulaDefinition[] m_Formulas;

        public FormulaDefinition[] Formulas => m_Formulas;

        public AttributeType HealthAttributeType => m_HealthAttributeType;
        public AttributeType MovementAttributeType => m_MovementAttributeType;

        public void Initialize()
        {
            // Initialize the formula collection
        }

        public void Cleanup()
        {
            // Cleanup the formula collection
        }

        public void Dispose()
        {
            // Dispose the formula collection
        }
    }
} 