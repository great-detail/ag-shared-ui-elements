using System.Collections.Generic;
using AwesomeGolf;
using AwesomeGolf.Common;
using AwesomeGolf.UI;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.UIElements;




[UxmlElement]
public partial class GDKeyboardAvoidingPanel : VisualElement
{
    //***************************************************************************
    // UXML Attribute Properties
    //***************************************************************************
    
    [UxmlAttribute] private float pMarginAroundInput = 20;

    
    
    //***************************************************************************
    // Class Properties
    //***************************************************************************

    private Vector3 mOriginalAnchoredPosition3D;
    private bool mEnabled;
    private GDMovingPanel mPanelMover;
    private VisualElement mPanelElement;
    
    
    
    //***************************************************************************
    // MonoBehaviours / Constructor 
    //***************************************************************************

    public GDKeyboardAvoidingPanel()
    {
        RegisterCallback<AttachToPanelEvent>(evt =>
        {
            OnEnable();
        });
        
        RegisterCallback<DetachFromPanelEvent>(evt => {
            OnDisable();
        });

        mOriginalAnchoredPosition3D = this.resolvedStyle.transformOrigin;
    }
    
    void OnEnable()
    {
        UnregisterCallback<AttachToPanelEvent>(evt =>
        {
            OnEnable();
        });
        
        mEnabled = true;
    }

    private void OnDisable()
    {
        mEnabled = false;
        if (mPanelMover != null && mPanelMover.pIsAnimating && mPanelMover.pOnShow)
            mPanelMover.ForceFinishHide();
    }



    //***************************************************************************
    // Public Methods  
    //***************************************************************************

    public void KeyboardWithHeightWillShowForInputRectTransform(
        int keyboardHeight, Rect inputElement)
    { 
        if (mPanelMover == null)
            mPanelMover = new GDMovingPanel(this);
        
        var screenHeight = Screen.height;
        var inputRect = inputElement;
        
        // Calculate distances
        var inputBottom = screenHeight - inputRect.yMax;
        var inputTop = screenHeight - inputRect.yMin;
        var keyboardTop = keyboardHeight + pMarginAroundInput;
        var rectTop = screenHeight - pMarginAroundInput;
        
        float distanceToMove = 0;

        if ((keyboardTop > inputBottom) && (rectTop < inputTop))
        {
            // If doesn't fit, halve the margin and pin to the top
            rectTop = screenHeight - (pMarginAroundInput * 0.5f);
            distanceToMove = rectTop - inputTop;
        }
        else if (keyboardTop > inputBottom)
        {
            distanceToMove = keyboardTop - inputBottom;
        }
        else if (rectTop < inputTop)
        {
            distanceToMove = rectTop - inputTop;
        }
        
        if (!Mathf.Approximately(distanceToMove, 0))
        {
            var targetPos = new Vector3(this.layout.x, this.layout.y, 0);
            targetPos.y -= distanceToMove;
            mPanelMover.Show(targetPos, true);
        }
    }

    public void KeyboardWillHide()
    {
        if(mPanelMover != null)
            mPanelMover.Hide(mOriginalAnchoredPosition3D, mEnabled); 
    }
}
