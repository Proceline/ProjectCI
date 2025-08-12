using ProjectCI.CoreSystem.Runtime.TacticRpgTool.Unit;
using UnityEngine;

namespace ProjectCI.CoreSystem.Runtime.TacticRpgTool.GUI
{
    public abstract class SlotDataUIElementBase : MonoBehaviour
    {
        protected AbilityListUIElementBase Owner { get; private set; }
        public virtual string DisplayName { get; set; }

        public void SetOwner(AbilityListUIElementBase inListUIElem)
        {
            Owner = inListUIElem;
        }

        public abstract void SetAbility(UnitAbilityCore inAbility, int inIndex);

        public abstract void ClearAbility();

        protected internal abstract void ForceHighlight(bool isEnabled);
    }
}
