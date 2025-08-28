using ProjectCI.CoreSystem.Runtime.Abilities;
using ProjectCI.CoreSystem.Runtime.TacticRpgTool.Unit;

namespace ProjectCI.CoreSystem.Runtime.TacticRpgTool.Concrete
{
    public interface IGameVisual
    {
        internal void OnVisualUpdate();
        internal void OnVisualUpdate(GridPawnUnit selectedUnit);
        internal void OnVisualUpdate(PvSoUnitAbility ability, GridPawnUnit selectedUnit);
        public void ResetVisualStateCells();
        public void HighlightAbilityRange(PvSoUnitAbility ability, GridPawnUnit casterUnit);
    }
}