#region Packages

using System.Collections;
using UnityEngine;

#endregion

namespace Runtime.UI.Overworld.Sight_Alerts
{
    [CreateAssetMenu(menuName = "UI/Sight Alerts/Standard Alert")]
    public class StandardAlert : AlertType
    {
        public override bool IsOperationDone => throw new System.NotImplementedException();

        public override IEnumerator Operation()
        {
            throw new System.NotImplementedException();
        }

        public override void OperationEnd()
        {
            throw new System.NotImplementedException();
        }
    }
}
