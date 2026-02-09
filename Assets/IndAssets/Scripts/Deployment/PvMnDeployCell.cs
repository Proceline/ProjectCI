using System;
using UnityEngine;
using UnityEngine.Events;

namespace Assets.IndAssets.Scripts.Deployment
{
    public class PvMnDeployCell : PvDeployCell
    {
        [NonSerialized] private static bool _isEnabled = true;

        [SerializeField]
        private Renderer cellRenderer;

        [SerializeField]
        private UnityEvent<PvDeployCell> onCellInteracted;

        public override void SetCharacter(ScriptableObject data)
        {
            StandingData = data;
        }

        public override void ClearCell()
        {
            StandingData = null;
        }

        private void Interact()
        {
            onCellInteracted?.Invoke(this);
        }

        public static void SetDeployCellEnableGlobally(bool isEnabled)
        {
            _isEnabled = isEnabled;
        }

        #region Materials
        public void SetMaterial(Material targetMaterial)
        {
            Material[] meshMaterials = cellRenderer.materials;
            meshMaterials[0] = targetMaterial;
            cellRenderer.materials = meshMaterials;
        }
        #endregion

        #region EventListeners
        public void OnRaycastsReceived(RaycastHit[] hits)
        {
            if (!_isEnabled)
            {
                return;
            }

            foreach (var hit in hits)
            {
                if (hit.transform.tag == "Marker")
                {
                    var cell = hit.transform.GetComponent<PvMnDeployCell>();
                    if (cell)
                    {
                        cell.Interact();
                    }
                    return;
                }
            }
        }

        #endregion
    }
}
