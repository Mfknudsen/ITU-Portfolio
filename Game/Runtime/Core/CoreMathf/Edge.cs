#region Libraries

using UnityEngine;

#endregion

namespace Runtime.Core.CoreMathf
{
    public readonly struct Edge
    {
        public readonly Vector3 A, B;

        public Edge(Vector3 A, Vector3 B)
        {
            this.A = A;
            this.B = B;
        }

        public Vector3 Dir => this.A - this.B;

        public bool Contains(Vector3 t) => this.A == t || this.B == t;

        public Vector3 Other(Vector3 t) => this.A == t ? this.B : this.A;
    }
}