using System.Text.RegularExpressions;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using static TMPro.TMP_InputField;

namespace GentlyUI.UIElements {
    [AddComponentMenu("GentlyUI/Input Field", 8)]
    public class GMInputField : GMSelectable, IPointerClickHandler,
        IUpdateSelectedHandler,
        ISubmitHandler,
        ISelectHandler,
        IDeselectHandler,
        IBeginDragHandler,
        IDragHandler {

        /// <summary>
        /// The current text of the input field.
        /// </summary>
        [Tooltip("The current text of the input field.")]
        [SerializeField] private string text = "";

        /// <summary>
        /// The viewport defines the interactive area of the input field.
        /// </summary>
        [Tooltip("The viewport defines the interactive area of the input field.")]
        [SerializeField] private RectTransform viewport;

        /// <summary>
        /// Text is displayed here.
        /// </summary>
        [Tooltip("Text is displayed here.")]
        [SerializeField] private GMTextComponent textOutput;

        /// <summary>
        /// Shown when no text was entered yet.
        /// </summary>
        [Tooltip("Shown when no text was entered yet.")]
        [SerializeField] private Graphic placeholder;

        /// <summary>
        /// The caret that indicates the cursor position;
        /// </summary>
        [Tooltip("The caret that indicates the cursor position;")]
        [SerializeField] private Graphic caret;

        /// <summary>
        /// The size of the caret.
        /// </summary>
        [SerializeField] [Range(0.5f, 3f)] private float caretSize = 1f;

        /// <summary>
        /// The blink rate of the caret in seconds
        /// </summary>
        [SerializeField] [Range(0f, 1f)] private float caretBlinkRate = 1f;

        /// <summary>
        /// The graphic that indicates the current selected part of the text.
        /// </summary>
        [Tooltip("The graphic that indicates the current selected part of the text.")]
        [SerializeField] private Graphic selection;

        /// <summary>
        /// The type of input. This mostly defines how input is displayed (e.g. passwords are hidden).
        /// </summary>
        [Tooltip("The type of input. This mostly defines how input is displayed (e.g. passwords are hidden).")]
        [SerializeField] private InputType inputType = InputType.Standard;

        /// <summary>
        /// What kind of validation to use when input is entered.
        /// </summary>
        [Tooltip("What kind of validation to use when input is entered.")]
        [SerializeField] private CharacterValidation characterValidation;

        /// <summary>
        /// Maximum number of characters that are allowed.
        /// </summary>
        [Tooltip("Maximum number of characters that are allowed.")]
        [SerializeField] private int characterLimit = 20;

        /// <summary>
        /// The character used to hide text in password field.
        /// </summary>
        [Tooltip("The character used to hide text in password field.")]
        [SerializeField] private char asteriskChar = '*';

        /// <summary>
        /// Defines whether the new text is submitted on deselect or reset.
        /// </summary>
        [SerializeField] private bool submitOnDeselect;

        /// <summary>
        /// Event delegates triggered when the input field changes its data.
        /// </summary>
        [Tooltip("Event delegates triggered when the input field changes its data.")]
        [SerializeField] private OnChangeEvent onValueChanged = new OnChangeEvent();

        public SubmitEvent onSubmit = new SubmitEvent();
        public OnValidateInput onValidateInput;

        /// <summary>
        /// Defines whether the input field is read only thus its content can't be changed.
        /// </summary>
        [Tooltip("Defines whether the input field is read only thus its content can't be changed.")]
        [SerializeField] private bool readOnly;

        private bool isFocused;
        /// <summary>
        /// Returns true if the input field is currently focused.
        /// </summary>
        [Tooltip("Returns true if the input field is currently focused.")]
        public bool IsFocused => isFocused;

        private int caretPosition;
        private int selectionStartPosition;
        private int selectionEndPosition;
        private float blinkTimer;
        private string initialText;

        static private readonly char[] kSeparators = { ' ', '.', ',', '\t', '\r', '\n' };
        const string kEmailSpecialCharacters = "!#$%&'*+-/=?^_`{|}~"; // Doesn't include dot and @ on purpose!

        public string Text {
            get { return text; }
            set { SetText(value); }
        }

        private bool HasSelection() {
            return selectionStartPosition != selectionEndPosition;
        }

        public void SetTextWithoutNotify(string input) {
            SetText(input, false);
        }

        void SetText(string value, bool sendCallback = true) {
            if (Text == value)
                return;

            if (value == null)
                value = "";

            text = value;

            if (sendCallback)
                SendOnValueChanged();

            UpdateLabel();
        }

        private void SendOnValueChanged() {
            if (onValueChanged != null)
                onValueChanged.Invoke(text);
        }


        protected override void OnInitialize() {
            base.OnInitialize();

            textOutput.enableWordWrapping = false;
            selection.rectTransform.pivot = new Vector2(0, selection.rectTransform.pivot.y);
        }

        protected override void OnEnable() {
            base.OnEnable();

            UpdateLabel();
            UpdateCaretAndSelection();
        }

        protected virtual bool IsValidChar(char c) {
            return textOutput.font.HasCharacter(c, true);
        }

        private Event processingEvent = new Event();

        public void OnUpdateSelected(BaseEventData eventData) {
            if (!isFocused)
                return;

            while (Event.PopEvent(processingEvent)) {
                //A key was pressed
                if (processingEvent.rawType == EventType.KeyDown) {
                    KeyPressed(processingEvent);
                }
            }
        }

        protected void KeyPressed(Event evt) {
            EventModifiers currentEventModifiers = evt.modifiers;
            bool ctrl = SystemInfo.operatingSystemFamily == OperatingSystemFamily.MacOSX
                        ? (currentEventModifiers & EventModifiers.Command) != 0
                        : (currentEventModifiers & EventModifiers.Control) != 0;
            bool shift = (currentEventModifiers & EventModifiers.Shift) != 0;
            bool alt = (currentEventModifiers & EventModifiers.Alt) != 0;
            bool ctrlOnly = ctrl && !alt && !shift;

            switch (evt.keyCode) {
                case KeyCode.Backspace:
                    //Delete from back
                    Backspace();
                    return;
                case KeyCode.Delete:
                    //Delete in front
                    DeleteKey();
                    return;
                case KeyCode.Home:
                    //Goto start
                    HomeButton(shift);
                    return;
                case KeyCode.End:
                    //Goto end
                    EndButton(shift);
                    return;
                case KeyCode.A:
                    //Select all
                    if (ctrlOnly) {
                        SelectAll();
                        return;
                    }

                    break;
                case KeyCode.C:
                    //Copy
                    if (ctrlOnly) {
                        if (inputType != InputType.Password)
                            clipboard = GetSelectedString();
                        else
                            clipboard = "";
                        return;
                    }
                    break;
                case KeyCode.V:
                    //Paste
                    if (ctrlOnly) {
                        Append(clipboard);
                        return;
                    }
                    break;
                case KeyCode.X:
                    //Cut
                    if (ctrlOnly) {
                        if (inputType != InputType.Password)
                            clipboard = GetSelectedString();
                        else
                            clipboard = "";
                        Delete();
                        return;
                    }
                    break;
                case KeyCode.LeftArrow:
                    Move(Vector2Int.left, shift, ctrl);
                    return;
                case KeyCode.RightArrow:
                    Move(Vector2Int.right, shift, ctrl);
                    return;
                case KeyCode.Return:
                case KeyCode.KeypadEnter:
                    //Submit
                    onSubmit.Invoke(Text);
                    break;

                case KeyCode.Escape:
                    Escape();
                    return;
            }

            char c = evt.character;

            if (IsValidChar(c)) {
                Append(c);
            }
        }

        private void Escape() {
            if (isFocused) {
                ResetText();
                isFocused = false;
                UpdateCaretAndSelection();
                UIManager.Instance.SelectUI(null);
            }
        }

        private void Backspace() {
            if (HasSelection()) {
                Delete();
            } else if (selectionStartPosition > 0) {
                Text = Text.Remove(selectionStartPosition - 1, 1);
                SetCaretPosition(selectionStartPosition - 1);
            }
        }

        private void DeleteKey() {
            if (HasSelection()) {
                Delete();
            } else if (selectionStartPosition < Text.Length) {
                Text = Text.Remove(selectionStartPosition, 1);
            }
        }

        private void Delete() {
            if (readOnly)
                return;

            if (selectionEndPosition - selectionStartPosition >= Text.Length) {
                //Delete all
                Text = "";
                SetCaretPosition(0);
            } else {
                Text = Text.Remove(selectionStartPosition, selectionEndPosition - selectionStartPosition);
                SetCaretPosition(selectionStartPosition);
            }
        }

        void EndButton(bool shift) {
            if (shift) {
                SetSelectionEndPosition(Text.Length);
            } else {
                SetCaretPosition(Text.Length);
            }
        }

        void HomeButton(bool shift) {
            if (shift) {
                SetSelectionStartPosition(0);
            } else {
                SetCaretPosition(0);
            }
        }

        void ResetText() {
            Text = initialText;
        }

        void SetCaretPosition(int position) {
            SetSelectionStartPosition(position);
            SetSelectionEndPosition(position);
            caretPosition = position;
            //Reset blink timer
            ResetCaretBlink();
        }

        void ResetCaretBlink() {
            caret.canvasRenderer.SetAlpha(1f);
            blinkTimer = 0f;
        }

        void SetSelectionStartPosition(int position) {
            selectionStartPosition = position;
            selectionStartPosition = Mathf.Clamp(selectionStartPosition, 0, Text.Length);
        }

        void SetSelectionEndPosition(int position) {
            selectionEndPosition = position;
            selectionEndPosition = Mathf.Clamp(selectionEndPosition, 0, Text.Length);
        }

        public override void Tick(float unscaledDeltaTime) {
            base.Tick(unscaledDeltaTime);

            if (!isFocused)
                return;

            //Do text updates
            UpdateLabel();

            if (textOutput.havePropertiesChanged) {
                textOutput.ForceMeshUpdate();
            }

            UpdateCaretAndSelection();

            //Caret blink
            if (!HasSelection()) {
                if (caretBlinkRate > 0f) {
                    blinkTimer += unscaledDeltaTime;

                    if (blinkTimer >= caretBlinkRate) {
                        blinkTimer -= caretBlinkRate;

                        if (caret.canvasRenderer.GetAlpha() == 0f) {
                            caret.canvasRenderer.SetAlpha(1f);
                        } else {
                            caret.canvasRenderer.SetAlpha(0f);
                        }
                    }
                } else {
                    caret.canvasRenderer.SetAlpha(1f);
                }
            }
        }

        void UpdateCaretAndSelection() {
            if (!isFocused) {
                caret.gameObject.SetActive(false);
                selection.gameObject.SetActive(false);

                return;
            }

            Vector2 startPos, endPos;
            TMP_CharacterInfo tmpInfo;
            float offset = 0f;

            if (Text.Length > 0) {
                if (selectionStartPosition == Text.Length) {
                    tmpInfo = textOutput.textInfo.characterInfo[Text.Length - 1];
                    offset = tmpInfo.bottomRight.x - tmpInfo.bottomLeft.x;
                } else {
                    tmpInfo = textOutput.textInfo.characterInfo[selectionStartPosition];
                }

                startPos = new Vector2(tmpInfo.origin + offset, caret.rectTransform.localPosition.y);

                if (selectionEndPosition == Text.Length) {
                    tmpInfo = textOutput.textInfo.characterInfo[Text.Length - 1];
                    offset = tmpInfo.bottomRight.x - tmpInfo.bottomLeft.x;
                } else {
                    tmpInfo = textOutput.textInfo.characterInfo[selectionEndPosition];
                }

                endPos = new Vector2(tmpInfo.origin + offset, caret.rectTransform.localPosition.y);
            } else {
                Vector3[] corners = new Vector3[4];
                textOutput.rectTransform.GetLocalCorners(corners);

                if (textOutput.alignment == TextAlignmentOptions.Center) {
                    startPos = endPos = new Vector2(Mathf.Lerp(corners[0].x, corners[2].x, 0.5f), caret.rectTransform.localPosition.y);
                } else if (textOutput.alignment == TextAlignmentOptions.Right) {
                    startPos = endPos = new Vector2(corners[2].x, caret.rectTransform.localPosition.y);
                } else {
                    startPos = endPos = new Vector2(corners[0].x, caret.rectTransform.localPosition.y);
                }
            }

            if (selectionStartPosition == selectionEndPosition) {
                if (!caret.gameObject.activeSelf) caret.gameObject.SetActive(true);
                if (selection.gameObject.activeSelf) selection.gameObject.SetActive(false);

                caret.rectTransform.localPosition = startPos;
                caret.rectTransform.SetSizeWithCurrentAnchors(RectTransform.Axis.Horizontal, caretSize);
            } else {
                if (caret.gameObject.activeSelf) caret.gameObject.SetActive(false);
                if (!selection.gameObject.activeSelf) selection.gameObject.SetActive(true);

                selection.rectTransform.localPosition = startPos;
                selection.rectTransform.SetSize(new Vector2(endPos.x - startPos.x, selection.rectTransform.GetSize().y));
            }

            UpdateTextOffsetPosition();
        }

        void UpdateTextOffsetPosition() {
            //Move textoutput to show caret or selection position
            if (Text.Length > 0) {
                int testIndex = 0;

                if (selectionStartPosition < caretPosition) {
                    testIndex = selectionStartPosition;
                } else if (selectionEndPosition > caretPosition) {
                    testIndex = selectionEndPosition;
                } else {
                    testIndex = caretPosition;
                }

                testIndex = Mathf.Clamp(testIndex, 0, Text.Length - 1);
                TMP_CharacterInfo charInfo = textOutput.textInfo.characterInfo[testIndex];

                float viewportMin = viewport.rect.xMin;
                float viewportMax = viewport.rect.xMax;

                float rightOffset = viewportMax - (textOutput.rectTransform.anchoredPosition.x + charInfo.origin + textOutput.margin.z + caretSize + GetCharacterWidth(charInfo));
                if (rightOffset < 0f) {
                    textOutput.rectTransform.anchoredPosition += new Vector2(rightOffset, 0);
                }

                float leftOffset = (textOutput.rectTransform.anchoredPosition.x + charInfo.origin - textOutput.margin.x) - viewportMin;
                if (leftOffset < 0f) {
                    textOutput.rectTransform.anchoredPosition += new Vector2(-leftOffset, 0);
                }
            }
        }

        float GetCharacterWidth(TMP_CharacterInfo charInfo) {
            return Mathf.Abs(charInfo.bottomRight.x - charInfo.bottomLeft.x);
        }

        void Move(Vector2Int direction, bool shift, bool ctrl) {
            int change = ctrl ? direction.x * characterLimit : direction.x;

            if (shift) {
                if (selectionStartPosition < caretPosition) {
                    SetSelectionStartPosition(selectionStartPosition + change);
                } else if (selectionEndPosition > caretPosition) {
                    SetSelectionEndPosition(selectionEndPosition + change);
                } else if (direction.x < 0) {
                    SetSelectionStartPosition(selectionStartPosition + change);
                } else if (direction.x > 0) {
                    SetSelectionEndPosition(selectionEndPosition + change);
                }
            } else {
                SetCaretPosition(selectionStartPosition + direction.x);
            }
        }

        protected virtual void Append(string input) {
            if (readOnly)
                return;

            for (int i = 0, length = input.Length; i < length; ++i) {
                char c = input[i];

                if (c >= ' ' || c == '\t' || c == '\r' || c == 10 || c == '\n') {
                    Append(c);
                }
            }
        }

        protected virtual void Append(char input) {
            if (readOnly)
                return;

            if (onValidateInput != null) {
                input = onValidateInput(Text, caretPosition, input);
            } else if (characterValidation == CharacterValidation.CustomValidator) {
                throw new System.NotImplementedException();
            } else if (characterValidation != CharacterValidation.None) {
                input = Validate(Text, caretPosition, input);
            }

            if (input == 0)
                return;

            Insert(input);
        }

        bool IsInsertingAllowed() {
            return characterLimit > 0 && Text.Length < characterLimit;
        }

        private void Insert(char c) {
            if (readOnly || !IsInsertingAllowed())
                return;

            if (HasSelection()) {
                Delete();
            }

            Text = Text.Insert(selectionStartPosition, c.ToString());

            SetCaretPosition(selectionStartPosition + 1);
        }

        void UpdateLabel() {
            string processed;
            if (inputType == InputType.Password) {
                processed = new string(asteriskChar, Text.Length);
            } else {
                processed = Text;
            }

            if (textOutput.text != processed) {
                textOutput.SetText(processed);

                placeholder.gameObject.SetActive(processed.Length == 0);
            }
        }

        string GetSelectedString() {
            return text.Substring(selectionStartPosition, selectionEndPosition - selectionStartPosition);
        }

        void SelectAll() {
            SetSelectionStartPosition(0);
            SetSelectionEndPosition(Text.Length);
        }

        private bool MayDrag(PointerEventData eventData) {
            return IsActive() &&
                   Interactable &&
                   eventData.button == PointerEventData.InputButton.Left &&
                   textOutput != null;
        }

        public virtual void OnBeginDrag(PointerEventData eventData) {
            if (!MayDrag(eventData)) {
                return;
            }

            UpdateSelectionByDragPosition(eventData);
        }

        public virtual void OnEndDrag(PointerEventData eventData) {
            if (!MayDrag(eventData)) {
                return;
            }
        }

        public virtual void OnDrag(PointerEventData eventData) {
            if (!MayDrag(eventData)) {
                return;
            }

            UpdateSelectionByDragPosition(eventData);
        }

        void SetCaretPositionByClickPosition(PointerEventData eventData) {
            int caretPosition = GetClickPosition(eventData);
            SetCaretPosition(caretPosition);
        }

        void UpdateSelectionByDragPosition(PointerEventData eventData) {
            int selectPosition = GetClickPosition(eventData);

            if (selectPosition < caretPosition) {
                SetSelectionStartPosition(selectPosition);
            } else if (selectPosition > caretPosition) {
                SetSelectionEndPosition(selectPosition);
            } else {
                SetCaretPosition(caretPosition);
            }

            RectTransformUtility.ScreenPointToLocalPointInRectangle(viewport, eventData.position, eventData.pressEventCamera, out Vector2 localMousePos);

            if (localMousePos.x < viewport.rect.xMin) {
                Move(Vector2Int.left, true, false);
            } else if (localMousePos.x > viewport.rect.xMax) {
                Move(Vector2Int.right, true, false);
            }
        }

        int GetClickPosition(PointerEventData eventData) {
            if (Text.Length == 0) return 0;

            int insertionIndex = TMP_TextUtilities.GetCursorIndexFromPosition(textOutput, eventData.position, eventData.pressEventCamera);

            return insertionIndex;
        }

        public override void OnPointerDown(PointerEventData eventData) {
            if (eventData.button != PointerEventData.InputButton.Left || !Interactable) return;

            UIManager.Instance.SelectUI(gameObject);

            SetCaretPositionByClickPosition(eventData);

            base.OnPointerDown(eventData);
        }

        public virtual void OnPointerClick(PointerEventData eventData) {
            if (eventData.button != PointerEventData.InputButton.Left || !Interactable) return;

            if (eventData.clickCount == 2) {
                SelectAll();
            }
        }

        public void OnSubmit(BaseEventData eventData) {
            onSubmit.Invoke(Text);
            UIManager.Instance.SelectUI(null);
        }

        public void OnSelect(BaseEventData eventData) {
            OnFocused();
        }

        public void OnDeselect(BaseEventData eventData) {
            OnFocusLost();
        }

        void OnFocused() {
            initialText = text;
            isFocused = true;
        }

        void OnFocusLost() {
            if (!submitOnDeselect) {
                ResetText();
            } else {
                OnSubmit(null);
            }

            isFocused = false;
            UpdateCaretAndSelection();
        }

        public void SetCharacterValidation(CharacterValidation validation) {
            this.characterValidation = validation;
        }

        public void SetInputType(InputType inputType) {
            this.inputType = inputType;
        }

        public void SetPlaceholderText(string placeholderText) {
            if (placeholder is TextMeshProUGUI _textOutput) {
                _textOutput.SetText(placeholderText);
            }
        }

        static string clipboard {
            get {
                return GUIUtility.systemCopyBuffer;
            }
            set {
                GUIUtility.systemCopyBuffer = value;
            }
        }

        /// <summary>
        /// Validate the specified input.
        /// </summary>
        protected char Validate(string text, int pos, char ch) {
            // Validation is disabled
            if (characterValidation == CharacterValidation.None || !enabled)
                return ch;

            if (characterValidation == CharacterValidation.Integer || characterValidation == CharacterValidation.Decimal) {
                // Integer and decimal
                bool cursorBeforeDash = (pos == 0 && text.Length > 0 && text[0] == '-');
                bool selectionAtStart = caretPosition == 0 || selectionStartPosition == 0;
                if (!cursorBeforeDash) {
                    if (ch >= '0' && ch <= '9') return ch;
                    if (ch == '-' && (pos == 0 || selectionAtStart)) return ch;
                    if (ch == '.' && characterValidation == CharacterValidation.Decimal && !text.Contains(".")) return ch;
                }
            } else if (characterValidation == CharacterValidation.Digit) {
                if (ch >= '0' && ch <= '9') return ch;
            } else if (characterValidation == CharacterValidation.Alphanumeric) {
                // All alphanumeric characters
                if (ch >= 'A' && ch <= 'Z') return ch;
                if (ch >= 'a' && ch <= 'z') return ch;
                if (ch >= '0' && ch <= '9') return ch;
            } else if (characterValidation == CharacterValidation.Name) {
                char lastChar = (text.Length > 0) ? text[Mathf.Clamp(pos, 0, text.Length - 1)] : ' ';
                char nextChar = (text.Length > 0) ? text[Mathf.Clamp(pos + 1, 0, text.Length - 1)] : '\n';

                if (char.IsLetter(ch)) {
                    // Space followed by a letter -- make sure it's capitalized
                    if (char.IsLower(ch) && lastChar == ' ')
                        return char.ToUpper(ch);

                    // Uppercase letters are only allowed after spaces (and apostrophes)
                    if (char.IsUpper(ch) && lastChar != ' ' && lastChar != '\'')
                        return char.ToLower(ch);

                    // If character was already in correct case, return it as-is.
                    // Also, letters that are neither upper nor lower case are always allowed.
                    return ch;
                } else if (ch == '\'') {
                    // Don't allow more than one apostrophe
                    if (lastChar != ' ' && lastChar != '\'' && nextChar != '\'' && !text.Contains("'"))
                        return ch;
                } else if (ch == ' ') {
                    // Don't allow more than one space in a row
                    if (lastChar != ' ' && lastChar != '\'' && nextChar != ' ' && nextChar != '\'')
                        return ch;
                }
            } else if (characterValidation == CharacterValidation.EmailAddress) {
                // From StackOverflow about allowed characters in email addresses:
                // Uppercase and lowercase English letters (a-z, A-Z)
                // Digits 0 to 9
                // Characters ! # $ % & ' * + - / = ? ^ _ ` { | } ~
                // Character . (dot, period, full stop) provided that it is not the first or last character,
                // and provided also that it does not appear two or more times consecutively.

                if (ch >= 'A' && ch <= 'Z') return ch;
                if (ch >= 'a' && ch <= 'z') return ch;
                if (ch >= '0' && ch <= '9') return ch;
                if (ch == '@' && text.IndexOf('@') == -1) return ch;
                if (kEmailSpecialCharacters.IndexOf(ch) != -1) return ch;
                if (ch == '.') {
                    char lastChar = (text.Length > 0) ? text[Mathf.Clamp(pos, 0, text.Length - 1)] : ' ';
                    char nextChar = (text.Length > 0) ? text[Mathf.Clamp(pos + 1, 0, text.Length - 1)] : '\n';
                    if (lastChar != '.' && nextChar != '.')
                        return ch;
                }
            }

            return (char)0;
        }

        public enum InputType {
            Standard,
            Password
        }
    }
}
