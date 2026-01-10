using System;
using System.Collections.Generic;
using ProjectCI.CoreSystem.Runtime.TacticRpgTool.Concrete;
using UnityEngine;

namespace ProjectCI.CoreSystem.Runtime.Passives
{   
    public abstract class PvSoPassiveGlobal : PvSoPassiveBase
    {
        public override void InstallPassive(PvMnBattleGeneralUnit unit)
        {
            // Empty
        }

        public override void DisposePassive(PvMnBattleGeneralUnit unit)
        {
            // Empty
        }
    }
}