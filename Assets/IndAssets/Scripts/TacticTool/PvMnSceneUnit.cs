using UnityEngine;
using ProjectCI.CoreSystem.Runtime.Units.Interfaces;
using System;
using ProjectCI.CoreSystem.Runtime.TacticRpgTool.Unit;
using System.Collections.Generic;

namespace ProjectCI.CoreSystem.Runtime.TacticRpgTool.Concrete
{
    public class PvMnSceneUnit : MonoBehaviour, ISceneUnit
    {
        [SerializeField]
        protected SoUnitData unitData;

        [SerializeField]
        protected List<UnitAbilityCore> unitAbilities;

        public SoUnitData UnitData => unitData;
        public List<UnitAbilityCore> UnitAbilities => unitAbilities;

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
    }
} 