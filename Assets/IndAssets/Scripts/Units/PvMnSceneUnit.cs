using IndAssets.Scripts.Weapons;
using ProjectCI.CoreSystem.Runtime.Abilities;
using ProjectCI.CoreSystem.Runtime.Attributes;
using ProjectCI.CoreSystem.Runtime.TacticRpgTool.Unit;
using ProjectCI.CoreSystem.Runtime.Units.Interfaces;
using ProjectCI_Animation.Runtime;
using System;
using System.Collections.Generic;
using UnityEngine;

namespace ProjectCI.CoreSystem.Runtime.TacticRpgTool.Concrete
{
    public class PvMnSceneUnit : MonoBehaviour, ISceneUnit
    {
        [SerializeField]
        private bool isSceneMode;

        [SerializeField]
        private bool isFriendly;
        public bool IsFriendly { get => isFriendly; set => isFriendly = value; }

        [SerializeField]
        protected PvSoBattleUnitData unitData;

        [SerializeField]
        private PvSoUnitAbility weaponAttackAbility;

        [SerializeField]
        private PvSoUnitAbility weaponFollowUpAbility;

        [SerializeField]
        private PvSoUnitAbility weaponCounterAbility;

        public PvSoBattleUnitData UnitData { get => unitData; set => unitData = value; }
        public PvSoUnitAbility WeaponAttackAbility => weaponAttackAbility;
        public PvSoUnitAbility WeaponFollowUpAbility => weaponFollowUpAbility;
        public PvSoUnitAbility WeaponCounterAbility => weaponCounterAbility;

        [SerializeField]
        private AttributeValuePair[] extraAttributes;

        [SerializeField]
        private PvSoWeaponData ownedWeapon;

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
            if (isSceneMode)
            {
                Initialize();
            }
        }

        #region SceneUnit Methods

        public void Initialize()
        {
            Initialize(ownedWeapon);
        }

        public void Initialize(PvSoWeaponData weaponData)
        {
            if (!weaponData)
            {
                return;
            }

            if (!ownedWeapon)
            {
                ownedWeapon = weaponData;
            }

            var animator = weaponData.Animator;

            weaponAttackAbility = weaponData.DefaultAttackAbility;
            weaponFollowUpAbility = weaponData.DefaultFollowUpAbility;
            weaponCounterAbility = weaponData.DefaultCounterAbility;

            RefreshWeapon(FindChild(transform, "weapon_r"), weaponData.weaponInfos[0]);
            if (weaponData.weaponInfos.Length > 1)
            {
                RefreshWeapon(FindChild(transform, "weapon_l"), weaponData.weaponInfos[1]);
            }
            
            if (animator)
            {
                var animatorController = GetComponentInChildren<UnitAnimationManager>();
                animatorController.SetupAnimationGraphDetails(animator, false);
            }
        }

        private void RefreshWeapon(Transform weaponRoot, FPvWeaponInfo weaponInfo)
        {
            if (weaponRoot.childCount > 0)
            {
                Destroy(weaponRoot.GetChild(0).gameObject);
                weaponRoot.DetachChildren();
            }

            var weaponInstance = Instantiate(weaponInfo.weaponPrefab, weaponRoot);
            weaponInstance.transform.localPosition = weaponInfo.prefabLocalPosition;
            weaponInstance.transform.localRotation = weaponInfo.prefabLocalRotation;
            weaponInstance.transform.localScale = weaponInfo.prefabLocalScale;
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

        public void InitializeAttributes(UnitAttributeContainer attributeContainer)
        {
            foreach (var attribute in extraAttributes)
            {
                attributeContainer.SetGeneralAttribute(attribute.m_AttributeType, attribute.m_Value);
            }

            if (!ownedWeapon)
            {
                return;
            }

            foreach (var attributePair in ownedWeapon.attributes)
            {
                attributeContainer.SetGeneralAttribute(attributePair.type, attributePair.value);
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