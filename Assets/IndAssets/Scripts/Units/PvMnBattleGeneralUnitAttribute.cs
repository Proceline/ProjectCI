using ProjectCI.CoreSystem.Runtime.Attributes;
using ProjectCI.Utilities.Runtime.Modifiers;

namespace ProjectCI.CoreSystem.Runtime.TacticRpgTool.Concrete
{
    public partial class PvMnBattleGeneralUnit : IAttributeOwner
    {
        public int GetAttributeValue(int attributeValue)
        {
            var attributeType = (AttributeType)attributeValue;
            return RuntimeAttributes.GetAttributeValue(attributeType);
        }
    }
} 