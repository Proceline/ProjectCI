using UnityEngine;
using ProjectCI.CoreSystem.Runtime.Units.Interfaces;
using System;
using ProjectCI.CoreSystem.Runtime.TacticRpgTool.Unit;
using System.Collections.Generic;
using IndAssets.Scripts.Weapons;
using ProjectCI_Animation.Runtime;
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

        [SerializeField]
        private PvSoWeaponData[] ownedWeapons;

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

        private void Start()
        {
            Initialize();
        }

        #region SceneUnit Methods

        public void Initialize()
        {
            AnimationPlayableSupportBase animator = null;
            for (var i = 0; i <= 1; i++)
            {
                if (i >= ownedWeapons.Length)
                {
                    break;
                }

                var weaponData = ownedWeapons[i];
                var prefab = weaponData.weaponPrefab;
                var weaponRoot = FindChild(transform, i == 0? "weapon_r" : "weapon_l");
                var weaponInstance = Instantiate(prefab, weaponRoot);
                weaponInstance.transform.localPosition = weaponData.prefabLocalPosition;
                weaponInstance.transform.localRotation = weaponData.prefabLocalRotation;
                weaponInstance.transform.localScale = weaponData.prefabLocalScale;

                animator = weaponData.Animator;
            }
            
            if (animator)
            {
                var animatorController = GetComponentInChildren<UnitAnimationManager>();
                animatorController.SetupAnimationGraphDetails(animator, false);
            }
        }

        public void PostInitialize()
        {
            // Empty
        }

        public void SetVisible(bool visible)
        {
            // Empty
        }

        public void CleanUp()
        {
            // Implement cleanup logic if needed
        }
        #endregion

        public void SetExtraAttributes(UnitAttributeContainer attributeContainer)
        {
            foreach (var attribute in extraAttributes)
            {
                attributeContainer.SetGeneralAttribute(attribute.m_AttributeType, attribute.m_Value);
            }
        }

        private Transform FindChild(Transform parent, string childName)
        {
            foreach (Transform child in parent)
            {
                if (child.name == childName)
                    return child;

                var result = FindChild(child, childName);
                if (result != null)
                {
                    return result;
                }
            }

            return null;
        }
    }
} 