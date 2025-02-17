#region Packages

using UnityEngine;

#endregion

namespace Runtime.ScriptableVariables.Objects
{
    [CreateAssetMenu(menuName = "Variables/Transform")]
    public sealed class TransformGenericVariable : ScriptableVariable<Transform>
    {
        public Vector3 Position => this.Value.position;
        public Vector3 Forward => this.Value.forward;
        public Vector3 Up => this.Value.up;
        public Vector3 Right => this.Value.right;
        public Quaternion Rotation => this.Value.rotation;
        public Vector3 Euler => this.Value.rotation.eulerAngles;
    }
}