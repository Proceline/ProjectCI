using System;
using UnityEngine;
using UnityEngine.Events;

namespace Assets.IndAssets.Scripts.Deployment
{
    public abstract class PvDeployCell : MonoBehaviour
    {
        public UnityEvent onCellSelected;
        public UnityEvent onCellDeSelected;

        public ScriptableObject StandingData { get; protected set; }

        public abstract void SetCharacter(ScriptableObject data);
        public abstract void ClearCell();
    }
}
