using System.Collections.Generic;
using ProjectCI.CoreSystem.Runtime.TacticRpgTool.AI;
using ProjectCI.CoreSystem.Runtime.TacticRpgTool.Concrete;
using ProjectCI.CoreSystem.Runtime.TacticRpgTool.GridData;
using ProjectCI.CoreSystem.Runtime.TacticRpgTool.Unit;
using TMPro;
using UnityEngine;

public class PvDebugGridHint : MonoBehaviour
{
    public GridPawnUnit inSceneUnit;
    public int presetRange;
    public bool bAllowBlocked;
    public BattleTeam teamRelation = BattleTeam.Friendly;

    public TextMeshPro textValueHint;
    
    public void ShowTargetPawnField()
    {
        var startUnit = inSceneUnit;
        if (!startUnit || !textValueHint)
        {
            Debug.LogError("You must drag and drop a unit!");
            return;
        }
        
        AIRadiusInfo radiusInfo = new AIRadiusInfo(startUnit.GetCell(), presetRange)
        {
            Caster = startUnit,
            bAllowBlocked = bAllowBlocked,
            bStopAtBlockedCell = true,
            EffectedTeam = teamRelation
        };
        
        var radiusField = BucketDijkstraSolutionUtils.CalculateBucket(radiusInfo, false, 10);

        foreach (var pair in radiusField.Dist)
        {
            var cell = pair.Key;
            var value = pair.Value;

            var hintText = Instantiate(textValueHint, transform);
            hintText.gameObject.SetActive(true);
            hintText.text = value.ToString("00");
            hintText.transform.localPosition = cell.transform.position;
        }

        var ability = (startUnit as PvMnBattleGeneralUnit)?.EquippedAbility;
        if (ability)
        {
            var attackField = BucketDijkstraSolutionUtils.ComputeAttackField(radiusField, GetCellList);

            List<LevelCellBase> GetCellList(LevelCellBase startCell)
            {
                return ability.GetShape().GetCellList(startUnit, startCell, ability.GetRadius(),
                    ability.DoesAllowBlocked(), ability.GetEffectedTeam());
            }

            var victimDic = attackField.VictimsFromCells;
            foreach (var victim in attackField.AllVictims)
            {
                var hintText = Instantiate(textValueHint, transform);
                hintText.gameObject.SetActive(true);
                hintText.text = victimDic[victim].Count.ToString();
                hintText.color = Color.red;
                hintText.transform.localPosition = victim.transform.position + Vector3.up;
            }
        }
    }
}
