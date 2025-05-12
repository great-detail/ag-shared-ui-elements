using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;
using AwesomeGolf.Data;



namespace AwesomeGolf
{
    [UxmlElement]
    public abstract partial class BaseTickButtonSelector : VisualElement
    {
        //***************************************************************************
        // Serialized Properties 
        //***************************************************************************
        
        [UxmlAttribute] protected List<string> pOptionButtons;

        
        
        //***************************************************************************
        // Class Properties 
        //***************************************************************************
        
        protected List<Button> mButtons = new();
        protected Dictionary<Button, VisualElement> mButtonTickMap = new();

        
        
        //***************************************************************************
        // Constants
        //***************************************************************************
        
        protected const string cTickElementID = "Tick";
        protected const string cSelectedClassID = "selected";

        
        
        //***************************************************************************
        // Initialisation
        //***************************************************************************
        
        public virtual void InitialiseButtons()
        {
            mButtons.Clear();
            mButtonTickMap.Clear();

            for (int i = 0; i < pOptionButtons.Count; i++)
            {
                var button = this.Q<Button>(pOptionButtons[i]);
                if (button == null) continue;

                mButtons.Add(button);

                int index = i;
                button.RegisterCallback<ClickEvent>(_ => OnButtonSelected(index));

                var tick = button.Q<VisualElement>(cTickElementID);
                if (tick != null)
                    mButtonTickMap[button] = tick;
            }
        }

        
        
        //***************************************************************************
        // Protected Methods
        //***************************************************************************
        
        protected virtual void ClearSelection()
        {
            foreach (var kvp in mButtonTickMap)
            {
                kvp.Value.style.display = DisplayStyle.None;
                kvp.Key.style.opacity = 1f;
            }

            foreach (var button in mButtons)
            {
                button.RemoveFromClassList(cSelectedClassID);
            }
        }

        protected void MarkSelected(int index)
        {
            if (index < 0 || index >= mButtons.Count)
                return;

            var button = mButtons[index];
            button.AddToClassList(cSelectedClassID);

            if (mButtonTickMap.TryGetValue(button, out var tick))
                tick.style.display = DisplayStyle.Flex;
        }

        
        
        //***************************************************************************
        // Abstract Methods
        //***************************************************************************
        
        protected abstract void OnButtonSelected(int index);
    }
}
