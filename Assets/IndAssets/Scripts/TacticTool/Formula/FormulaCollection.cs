using ProjectCI.CoreSystem.Runtime.Services;
using ProjectCI.CoreSystem.Runtime.Attributes;
using UnityEngine;
using IndAssets.Scripts.Units;
using System.Collections.Generic;

namespace ProjectCI.TacticTool.Formula.Concrete
{
    [CreateAssetMenu(fileName = "FormulaCollection", menuName = "ProjectCI/Attributes/Create FormulaCollection")]
    public class FormulaCollection : ScriptableObject, IService
    {
        [SerializeField]
        private AttributeType hitPointAttribute;

        [SerializeField]
        private AttributeType m_MovementAttributeType;

        [SerializeField]
        private AttributeType criticalAttributeType;

        [SerializeField]
        private AttributeType attackSpeedType;

        [SerializeField]
        private int attackSpeedDifference = 5;

        [SerializeField] private PvPersonalityRedirectionPair[] personalityRedirections;
        private readonly Dictionary<EPvPersonalityName, AttributeType> _personalityRedirectionDic = new();
        private Dictionary<EPvPersonalityName, AttributeType> PersonalityRedirectionDic
        {
            get
            {
                if (_personalityRedirectionDic.Count != personalityRedirections.Length)
                {
                    _personalityRedirectionDic.Clear();
                    foreach (var pair in personalityRedirections)
                    {
                        _personalityRedirectionDic.Add(pair.personalityName, pair.redirectToAttribute);
                    }
                }

                return _personalityRedirectionDic;
            }
        }

        public AttributeType GetPersonalityAttribute(EPvPersonalityName personalityName)
        {
            if (PersonalityRedirectionDic.TryGetValue(personalityName, out var attributeType))
            {
                return attributeType;
            }

            Debug.LogError($"Personality {personalityName} not found in redirection dictionary.");
            return attackSpeedType; // Return a default value to avoid null reference
        }
        
        [SerializeField]
        private FormulaDefinition[] m_Formulas;

        public FormulaDefinition[] Formulas => m_Formulas;

        public AttributeType HealthAttributeType => hitPointAttribute;
        public AttributeType MovementAttributeType => m_MovementAttributeType;
        public AttributeType CriticalAttributeType => criticalAttributeType;
        public AttributeType AttackSpeedType => attackSpeedType;
        public int AttackSpeedDifference => attackSpeedDifference;

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