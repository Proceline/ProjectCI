using ProjectCI_Animation.Runtime;
using ProjectCI.CoreSystem.Runtime.Passives;
using ProjectCI.CoreSystem.Runtime.Saving.Interfaces;
using ProjectCI.CoreSystem.Runtime.TacticRpgTool.Unit;
using UnityEngine;

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
        private UnitAnimationManager animatedMountForm;

        public UnitAnimationManager PresetAnimatedMount => animatedMountForm;

        [SerializeField]
        private GameObject headMeshPrefab;

        public GameObject HeadMeshPrefab => headMeshPrefab;

        public override void InitializeUnitDataToGridUnit(GridPawnUnit pawnUnit)
        {
            foreach (var passive in personalPassives)
            {
                passive.InstallPassive(pawnUnit);
            }
        }

        public string EntryId => unitIdentifier;
        public Sprite GetIcon => icon;
    }
} 