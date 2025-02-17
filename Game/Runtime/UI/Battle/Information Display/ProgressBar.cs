using UnityEngine;
using UnityEngine.UI;

namespace Runtime.UI.Battle.Information_Display
{
    public class ProgressBar : MonoBehaviour
    {
        public Image bar;
        public float curBar, maxBar;

        public void SetBarMax(float max)
        {
            this.maxBar = max;
            this.curBar = this.maxBar;

            this.SetCurrentBar(this.curBar);
        }

        public void SetCurrentBar(float input)
        {
            this.curBar = Mathf.Clamp(input, 0, Mathf.Infinity);
            
            if(this.maxBar == 0) return;
            
            float procent = (100 / this.maxBar) * this.curBar / 100;
            
            if (procent < 0.25)
                this.bar.color = Color.red;
            else if (procent < 0.7)
                this.bar.color = Color.yellow;
            else
                this.bar.color = Color.green;

            procent = Mathf.Clamp(procent, 0.0f, 1.0f);


            this.bar.transform.localScale = new Vector3(procent, 1, 1);
        }
    }
}
