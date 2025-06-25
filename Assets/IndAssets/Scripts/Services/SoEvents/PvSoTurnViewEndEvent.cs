using ProjectCI.CoreSystem.Runtime.Services;
using UnityEngine;

namespace ProjectCI.Utilities.Runtime.Events
{
    
    [CreateAssetMenu(fileName = "UnitState Event", menuName = "ProjectCI Utilities/Events/Void/TurnViewEnd Event")]
    public class PvSoTurnViewEndEvent : PvSoVoidEventBase, IService
    {
    }
}