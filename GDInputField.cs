using System;
using System.Threading.Tasks;
using AwesomeGolf.Common;
using UnityEngine;
using UnityEngine.UIElements;
using Mopsicus.AG.Modified;

namespace AwesomeGolf.UI
{
    [UxmlElement]
    public partial class GDInputField : VisualElement
    {
        //***************************************************************************
        // Event Delegates
        //***************************************************************************

        public delegate void GDInputFieldEditingEvent();

        public static event GDInputFieldEditingEvent OnUserInputFinished;

        public delegate void GDInputFieldEvent(GDInputField gdInputField);

        public static event GDInputFieldEvent OnGainedFocus;
        public static event GDInputFieldEvent OnLostFocus;

        public delegate void GDInputFieldTextEvent(GDInputField gdInputField);

        public static event GDInputFieldTextEvent OnTextChanged;



        //***************************************************************************
        // Constants 
        //***************************************************************************

        private const string cMobileInputFieldName = "mobileInputField";
        private const string cKeyboardAvoidingPanelName = "GDKeyboardAvoidingPanel";



        //***************************************************************************
        // Getters & Setters
        //***************************************************************************

        private static bool UsingMobileInput
        {
            get
            {
#if (UNITY_IOS || UNITY_ANDROID) && !UNITY_EDITOR
                return true;
#else
                return false;
#endif
            }
        }



        //***************************************************************************
        // Class Properties
        //***************************************************************************

        private bool mMoveOnWhenLimitReached;
        private bool mIsKeyboardOpen;
        private Color mInputColorNormal;
        private Color mInputColorSelected;
        private bool mUnityInputFieldHasFocus;
        private bool mMobileKeyboardShownByMe;
        private bool mIsInitialised = false;
        private string mMobileHackPlaceholderText;
        private string mMobileHackInputText;
        private string mPreviousDesktopText = "";
        private TouchScreenKeyboard mTouchScreenKeyboard;
        private GDInputField mNextInputField;
        private GDMobileInputField mMobileInputField;
        private Label mForceBehindReplacementText;
        private Image mInputTextBoxImage;
        private GDKeyboardAvoidingPanel mKeyboardAvoidance;
        private TextField lastDesktopInputFieldToHaveFocus;
        private VisualElement rootVisualElement;



        //***************************************************************************
        // Getters 
        //***************************************************************************

        public string Text
        {
            get
            {
                if (UsingMobileInput)
                {
                    if (mAlwaysOnTopHackCount > 0)
                        return mMobileHackInputText;

                    if (mMobileInputField.pInputField != null)
                        return mMobileInputField.pInputField.text;
                }
                else if (mMobileInputField.pInputField == null)
                    return "";

                return mMobileInputField.pInputField.value;
            }

            private set
            {
                if (mMobileInputField.pInputField.value != null)
                {
                    mMobileInputField.pInputField.value = value;
                    if (mAlwaysOnTopHackCount > 0)
                        UpdateReplacementText(value);
                }
            }
        }



        //***************************************************************************
        // Initialisation
        //***************************************************************************

        public void Initialise()
        {
            InitialiseInputField();
            SetInitialFocus();

            mIsInitialised = true;
        }

        private void InitialiseInputField()
        {
            // Step up the hierarchy until we find GDKeyboard or reach the top
            VisualElement currentVisualElement = this;
            while (currentVisualElement != null
                   && currentVisualElement.GetType() != typeof(GDKeyboardAvoidingPanel))
            {
                currentVisualElement = currentVisualElement.hierarchy.parent;
            }

            // Issue with callbacks creating empty UI fields
            rootVisualElement = currentVisualElement;
            mMobileInputField = this.Q<GDMobileInputField>(cMobileInputFieldName);
            mMobileInputField.InitialiseTextField();
            mMobileInputField.pInputField.RegisterValueChangedCallback(evt => OnTextUpdated(evt.newValue));

            // Prevent UI Builder from triggering
            if (mMobileInputField == null || mMobileInputField.pInputField == null)
            {
                return;
            }

            mKeyboardAvoidance = rootVisualElement.Q<GDKeyboardAvoidingPanel>(cKeyboardAvoidingPanelName);
            
            if (mMobileInputField.pInputField == null)
                return;
            
#if ((UNITY_IOS || UNITY_ANDROID) && !UNITY_EDITOR)
            mMobileInputField.pInputField.SetEnabled(false);

            // Remove disabled styling
            mMobileInputField.pInputField.RemoveFromClassList("unity-disabled");

            Font mobileFont = Resources.Load<Font>($"Fonts/{mMobileInputField.pCustomFont}");
            FontDefinition fontDef = new FontDefinition
            {
                font = mobileFont
            };

            mMobileInputField.pInputField.style.unityFontDefinition = fontDef;
#endif
            
            mMobileInputField.OnFocusChanged += isFocused =>
            {
                if (isFocused)
                {
                    if (!mUnityInputFieldHasFocus)
                    {
                        SetAsSelected();
                    }
                }
                else
                {
                    if (mUnityInputFieldHasFocus)
                    {
                        SetAsNormal();
                    }
                }
            };

            // Handle the return key (e.g., "Next" or "Done")
            mMobileInputField.pInputField.RegisterCallback<KeyDownEvent>(evt =>
            {
                if (evt.keyCode == KeyCode.Return)
                {
                    if (mNextInputField != null)
                    {
                        mNextInputField.SetFocus(true);
                    }
                    else
                    {
                        SetFocus(false);
                    }

                    evt.StopPropagation();
                }
            });

            GDMobileInput.OnShowKeyboard += MobileInput_OnShowKeyboard;
        }

