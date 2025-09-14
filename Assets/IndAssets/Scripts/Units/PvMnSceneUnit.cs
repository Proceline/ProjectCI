using UnityEngine;
using ProjectCI.CoreSystem.Runtime.Units.Interfaces;
using System;
using ProjectCI.CoreSystem.Runtime.TacticRpgTool.Unit;
using System.Collections.Generic;
using ProjectCI.CoreSystem.Runtime.Abilities;
using ProjectCI.CoreSystem.Runtime.Attributes;

namespace ProjectCI.CoreSystem.Runtime.TacticRpgTool.Concrete
{
    public class PvMnSceneUnit : MonoBehaviour, ISceneUnit
    {
        [SerializeField]
        private bool isFriendly;
        public bool IsFriendly => isFriendly;

        [SerializeField]
        protected SoUnitData unitData;

        [SerializeField]
        protected List<PvSoUnitAbility> unitAbilities;

        public SoUnitData UnitData => unitData;
        public List<PvSoUnitAbility> UnitAbilities => unitAbilities;

        [SerializeField]
        private AttributeValuePair[] extraAttributes;

        /// <summary>
        /// Gets the unique identifier of the object
        /// </summary>
        public string ID { get; private set; }

        /// <summary>
        /// Generates a new unique identifier for the object
        /// </summary>
        public void GenerateNewID()
        {
            ID = Guid.NewGuid().ToString();
        }

        public bool IsVisible { get; private set; }
        public Vector3 Bounds { get; private set; }
        public Vector3 Position { get; private set; }

        // Methods
        public void Initialize()
        {
        }

        public void PostInitialize()
        {
        }

        public void SetVisible(bool visible)
        {
        }

        public void CleanUp()
        {
            // Implement cleanup logic if needed
        }

        public void SetExtraAttributes(UnitAttributeContainer attributeContainer)
        {
            foreach (var attribute in extraAttributes)
            {
                attributeContainer.SetGeneralAttribute(attribute.m_AttributeType, attribute.m_Value);
            }
        }
    }
} 