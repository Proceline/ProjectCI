using ProjectCI.CoreSystem.Runtime.TacticRpgTool.Gameplay;
using ProjectCI.CoreSystem.Runtime.TacticRpgTool.GridData;

namespace ProjectCI.CoreSystem.Runtime.TacticRpgTool.Concrete
{
    public class PvMnLevelCell : LevelCellImp
    {
        protected override void Start()
        {
            base.Start();

            var objRenderer = GetRenderer();
            if (objRenderer)
            {
                objRenderer.enabled = false;
            }

        }

        public override void SetMaterial(CellState InCellState)
        {
            var objRenderer = GetRenderer();

            if (InCellState == CellState.eNormal)
            {
                objRenderer.enabled = false;
            }
            else
            {
                if (!objRenderer.enabled)
                {
                    objRenderer.enabled = true;
                }
                base.SetMaterial(InCellState);
            }
        }
    }
}