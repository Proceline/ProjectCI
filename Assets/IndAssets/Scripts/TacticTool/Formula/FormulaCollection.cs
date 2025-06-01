using System.Collections.Generic;
using ProjectCI.CoreSystem.Runtime.Attributes;
using UnityEngine;

namespace ProjectCI.TacticTool.Formula.Concrete
{
    [CreateAssetMenu(fileName = "FormulaCollection", menuName = "ProjectCI/Attributes/Create FormulaCollection")]
    public class FormulaCollection : ScriptableObject
    {
        [SerializeField]
        private FormulaDefinition[] m_Formulas;

        public FormulaDefinition[] Formulas => m_Formulas;
    }
} 