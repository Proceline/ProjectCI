using IndAssets.Scripts.Units;
using ProjectCI.CoreSystem.Runtime.Abilities;
using ProjectCI.CoreSystem.Runtime.Passives;
using ProjectCI.CoreSystem.Runtime.Saving.Interfaces;
using ProjectCI.CoreSystem.Runtime.TacticRpgTool.Unit;
using UnityEngine;
using ProjectCI.CoreSystem.Runtime.Services;
using ProjectCI.TacticTool.Formula.Concrete;

namespace ProjectCI.CoreSystem.Runtime.TacticRpgTool.Concrete
{
    [CreateAssetMenu(fileName = "NewUnitData", menuName = "ProjectCI Tools/Create PvSoUnitData", order = 1)]
    public class PvSoBattleUnitData : SoUnitData, IPvSaveEntry
    {
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

        [SerializeField] private PvPersonalitiesCombination personality;

        private static readonly ServiceLocator<FormulaCollection> FormulaService = new();
        private static FormulaCollection FormulaColInstance => FormulaService.Service;

        public override void InitializeUnitDataToGridUnit(GridPawnUnit pawnUnit)
        {
            var energyLevel = personality.GetBasicLevel(EPvPersonalityName.Energy);
            var informationLevel = personality.GetBasicLevel(EPvPersonalityName.Information);
            var decisionLevel = personality.GetBasicLevel(EPvPersonalityName.Decisions);
            var styleLevel = personality.GetBasicLevel(EPvPersonalityName.Style);
            var energyAttribute = FormulaColInstance.GetPersonalityAttribute(EPvPersonalityName.Energy);
            var informationAttribute = FormulaColInstance.GetPersonalityAttribute(EPvPersonalityName.Information);
            var decisionAttribute = FormulaColInstance.GetPersonalityAttribute(EPvPersonalityName.Decisions);
            var styleAttribute = FormulaColInstance.GetPersonalityAttribute(EPvPersonalityName.Style);
            pawnUnit.RuntimeAttributes.SetGeneralAttribute(energyAttribute, energyLevel);
            pawnUnit.RuntimeAttributes.SetGeneralAttribute(informationAttribute, informationLevel);
            pawnUnit.RuntimeAttributes.SetGeneralAttribute(decisionAttribute, decisionLevel);
            pawnUnit.RuntimeAttributes.SetGeneralAttribute(styleAttribute, styleLevel);

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
    }
} 