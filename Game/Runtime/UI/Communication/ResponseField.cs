#region Packages

using Sirenix.OdinInspector;
using TMPro;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

#endregion

namespace Runtime.UI.Communication
{
    public sealed class ResponseField : MonoBehaviour
    {
        #region Values

        [SerializeField, Required] private GameObject buttonPrefab;
        [SerializeField, Required] private Image uiImage;
        [SerializeField] private float sizeX;

        #endregion

        #region Build In States

        private void Start() =>
            this.uiImage.enabled = false;

        #endregion

        #region In

        public void Show(string[] labels, UnityEvent[] actions)
        {
            RectTransform t = this.transform as RectTransform;
            RectTransform rectTransform = this.uiImage.transform as RectTransform;

            if (t == null || rectTransform == null) return;

            float sizeY = ((RectTransform)this.buttonPrefab.transform).sizeDelta.y;
            const float sizeBetween = 10;
            const float edge = 10;

            this.uiImage.enabled = true;

            rectTransform.sizeDelta =
                new Vector2(this.sizeX, edge * 2 + sizeY * labels.Length + sizeBetween * (labels.Length - 1));
            rectTransform.localPosition = Vector3.up * rectTransform.sizeDelta.y / 2f;

            Vector3 initPosition = Vector3.up * (edge + sizeY / 2f),
                increaseOffset = Vector3.up * (sizeY + sizeBetween);

            for (int i = 0; i < labels.Length; i++)
            {
                GameObject obj = Instantiate(this.buttonPrefab, t);
                obj.transform.localPosition = initPosition + increaseOffset * i;
                int callIndex = i;
                obj.GetComponent<Button>().onClick.AddListener(() =>
                {
                    actions[callIndex].Invoke();
                    this.Hide();
                });
                obj.GetComponentInChildren<TextMeshProUGUI>().text = labels[i];
            }
        }

        private void Hide()
        {
            this.uiImage.enabled = false;

            for (int i = this.transform.childCount - 1; i > 0; i--)
                Destroy(this.transform.GetChild(i).gameObject);
        }

        #endregion
    }
}