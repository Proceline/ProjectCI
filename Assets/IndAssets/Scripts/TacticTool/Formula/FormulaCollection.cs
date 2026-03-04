using ProjectCI.CoreSystem.Runtime.Services;
using ProjectCI.CoreSystem.Runtime.Attributes;
using UnityEngine;
using IndAssets.Scripts.Units;
using System.Collections.Generic;
using System;

namespace ProjectCI.TacticTool.Formula.Concrete
{
    [CreateAssetMenu(fileName = "FormulaCollection", menuName = "ProjectCI/Attributes/Create FormulaCollection")]
    public class FormulaCollection : ScriptableObject, IService
    {
        private static readonly ServiceLocator<FormulaCollection> FormulaService = new();
        private static FormulaCollection Instance => FormulaService.Service;

        [SerializeField]
        private AttributeType hitPointAttribute;

        [SerializeField]
        private AttributeType ultEnergyAttribute;

        [SerializeField]
        private AttributeType movementAttributeType;

        [SerializeField]
        private AttributeType criticalAttributeType;

        [SerializeField]
        private AttributeType attackSpeedType;

        [SerializeField]
        private AttributeType unitTypeAttribute;

        [SerializeField]
        private int attackSpeedDifference = 5;

        [SerializeField] private PvPersonalityRedirectionPair[] personalityRedirections;
        private readonly Dictionary<EPvPersonalityName, (AttributeType, AttributeType)> _personalityRedirectionDic = new();
        private Dictionary<EPvPersonalityName, (AttributeType, AttributeType)> PersonalityRedirectionDic
        {
            get
            {
                if (_personalityRedirectionDic.Count != personalityRedirections.Length)
                {
                    _personalityRedirectionDic.Clear();
                    foreach (var pair in personalityRedirections)
                    {
                        _personalityRedirectionDic.Add(pair.personalityName, (pair.leftSideAttribute, pair.rightSideAttribute));
                    }
                }

                return _personalityRedirectionDic;
            }
        }

        /// <summary>
        /// [("I <---> E")][("N <---> S")][("F <---> T")][("P <---> J")]
        /// </summary>
        /// <param name="personalityName"></param>
        /// <returns></returns>
        /// <exception cref="Exception"></exception>
        public (AttributeType, AttributeType) GetPersonalityAttributes(EPvPersonalityName personalityName)
        {
            if (PersonalityRedirectionDic.TryGetValue(personalityName, out var attributePair))
            {
                return attributePair;
            }

            throw new Exception("No such Personality Attribute existed!");
        }

        /// <summary>
        /// [("I <---> E")][("N <---> S")][("F <---> T")][("P <---> J")]
        /// </summary>
        /// <param name="personalityName"></param>
        /// <param name="getAttributeValue"></param>
        /// <param name="details"></param>
        /// <returns></returns>
        public int GetPersonalityDifference(EPvPersonalityName personalityName, Func<AttributeType, int> getAttributeValue, out (int, int) details)
        {
            var items = GetPersonalityAttributes(personalityName);
            var rightSide = getAttributeValue.Invoke(items.Item2);
            var leftSide = getAttributeValue.Invoke(items.Item1);
            details = (leftSide, rightSide);
            return rightSide - leftSide;
        }
        
        [SerializeField]
        private FormulaDefinition[] m_Formulas;

        public FormulaDefinition[] Formulas => m_Formulas;

        public AttributeType HealthAttributeType => hitPointAttribute;
        public AttributeType CriticalAttributeType => criticalAttributeType;
        public AttributeType AttackSpeedType => attackSpeedType;
        public AttributeType UnitTypeAttribute => unitTypeAttribute;
        public int AttackSpeedDifference => attackSpeedDifference;

        public static AttributeType UltEnergyType => Instance.ultEnergyAttribute;
        public static AttributeType MoveValueType => Instance.movementAttributeType;

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