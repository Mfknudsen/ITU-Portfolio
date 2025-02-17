#region Packages

using UnityEngine;

#endregion

namespace Runtime.ScriptableVariables.Structs
{
    [CreateAssetMenu(menuName = "Variables/Vector2")]
    public sealed class Vec2GenericVariable : ScriptableVariable<Vector2>
    {
        public float x => this.Value.x;
        public float y => this.Value.y;
    }
}