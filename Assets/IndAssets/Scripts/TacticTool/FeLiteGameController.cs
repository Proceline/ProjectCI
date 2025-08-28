using ProjectCI.CoreSystem.Runtime.InputSupport;
using ProjectCI.CoreSystem.Runtime.TacticRpgTool.Gameplay.GameRules;
using ProjectCI.CoreSystem.Runtime.TacticRpgTool.GridData;
using ProjectCI.CoreSystem.Runtime.TacticRpgTool.Unit;
using UnityEngine;
using UnityEngine.InputSystem;

namespace ProjectCI.CoreSystem.Runtime.TacticRpgTool.Concrete
{
    public class FeLiteGameController : ScriptableObject, IGameController
    {
        [SerializeField] private BattleGameRules gameRulesModel;
        [SerializeField] private FeLiteGameVisual gameVisual;
        
        [SerializeField]
        private InputActionManager inputActionManager;
        
        public void RegisterControlActions()
        {
            inputActionManager.BindConfirmAction(HandleConfirmAction);
        }

        public void UnregisterControlActions()
        {
            inputActionManager.UnbindConfirmAction(HandleConfirmAction);
        }

        private void HandleConfirmAction(InputAction.CallbackContext context)
        {
            var currentHoverCell = gameVisual.CurrentHoverCell;
            if (gameVisual.CurrentHoverCell)
            {
                GridObject objOnCell = currentHoverCell.GetObjectOnCell();
                if (objOnCell)
                {
                    objOnCell.HandleBeingConfirmed();
                }
            }

            HandleCellClicked(currentHoverCell);
        }

        private void HandleCellClicked(LevelCellBase inCell)
        {
            if (!inCell)
            {
                return;
            }

            if (!gameRulesModel)
            {
                return;
            }

            GridPawnUnit standUnit = inCell.GetUnitOnCell();
            if (standUnit)
            {
                BattleTeam currentTurnTeam = gameRulesModel.GetCurrentTeam();
                BattleTeam unitsTeam = standUnit.GetTeam();

                if (unitsTeam == currentTurnTeam)
                {
                    gameRulesModel.HandlePlayerSelected(standUnit);
                }
                else
                {
                    if (unitsTeam == BattleTeam.Hostile)
                    {
                        gameRulesModel.HandleEnemySelected(standUnit);
                    }
                }
            }
            gameRulesModel.HandleCellSelected(inCell);
        }
    }
}