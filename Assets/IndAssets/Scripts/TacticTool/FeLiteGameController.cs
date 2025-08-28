using ProjectCI.CoreSystem.Runtime.InputSupport;
using ProjectCI.CoreSystem.Runtime.TacticRpgTool.Gameplay.GameRules;
using ProjectCI.CoreSystem.Runtime.TacticRpgTool.GridData;
using ProjectCI.CoreSystem.Runtime.TacticRpgTool.Unit;
using UnityEngine;
using UnityEngine.InputSystem;

namespace ProjectCI.CoreSystem.Runtime.TacticRpgTool.Concrete
{
    [CreateAssetMenu(fileName = "New Controller", menuName = "ProjectCI Tools/MVC/Controller", order = 1)]
    public class FeLiteGameController : ScriptableObject, IGameController
    {
        [SerializeField] private BattleGameRules gameRulesModel;
        [SerializeField] private FeLiteGameVisual gameVisual;
        
        [SerializeField]
        private InputActionManager inputActionManager;
        
        public void RegisterControlActions()
        {
            inputActionManager.EnableUnitControl();
            inputActionManager.BindConfirmAction(HandleConfirmAction);
        }

        public void UnregisterControlActions()
        {
            inputActionManager.UnbindConfirmAction(HandleConfirmAction);
            inputActionManager.DisableUnitControl();
        }

        private void HandleConfirmAction(InputAction.CallbackContext context)
        {
            var currentHoverCell = gameVisual.CurrentHoverCell;
            if (gameVisual.CurrentHoverCell)
            {
                GridObject objOnCell = currentHoverCell.GetObjectOnCell();
                // TODO: Handle Obj being confirmed
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
                BattleTeam currentTurnTeam = gameRulesModel.CurrentTeam;
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