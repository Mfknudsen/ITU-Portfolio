#region Packages

using Runtime.Systems;
using System.Collections.Generic;
using Runtime.Core;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

#endregion

namespace Runtime.Testing
{
    public sealed class Logger : MonoBehaviour
    {
        #region Values

        public static Logger instance;
        [SerializeField] private bool active, show;
        [SerializeField] private TextMeshProUGUI textField;

        // ReSharper disable once CollectionNeverQueried.Local
        private readonly List<string> textLog = new List<string>();
        [SerializeField] private Scrollbar scroller;

        #endregion

        private void Start()
        {
            if (instance == null) return;

            instance = this;
            this.textField.text = "";

            InputManager.Instance.showHideEvent.AddListener(this.ShowHide);
            this.ShowHide();
        }

        private void Awake()
        {
            foreach (Transform t in this.transform)
                t.gameObject.SetActive(this.active);
        }

        #region In

        // ReSharper disable Unity.PerformanceAnalysis
        public void AddNewLog(string script, string input)
        {
            string scriptText = script + "[" + System.DateTime.Now.ToLocalTime().ToString("HH:mm:ss") + "]: ";
            this.textLog.Add(scriptText + input);

            this.textField.text += scriptText + "\n" + input + "\n";

            this.Invoke(nameof(this.ScrollControl), 0.01f);
        }

        #region Statics

        public static void AddLog(object script, string input) =>
            instance.AddNewLog(script.ToString(), input);

        #endregion

        #endregion

        #region Out

        private void ScrollControl()
        {
            this.scroller.value = 0;
        }

        #endregion

        #region Internal

        private void ShowHide()
        {
            foreach (Transform t in this.transform)
                t.gameObject.SetActive(this.show);

            this.show.Reverse();
        }

        #endregion
    }
}