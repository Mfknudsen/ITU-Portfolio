#region Package

using UnityEngine;

#endregion

namespace Runtime.World.Overworld.Decorations
{
    public class Windmill : MonoBehaviour
    {
        private Transform wings;
        private float speed = 20;
        private Vector3 forward;

        private void Start()
        {
            Transform tempTrans;
            this.wings = (tempTrans = this.transform).GetChild(1);
            this.forward = tempTrans.up;
            this.wings.Rotate(this.forward, Random.Range(1f, 100f));
            this.speed += Random.Range(-5f, 5f);
        }

        private void Update()
        {
            this.wings.Rotate(this.forward, (this.speed * Time.deltaTime));
        }
    }
}