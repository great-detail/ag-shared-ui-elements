using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;



namespace AwesomeGolf
{
    [UxmlElement]
    public abstract partial class BaseGenericButtonSelector<T> : VisualElement
    {
        //***************************************************************************
        // Serialized Properties 
        //***************************************************************************
        
        [UxmlAttribute] protected List<string> pOptionButtons;
        [UxmlAttribute] protected List<T> pValidOptions;

        
        
        //***************************************************************************
        // Class Properties 
        //***************************************************************************
        
        protected List<Button> mOptionButtons = new();
        protected Dictionary<Button, T> mButtonOptionMap = new();

        
        
        //***************************************************************************
        // Constants
        //***************************************************************************
        
        protected const string cSelectedClassID = "selected";
        protected const string cSelectedFillID = "SelectedFill";
        protected const string cSelectedFillClassID = "selected-fill";
        
        
        
        //***************************************************************************
        // Getters
        //***************************************************************************
        
        public T SelectedOption { get; private set; }
        
        
        
        //***************************************************************************
        // Initialisation
        //***************************************************************************
        
        public virtual void Initialise()
        {
            if (!Application.isPlaying || pOptionButtons == null || pValidOptions == null)
                return;

            mOptionButtons.Clear();
            mButtonOptionMap.Clear();

            for (int i = 0; i < pOptionButtons.Count && i < pValidOptions.Count; i++)
            {
                var button = this.Q<Button>(pOptionButtons[i]);
                if (button == null || mButtonOptionMap.ContainsKey(button)) continue;

                var option = pValidOptions[i];
                mOptionButtons.Add(button);
                mButtonOptionMap[button] = option;

                button.RegisterCallback<ClickEvent>(_ => OnOptionSelected(option));
            }
        }

        protected virtual void OnOptionSelected(T selected)
        {
            SetSelection(selected);
        }

        
        
        //***************************************************************************
        // Public Methods
        //***************************************************************************
        
        public void SetInitialSelection(T option)
        {
            SetSelection(option);
        }

        
        
        //***************************************************************************
        // Protected Methods
        //***************************************************************************
        
        protected virtual void SetSelection(T selected)
        {
            if (!pValidOptions.Contains(selected))
                return;

            foreach (var button in mOptionButtons)
            {
                button.RemoveFromClassList(cSelectedClassID);
                var fill = button.Q<VisualElement>(cSelectedFillID);
                fill?.RemoveFromClassList(cSelectedFillClassID);
            }

            foreach (var kvp in mButtonOptionMap)
            {
                if (EqualityComparer<T>.Default.Equals(kvp.Value, selected))
                {
                    kvp.Key.AddToClassList(cSelectedClassID);
                    var fill = kvp.Key.Q<VisualElement>(cSelectedFillID);
                    fill?.AddToClassList(cSelectedFillClassID);
                    break;
                }
            }

            SelectedOption = selected;
        }
    }
}
