#region Packages

using UnityEngine;

#endregion

namespace Runtime.ScriptableVariables.Structs
{
    public sealed class Vec3GenericVariable : ScriptableVariable<Vector3>
    {
        public float x => this.Value.x;
        public float y => this.Value.y;
        public float z => this.Value.z;
    }
}