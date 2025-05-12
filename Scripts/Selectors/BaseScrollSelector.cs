using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;



namespace AwesomeGolf
{
    [UxmlElement]
    public abstract partial class BaseScrollSelector<T> : VisualElement
    {
        //***************************************************************************
        // Serialized Properties 
        //***************************************************************************
        
        [UxmlAttribute] protected List<T> pOptions;
        
        
        
        //***************************************************************************
        // Class Properties 
        //***************************************************************************

        // Visual Elements
        protected VisualElement mCurrentOptionIcon;
        protected Button mLeftButton;
        protected Button mRightButton;

        // Generic
        protected int mCurrentIndex;
        

        
        //***************************************************************************
        // Constants
        //***************************************************************************

        private const string cIconElementID = "Icon";
        private const string cLeftButtonID = "LeftButton";
        private const string cRightButtonID = "RightButton";
        private const string cCurrentOptionIconID = "CurrentOptionIcon";

        
        
        //***************************************************************************
        // Getters
        //***************************************************************************
        
        public T pSelectedOption { get; protected set; }

        
        
        
        //***************************************************************************
        // Initialisation
        //***************************************************************************

        public virtual void Initialise()
        {
            mCurrentOptionIcon = this.Q<VisualElement>(cCurrentOptionIconID);

            mLeftButton = this.Q<Button>(cLeftButtonID);
            mLeftButton.RegisterCallback<ClickEvent>(_ => ArrowButtonPressed(-1));

            mRightButton = this.Q<Button>(cRightButtonID);
            mRightButton.RegisterCallback<ClickEvent>(_ => ArrowButtonPressed(1));
        }

        public void SetInitialOption(T initialSelection)
        {
            mCurrentIndex = pOptions.IndexOf(initialSelection);
            mCurrentIndex = Mathf.Clamp(mCurrentIndex, 0, pOptions.Count - 1);
            UpdateSelected();
        }

        
        
        //***************************************************************************
        // Public Methods
        //***************************************************************************

        public void ArrowButtonPressed(int delta)
        {
            if (pOptions.Count == 0)
            {
                AgLogger.LogE("No valid options have been set for this setting");
                return;
            }

            mCurrentIndex = Mathf.Clamp(mCurrentIndex + delta, 0, pOptions.Count - 1);
            UpdateSelected();
        }

        
        
        //***************************************************************************
        // Private Methods
        //***************************************************************************
        
        private void UpdateSelected()
        {
            pSelectedOption = pOptions[mCurrentIndex];
            UpdateUI(mCurrentIndex);
            UpdateButtons();
        }

        private void UpdateButtons()
        {
            bool canGoLeft = mCurrentIndex > 0;
            bool canGoRight = mCurrentIndex < pOptions.Count - 1;

            mLeftButton.pickingMode = canGoLeft ? PickingMode.Position : PickingMode.Ignore;
            mRightButton.pickingMode = canGoRight ? PickingMode.Position : PickingMode.Ignore;

            SetIconOpacity(mLeftButton, canGoLeft);
            SetIconOpacity(mRightButton, canGoRight);
        }

        private void SetIconOpacity(VisualElement button, bool isEnabled)
        {
            var icon = button.Q<VisualElement>(cIconElementID);
            if (icon != null)
            {
                icon.style.opacity = isEnabled ? 1f : 0.5f;
            }
        }

        
        
        //***************************************************************************
        // Abstract Methods
        //***************************************************************************

        protected abstract void UpdateUI(int selectedIndex);
    }
}
