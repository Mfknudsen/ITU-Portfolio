#region Packages

using System;
using System.Collections;
using System.Collections.Generic;
using Runtime.Player;
using Runtime.Systems;
using Runtime.Systems.UI;
using Runtime.UI.Book.Button;
using Runtime.UI.Book.Interfaces;
using Runtime.UI.Book.Light;
using Runtime.UI.Book.Slider;
using Runtime.UI.Book.TextInputField;
using Sirenix.OdinInspector;
using TMPro;
using Unity.Cinemachine;
using UnityEngine;
using UnityEngine.UI;

#endregion

namespace Runtime.UI_Book
{
    #region Enums

    public enum BookTurn
    {
        Null,
        Open,
        Close,
        Left,
        Right
    }

    #endregion

    public class UIBook : MonoBehaviour
    {
        #region Values

        [SerializeField] [Required] private PlayerManager playerManager;
        [SerializeField] [Required] private OperationManager operationManager;
        [SerializeField] [Required] private UIManager uiManager;
        [SerializeField] [Required] private CameraManager cameraManager;

        [BoxGroup("References")] [FoldoutGroup("References/Render Textures")] [SerializeField]
        private RenderTexture preRenderTexture, curRenderTexture;

        [FoldoutGroup("References/Camera")] [SerializeField]
        private CinemachineVirtualCameraBase bookCameraRig;

        [FoldoutGroup("References/Camera")] [SerializeField]
        private UIBookCameraTransition transition;

        [FoldoutGroup("References/Pages")] [SerializeField]
        private GameObject openLeft, openRight, turnLeft, turnRight;

        [FoldoutGroup("References/Animation")] [SerializeField]
        private Animator bookAnimator;

        [FoldoutGroup("References/Animation")] [SerializeField]
        private GameObject invisiblyUI, visuals;

        [BoxGroup("References")] [SerializeField]
        private UIBookLight bookLight;

        [SerializeField] [BoxGroup("Actions")] private BookOpenAction bookOpenActionAction;
        [SerializeField] [BoxGroup("Actions")] private BookTurnAction bookTurnBookAction;
        [SerializeField] [BoxGroup("Actions")] private CloseBookAction closeBookActionAction;

        public static UIBook Instance { get; private set; }

        private readonly Dictionary<string, BookButton> buttonReferences = new Dictionary<string, BookButton>();
        private readonly Dictionary<string, BookSlider> sliderReferences = new Dictionary<string, BookSlider>();

        private readonly Dictionary<string, BookTextInputField> textInputFieldReferences =
            new Dictionary<string, BookTextInputField>();

        private static readonly int PreRenderTextureID = Shader.PropertyToID("RenderTexture");

        #region Hash

        private static readonly int HashCloseBook = Animator.StringToHash("CloseBook"),
            HashOpenBook = Animator.StringToHash("OpenBook"),
            HashTurnLeft = Animator.StringToHash("TurnLeftToRight"),
            HashTurnRight = Animator.StringToHash("TurnRightToLeft");

        #endregion

        #endregion

        #region Build In States

        private IEnumerator Start()
        {
            Instance = this;

            DontDestroyOnLoad(this.gameObject);
            this.uiManager.SetUIBook(this);

            this.invisiblyUI.transform.localScale /= 10000;

            this.turnRight.SetActive(false);
            this.turnLeft.SetActive(false);

            this.bookLight.Calculate();
            this.transition.CheckMiddle();

            this.cameraManager.SetCurrentRig(this.bookCameraRig);
            Cursor.visible = true;

            yield return new WaitUntil(() => this.playerManager.Ready && this.uiManager.Ready);

            GameObject bookHolder = new GameObject("Book Holder")
            {
                transform = { parent = this.playerManager.GetController().transform }
            };

            this.transform.parent = bookHolder.transform;
        }

        #endregion

        #region Getters

        public GameObject GetVisuals()
        {
            return this.visuals;
        }

        #endregion

        #region In

        public void ConstructUI()
        {
            this.CopyTextures();
            this.ConstructAsync();
        }

