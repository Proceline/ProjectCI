using UnityEngine;

namespace ProjectCI.Utilities.Runtime.Events
{
    public interface IEventOwner
    {
        string EventIdentifier { get; }
        bool IsGridObject { get; }
        Vector3 Position { get; }
        Vector2 GridPosition { get; }
    }
}