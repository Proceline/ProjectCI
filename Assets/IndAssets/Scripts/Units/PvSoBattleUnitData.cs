using ProjectCI.CoreSystem.Runtime.Passives;
using ProjectCI.CoreSystem.Runtime.TacticRpgTool.Unit;
using UnityEngine;

namespace ProjectCI.CoreSystem.Runtime.TacticRpgTool.Concrete
{
    [CreateAssetMenu(fileName = "NewUnitData", menuName = "ProjectCI Tools/Create PvSoUnitData", order = 1)]
    public class PvSoBattleUnitData : SoUnitData
    {
        [SerializeField] 
        private PvSoPassiveBase[] personalPassives;

        public override void InitializeUnitDataToGridUnit(GridPawnUnit pawnUnit)
        {
            foreach (var passive in personalPassives)
            {
                passive.InstallPassive(pawnUnit);
            }
        }
    }
} 