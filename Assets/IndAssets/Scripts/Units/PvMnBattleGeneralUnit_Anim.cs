using System;
using ProjectCI_Animation.Runtime;
using ProjectCI.CoreSystem.Runtime.Animation.Services;
using ProjectCI.CoreSystem.Runtime.Services;
using UnityEngine;

namespace ProjectCI.CoreSystem.Runtime.TacticRpgTool.Concrete
{
    public partial class PvMnBattleGeneralUnit
    {
        private readonly ServiceLocator<PvSoAnimSetsCollection> _animSetService = new();

        [NonSerialized] 
        private UnitAnimationManager _mountAnimalAnimManager;
        
        public void UpdateAnimationToRide(PvSoBattleUnitData unitDataWithDetail)
        {
            if (!_animationManager)
            {
                throw new NullReferenceException("ERROR: Animation Essential Component missing!");
            }

            var animSetManager = _animSetService.Service;
            _animationManager.SetupAnimationGraphDetails(animSetManager.defaultRiderAnimation, false);

            var rootOfMesh = transform.GetChild(0);
            var currentRootPosition = rootOfMesh.transform.localPosition;
            
            // TODO: Consider to set the Height as an CONST, but V3 cannot be set as CONST
            rootOfMesh.transform.localPosition = currentRootPosition + Vector3.up;

            if (!unitDataWithDetail.PresetAnimatedMount)
            {
                throw new TypeAccessException("This Animal cannot become a Mount!");
            }

            _mountAnimalAnimManager = Instantiate(unitDataWithDetail.PresetAnimatedMount, transform);
        }
    }
} 