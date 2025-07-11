using ProjectCI.CoreSystem.Runtime.TacticRpgTool.Unit;
using UnityEngine;

namespace ProjectCI.CoreSystem.Runtime.TacticRpgTool.GUI
{
    public class SlotDataUIElementBase : MonoBehaviour
    {
        protected AbilityListUIElementBase Owner { get; private set; }
        public virtual string DisplayName { get; set; }

        public void SetOwner(AbilityListUIElementBase InListUIElem)
        {
            Owner = InListUIElem;
        }

        public virtual void SetAbility(UnitAbilityCore InAbility, int InIndex)
        {
            // Do nothing
        }

        public virtual void ClearAbility()
        {
            // Do nothing
        }

        public virtual void OnHover()
        {
            // Do nothing
        }
        
        protected internal virtual void ForceHighlight(bool enabled)
        {
            
        }
    }
}
