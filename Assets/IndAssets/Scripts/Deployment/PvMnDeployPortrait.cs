using IndAssets.Scripts.Managers;
using ProjectCI.CoreSystem.Runtime.Saving;
using ProjectCI.CoreSystem.Runtime.TacticRpgTool.Concrete;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace Assets.IndAssets.Scripts.Deployment
{
    public class PvMnDeployPortrait : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler, IPointerClickHandler
    {
        public PvSoBattleUnitData Data { get; private set; }
        public Image iconImage;
        [SerializeField] private Image backgroundImage;

        [SerializeField] private Transform defaultParent;
        [SerializeField] private GameObject trackingPivot;
        [SerializeField] private Camera targetCamera;
        [SerializeField] private RectTransform canvasTransform;
        [SerializeField] private PvSoWeaponAndRelicCollection charactersCol;

        [Header("Events")]
        public UnityEvent<PvSoBattleUnitData, PvDeployCell> onPortraitInteracted;
        public UnityEvent onHoverEnter;
        public UnityEvent onHoverExit;

        [SerializeField]
        private UnityEvent<ScriptableObject, Image> onDataIconUpdated;

        private static PvDeployCell _currentContextCell;

        public static void SetupCurrentContextCell(PvDeployCell cell)
        {
            if (_currentContextCell)
            {
                _currentContextCell.onCellDeSelected?.Invoke();
            }
            _currentContextCell = cell;

            if (cell)
            {
                cell.onCellSelected?.Invoke();
            }
        }

        public void ToggleParent(PvDeployCell cell)
        {
            if (defaultParent && trackingPivot)
            {
                var currentEnableStatus = trackingPivot.activeSelf;
                trackingPivot.SetActive(!currentEnableStatus);

                if (currentEnableStatus)
                {
                    cell.onCellDeSelected?.Invoke();
                }
                else
                {
                    var targetPosition = WorldToCanvasPosition(cell.transform.position);
                    trackingPivot.transform.localPosition = targetPosition;
                    var existedPortraits = new List<PvMnDeployPortrait>(defaultParent.GetComponentsInChildren<PvMnDeployPortrait>(true));
                    var allCharacters = PvSaveManager.Instance.GetUnlockedCharacters();
                    if (existedPortraits.Count > allCharacters.Count)
                    {
                        for (var i = allCharacters.Count; i < existedPortraits.Count; i++)
                        {
                            existedPortraits[i].gameObject.SetActive(false);
                        }
                    }

                    if (allCharacters.Count > existedPortraits.Count)
                    {
                        for (var i = existedPortraits.Count; i < allCharacters.Count; i++)
                        {
                            var portrait = Instantiate(this, defaultParent);
                            existedPortraits.Add(portrait);
                        }
                    }

                    for (var i = 0; i < existedPortraits.Count; i++)
                    {
                        var portrait = existedPortraits[i];
                        var saveData = allCharacters[i];
                        if (charactersCol.UnitDataDict.TryGetValue(saveData.CharacterId, out var unitData))
                        {
                            portrait.gameObject.SetActive(true);
                            portrait.Data = unitData;
                            portrait.iconImage.sprite = unitData.GetIcon;
                            portrait.onDataIconUpdated?.Invoke(unitData, portrait.backgroundImage);
                        }
                    }
                }
            }
        }

        private void OnClickPortrait()
        {
            if (_currentContextCell)
            {
                onPortraitInteracted?.Invoke(Data, _currentContextCell); 
                onHoverExit?.Invoke();
            }
        }

        public void OnPointerEnter(PointerEventData eventData) => onHoverEnter?.Invoke();
        public void OnPointerExit(PointerEventData eventData) => onHoverExit?.Invoke();

        private Vector2 WorldToCanvasPosition(Vector3 worldPosition)
        {
            Vector3 screenPos = targetCamera.WorldToScreenPoint(worldPosition);

            if (screenPos.z < 0) return new Vector2(-10000, -10000);

            RectTransformUtility.ScreenPointToLocalPointInRectangle(
                canvasTransform,
                screenPos,
                targetCamera,
                out Vector2 localPoint
            );

            return localPoint;
        }

        public void OnPointerClick(PointerEventData eventData)
        {
            OnClickPortrait();
        }
    }
}