        public void Effect(BookTurn turn)
        {
            if (turn == BookTurn.Null) return;

            this.invisiblyUI.SetActive(false);
            this.StartCoroutine(this.AnimationTrigger(turn, 0.5f));

            this.CopyTextures();

            if (turn == BookTurn.Close)
            {
                this.openLeft.GetComponent<Renderer>().material.SetTexture(PreRenderTextureID, this.preRenderTexture);
                this.openRight.GetComponent<Renderer>().material.SetTexture(PreRenderTextureID, this.preRenderTexture);
            }

            OperationsContainer container = new OperationsContainer();
            switch (turn)
            {
                case BookTurn.Open:
                    container.Add(this.bookOpenActionAction);
                    break;

                case BookTurn.Close:
                    container.Add(this.closeBookActionAction);
                    break;

                case BookTurn.Left:
                    this.bookTurnBookAction.SetDirection(false);
                    container.Add(this.bookTurnBookAction);
                    break;

                case BookTurn.Right:
                    this.bookTurnBookAction.SetDirection(true);
                    container.Add(this.bookTurnBookAction);
                    break;

                case BookTurn.Null:
                default:
                    throw new ArgumentOutOfRangeException(nameof(turn), turn, null);
            }

            this.operationManager.AddOperationsContainer(container);
        }

        private void CopyTextures()
        {
            Graphics.CopyTexture(this.curRenderTexture, this.preRenderTexture);
        }

        #endregion

        #region Internal

        private static List<GameObject> GetAllByRoot(GameObject obj)
        {
            List<GameObject> result = new List<GameObject>();

            foreach (Transform t in obj.transform)
            {
                result.Add(t.gameObject);
                result.AddRange(GetAllByRoot(t.gameObject));
            }

            return result;
        }

        private IEnumerator AnimationTrigger(BookTurn trigger, float time)
        {
            if (trigger == BookTurn.Null) yield break;

            yield return new WaitForSeconds(time);

            int hash = trigger switch
            {
                BookTurn.Close => HashCloseBook,
                BookTurn.Open => HashOpenBook,
                BookTurn.Left => HashTurnLeft,
                BookTurn.Right => HashTurnRight,
                _ => throw new ArgumentOutOfRangeException(nameof(trigger), trigger, null)
            };

            if (trigger is BookTurn.Close or BookTurn.Open) this.playerManager.GetController().TriggerAnimator(hash);

            this.bookAnimator.SetTrigger(hash);
        }

        private static void AddToReference<TElement>(GameObject obj, IDictionary<string, TElement> dictionary)
            where TElement : MonoBehaviour, ICustomGUIElement
        {
            if (obj.GetComponent<TElement>() is { } element)
                dictionary.Add(obj.name, element);
        }

        private void Replace<TReference, TElement>(GameObject obj, IReadOnlyDictionary<string, TReference> list)
            where TReference : MonoBehaviour, ICustomGUIElement
            where TElement : MonoBehaviour, ICustomGUIElementReference
        {
            string n = obj.name;

            if (!list.ContainsKey(n)) return;

            TReference element = list[n];

            if (element is null) return;

            Destroy(obj.GetComponent<TReference>());
            this.StartCoroutine(AddGUIReferenceComponent<TElement>(obj, this, element));
        }

        private static IEnumerator AddGUIReferenceComponent<T>(GameObject obj, UIBook uiBook, ICustomGUIElement element)
            where T : MonoBehaviour, ICustomGUIElementReference
        {
            yield return null;

            if (obj is null || element is null) yield break;

            obj.AddComponent<T>().Setup(uiBook, element);
        }

        private void ConstructAsync()
        {
            this.invisiblyUI.SetActive(false);

            foreach (Transform t in this.invisiblyUI.transform)
                Destroy(t.gameObject);

            foreach (Transform t in this.uiManager.CanvasObject.transform)
            {
                GameObject transObj = t.gameObject;
                string objName = transObj.name;

                if (!transObj.activeSelf ||
                    objName.Equals("Transition UI") ||
                    objName.Equals("Template"))
                    continue;

                GameObject obj = Instantiate(t.gameObject, this.invisiblyUI.transform);

                this.buttonReferences.Clear();
                this.sliderReferences.Clear();
                this.textInputFieldReferences.Clear();
                foreach (GameObject o in GetAllByRoot(t.gameObject))
                {
                    AddToReference(o, this.buttonReferences);
                    AddToReference(o, this.sliderReferences);
                    AddToReference(o, this.textInputFieldReferences);
                }

                //Clean the copied ui
                foreach (GameObject o in GetAllByRoot(obj))
                {
                    if (!o.activeSelf)
                    {
                        Destroy(o);
                        continue;
                    }

                    this.Replace<BookButton, BookButtonReference>(o, this.buttonReferences);
                    this.Replace<BookSlider, BookSliderReference>(o, this.sliderReferences);
                    this.Replace<BookTextInputField, BookTextInputFieldReference>(o, this.textInputFieldReferences);

                    if (o.GetComponent<Outline>() is { } outline)
                        Destroy(outline);

                    if (o.GetComponent<TextMeshProUGUI>() is { } text)
                        Destroy(text);
                }

                break;
            }

            this.invisiblyUI.SetActive(true);
        }

        #endregion
    }
}