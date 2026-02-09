using System;
using UnityEngine;
using UnityEngine.Events;

namespace Assets.IndAssets.Scripts.Deployment
{
    public class PvMnDeployCell : MonoBehaviour
    {
        public ScriptableObject StandingData { get; private set; }

        [NonSerialized]
        private bool _isFocusing;

        [SerializeField]
        private UnityEvent<PvMnDeployCell, bool> onCellHovered;
        
        [SerializeField]
        private UnityEvent<PvMnDeployCell> onCellInteracted;

        public void SetCharacter(ScriptableObject data)
        {
            StandingData = data;
        }

        public void ClearCell()
        {
            StandingData = null;
        }

        private void Interact()
        {
            onCellInteracted?.Invoke(this);
        }

        #region EventListeners

        public void OnMouseOver()
        {
            if (!_isFocusing)
            {
                _isFocusing = true;
                onCellHovered?.Invoke(this, true);
            }
        }

        public void OnMouseExit()
        {
            _isFocusing = false;
            onCellHovered?.Invoke(this, false);
        }

        private void OnMouseDown()
        {
            // Empty
        }

        private void OnMouseUp()
        {
            Interact();
        }

        #endregion
    }
}