        private void SetInitialFocus()
        {
#if !((UNITY_IOS || UNITY_ANDROID) && !UNITY_EDITOR)
            if (mMobileInputField == null || mMobileInputField.pInputField == null)
                return;

            var isFocused = mMobileInputField.pInputField.focusController.focusedElement
                            == mMobileInputField.pInputField;

            switch (mUnityInputFieldHasFocus)
            {
                case false when isFocused:
                    SetAsSelected();
                    break;

                case true when !isFocused:
                    SetAsNormal();
                    break;
            }
#endif
        }



        //***************************************************************************
        // Mobile Only Methods
        //***************************************************************************

        private void MobileInput_OnShowKeyboard(bool isShow, int height)
        {
            // iOS 14
            // - Input field activated (SetASSelected)
            // - get this event for the input field
            // - if go to another input field, get event for existing
            //   input field not the new one.
            // - so, we need to record who set the height, what the height is,
            //   and if its on show already.

            if (isShow)
            {
                // Since every input field is attached to the callback, this will filter them 
                if (mUnityInputFieldHasFocus)
                {
                    MobileShowHideKeyboard(true, height);
                    mMobileKeyboardShownByMe = true;
                }
            }
            else
            {
                MobileShowHideKeyboard(false, height);
                mMobileKeyboardShownByMe = false;
            }
        }

        private void MobileShowHideKeyboard(bool isShow, int height)
        {
            AgLogger.Log(
                $"Mobile keyboard will show / hide with height {height}, " +
                $"a screen height of {Screen.height} and " +
                $"a canvasHeight of {UIManager.sCanvasHeight}");

            // BF - 17/11/22 - Had a regular instance on an android device giving a
            // keyboard height that was larger than its screen height!
            // So, this is a workaround, to catch this 'bug' in android.
            // If the height of the keyboard is nonsense, assume half-height of screen.
            if (height > Screen.height)
            {
                AgLogger.Log(
                    "Nonsense!  Forcing height to be half of screen!");

                height = (int)(Screen.height * 0.5f);
            }

            OnKeyboardWillShowHide(isShow, height);
        }

        private void SetAsNormal()
        {
            mUnityInputFieldHasFocus = false;
            OnLostFocus?.Invoke(this);
        }

        private void SetAsSelected()
        {
            mUnityInputFieldHasFocus = true;
            OnGainedFocus?.Invoke(this);
        }



        //***************************************************************************
        // Shared Methods
        //***************************************************************************

        public void SetFocus(bool toOn)
        {
            if (UsingMobileInput)
            {
                // Load secondary font if mobile device
                mMobileInputField.SetFocus(toOn);
            }
            else
            {
                mMobileInputField.pInputField.Focus();
            }
        }

        private bool mShowHideKeyboardLogToggle;

        private void OnKeyboardWillShowHide(bool isShow, int keyboardHeight)
        {
            if (isShow)
            {
                // Hide keyboard can get called multiple times for one 'action', so
                // dont want to spam the console when all I want to see is if the 
                // keyboard is being displayed.
                if (!mShowHideKeyboardLogToggle)
                {
                    mShowHideKeyboardLogToggle = true;
                    AgLogger.Log(
                        $"Keyboard will show with height of {keyboardHeight}");
                }

                if (mKeyboardAvoidance != null)
                {
                    mKeyboardAvoidance.KeyboardWithHeightWillShowForInputRectTransform(
                        keyboardHeight, this.mMobileInputField.GetScreenRectFromVisualElement(this));
                }
            }
            else
            {
                // Hide keyboard can get called multiple times for one 'action', so
                // dont want to spam the console when all I want to see is if the 
                // keyboard is being displayed.
                if (mShowHideKeyboardLogToggle)
                {
                    mShowHideKeyboardLogToggle = false;
                    AgLogger.Log(
                        $"Keyboard will hide with height of {keyboardHeight}");
                }


                if (mKeyboardAvoidance != null)
                    mKeyboardAvoidance.KeyboardWillHide();
            }
        }

        public void ForceEndUserInput()
        {
            SetFocus(false);

            OnKeyboardWillShowHide(false, 0);
        }



        //***************************************************************************
        // Text Changing Methods 
        //***************************************************************************

