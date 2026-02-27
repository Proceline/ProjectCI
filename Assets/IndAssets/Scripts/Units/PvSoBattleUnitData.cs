using IndAssets.Scripts.Units;
using ProjectCI.CoreSystem.Runtime.Abilities;
using ProjectCI.CoreSystem.Runtime.Attributes;
using ProjectCI.CoreSystem.Runtime.Passives;
using ProjectCI.CoreSystem.Runtime.Saving.Interfaces;
using ProjectCI.CoreSystem.Runtime.Services;
using ProjectCI.CoreSystem.Runtime.TacticRpgTool.Unit;
using ProjectCI.TacticTool.Formula.Concrete;
using System.Collections.Generic;
using UnityEngine;

namespace ProjectCI.CoreSystem.Runtime.TacticRpgTool.Concrete
{
    [CreateAssetMenu(fileName = "NewUnitData", menuName = "ProjectCI Tools/Create PvSoUnitData", order = 1)]
    public class PvSoBattleUnitData : SoUnitData, IPvSaveEntry
    {
        private readonly Dictionary<AttributeType, int> _runtimeOriginalAttributes = new();

        [SerializeField] private string unitIdentifier;
        [SerializeField] private Sprite icon;
        
        [SerializeField] 
        private PvSoPassiveBase[] personalPassives;

        [SerializeField]
        private GameObject[] meshes;

        public GameObject BodyMeshPrefab => meshes[0];
        public GameObject HeadMeshPrefab => meshes.Length > 1 ? meshes[1] : null;

        [SerializeField]
        private PvSoUnitAbility talentedSupportAbility;

        [SerializeField]
        private PvSoUnitAbility talentedUltimateAbility;

        public PvSoUnitAbility TalentedSupportAbility => talentedSupportAbility;
        public PvSoUnitAbility TalentedUltimateAbility => talentedUltimateAbility;

        private static readonly ServiceLocator<FormulaCollection> FormulaService = new();
        private static FormulaCollection FormulaColInstance => FormulaService.Service;

        public override void InitializeUnitDataToGridUnit(GridPawnUnit pawnUnit)
        {
            foreach (var attrItem in originalAttributes)
            {
                pawnUnit.RuntimeAttributes.SetGeneralAttribute(attrItem.m_AttributeType, attrItem.m_Value);
            }

            if (pawnUnit is PvMnBattleGeneralUnit battleUnit)
            {
                battleUnit.CleanUpPassives();

                foreach (var passive in personalPassives)
                {
                    passive.InstallPassive(battleUnit);
                    battleUnit.AddPassiveRecord(passive);
                }
            }
        }

        public string EntryId => unitIdentifier;
        public Sprite GetIcon => icon;

        /// <summary>
        /// Should only be used in UI sessions
        /// </summary>
        /// <param name="personalityElement"></param>
        /// <returns></returns>
        public int GetPersonalityLevel(EPvPersonalityName personalityElement, out (int, int) details)
        {
            details = (0, 0);
            return FormulaColInstance.GetPersonalityDifference(personalityElement, GetOriginalAttributeValue, out details);
        }

        private int GetOriginalAttributeValue(AttributeType attributeType)
        {
            if (_runtimeOriginalAttributes.Count == 0)
            {
                foreach (var item in originalAttributes)
                {
                    _runtimeOriginalAttributes.Add(item.m_AttributeType, item.m_Value);
                }
            }

            if (_runtimeOriginalAttributes.TryGetValue(attributeType, out var value))
            {
                return value;
            }
            return 0;
        }

        public string GetPersonalitySpecialDescription(out string desc)
        {
            var personalityPassive = personalPassives.Length > 0 ? personalPassives[0] : null;
            
            if (personalityPassive)
            {
                desc = personalityPassive.description;
                return personalityPassive.PassiveName;
            }

            desc = string.Empty;
            return string.Empty;
        }
    }
} 