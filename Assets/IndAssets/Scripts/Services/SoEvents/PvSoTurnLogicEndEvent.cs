using ProjectCI.CoreSystem.Runtime.Services;
using UnityEngine;

namespace ProjectCI.Utilities.Runtime.Events
{
    
    [CreateAssetMenu(fileName = "TurnLogic Event", menuName = "ProjectCI Utilities/Events/Void/TurnLogicEnd Event")]
    public class PvSoTurnLogicEndEvent : PvSoVoidEventBase, IService
    {
    }
}