        public void ResetText()
        {
            Text = String.Empty;
        }

        private bool mInitialisingText;
        public void InitialiseText(
            string text, 
            GDKeyboardAvoidingPanel avoidingPanel)
        {
            mKeyboardAvoidance = avoidingPanel;

            mInitialisingText = true;
            Text = text;
            mInitialisingText = false;
        }
        
        // Predominantly required for removing unwanted characters, and handling the
        // desktop soft keyboard, because every key press removes focus from the input
        // field, and generally dont work well together.
        public void OnTextUpdated(string text)
        {
            if (CommonStrings.RemoveUnwantedCharacters(Text, out var modified))
                Text = modified;

            int characterLimit;
            characterLimit = mMobileInputField.pInputField.maxLength;

            if(mMoveOnWhenLimitReached && 
               characterLimit > 0 && 
               Text.Length == characterLimit)
            {
                if (mNextInputField != null)
                    mNextInputField.SetFocus(true);
                else
                {
                    ForceEndUserInput();
                    OnUserInputFinished?.Invoke();
                }
            }

            OnTextChanged?.Invoke(this);
        }



        //***************************************************************************
        // Always On Top Hack Methods
        //***************************************************************************

        // On iOS, editViews are created in the native plugin and added to the main view
        // controller (i.e. Unity's game view).  This means that UITextFields are always
        // drawn on top.  Which, for the most part, is ok.  However, they can also be
        // part of scroll views, and be under dialog boxes.  This looks terrible, so we
        // have a hack to switch them off when they might be 'under' other UI elements.
        private int mAlwaysOnTopHackCount;

        // We do this, by having our own Unity text view which we use to show whatever
        // text is on the UITextField, and then switch the UITextField off, when its
        // under something, and back on when it isn;t.
        public void ReplaceNativeTextFieldWithUnityView()
        {
            var clearTexts = false;
            // We only want / need to do this for the first time its required
            // (i.e. multiple things can happen at the same time that force the hack),
            if (mAlwaysOnTopHackCount == 0)
            {
                mMobileInputField.pInputField.SetEnabled(false);
                mForceBehindReplacementText.style.display = DisplayStyle.Flex;

                var placeholder =
                    mMobileInputField.pInputField.textEdition.placeholder;

                mMobileHackInputText = mMobileInputField.pInputField.text;

                mForceBehindReplacementText.text = mMobileInputField.pInputField.text;
                mForceBehindReplacementText.style.fontSize = mMobileInputField.resolvedStyle.fontSize;
                mForceBehindReplacementText.style.unityFontStyleAndWeight = mMobileInputField.resolvedStyle.unityFontStyleAndWeight;
                mForceBehindReplacementText.style.color = mMobileInputField.resolvedStyle.color;
                mForceBehindReplacementText.style.unityTextAlign = mMobileInputField.resolvedStyle.unityTextAlign;

                clearTexts = true;
            }

            mAlwaysOnTopHackCount++;

            // Do the above before this.  ALWAYS!
            // We dont set the texts anymore so can probably move this to the above.
            // But make sure you test it on iOS Edit Name / Password and others
            // to make sure.
            if (clearTexts)
                mMobileInputField.pInputField.Blur();
        }

        private void UpdateReplacementText(string text)
        {
            mMobileHackInputText = text;
            mForceBehindReplacementText.text =
                text == ""
                    ? mMobileHackPlaceholderText
                    : text;
        }

        public void ReplaceUnityViewWithNativeTextField()
        {
            if(mAlwaysOnTopHackCount > 0)
            {
                mAlwaysOnTopHackCount--;
                if(mMobileInputField.pInputField != null && mAlwaysOnTopHackCount == 0)
                {
                    mMobileInputField.pInputField.SetEnabled(true);
                    mForceBehindReplacementText.style.display = DisplayStyle.None;
                }
            }
        }
        
        
        
        //***************************************************************************
        // Cleanup
        //***************************************************************************
        
        public void OnDisable()
        {
            mMobileInputField.pInputField.UnregisterCallback<FocusInEvent>(async evt => { });
            mMobileInputField.pInputField.UnregisterCallback<FocusOutEvent>(async evt => { });
            
            // Handle the return key (e.g., "Next" or "Done")
            mMobileInputField.pInputField.UnregisterCallback<KeyDownEvent>(evt => { });
            
            GDMobileInput.OnShowKeyboard -= MobileInput_OnShowKeyboard;
            
            mMobileInputField.pInputField.UnregisterValueChangedCallback(evt => OnTextUpdated(evt.newValue));

            
            SetAsNormal();
        }
        
        
        
        //***************************************************************************
        // Static Utility Methods
        //***************************************************************************

        public static void ForceTextFieldToHaveFocus(TextField textField)
        {
            if (textField != null)
            {
                if (!UsingMobileInput)
                {
                    // Activate focus on the text field
                    textField.Focus();
                }
            }
        }
    }
}

