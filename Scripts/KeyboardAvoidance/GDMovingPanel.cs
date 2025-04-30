using UnityEngine;
using UnityEngine.UIElements;



public class GDMovingPanel
{
    //***************************************************************************
    // Class Properties
    //***************************************************************************

    private float mAnimationTime = 1f;
    private bool mStayActiveWhenHidden;
    private bool mDestroyWhenHidden;
    private bool mPauseBeforeHiding;
    private float mPauseBeforeHideTime;

    private VisualElement mPanel;
    private Vector3 mTargetPosition;
    private bool mIsShowing;
    private bool mAnimate;
    private float mPauseBeforeHidingTimer;
    private float mAnimationTimer;



    //***************************************************************************
    // Getter/Setters
    //***************************************************************************
    public bool pIsAnimating { get; private set; }
    public bool pOnShow { get; private set; }

    // Convenience Setters
    public float pAnimationTime
    {
        set => mAnimationTime = value;
    }

    public bool pStayActiveWhenHidden
    {
        set => mStayActiveWhenHidden = value;
    }



    //***************************************************************************
    // Constructor
    //***************************************************************************

    public GDMovingPanel(VisualElement panel)
    {
        this.mPanel = panel ?? throw new System.ArgumentNullException(nameof(panel));
        pIsAnimating = false;
        pOnShow = false;
    }



    //***************************************************************************
    // Public Methoods
    //***************************************************************************

    public void Show(Vector3 targetPos, bool animate)
    {
        if (!(pIsAnimating && mIsShowing))
        {
            this.mAnimate = animate;
            mTargetPosition = targetPos;
            mIsShowing = true;
            StartSliding(animate);

            if (mPauseBeforeHiding)
                mPauseBeforeHidingTimer = mPauseBeforeHideTime;
        }
    }

    public void Hide(Vector3 targetPos, bool animate)
    {
        if (!(pIsAnimating && !mIsShowing))
        {
            mTargetPosition = targetPos;
            mIsShowing = false;
            StartSliding(animate);
        }
    }

    public void ForceFinishHide()
    {
        if (!mIsShowing)
            CompleteSliding();
    }



    //***************************************************************************
    // Private Methods
    //***************************************************************************

    private void StartSliding(bool animate)
    {
        if (animate)
        {
            mAnimationTimer = mAnimationTime;
            pIsAnimating = true;

            // Use UI Toolkit scheduling system to handle updates
            mPanel.schedule.Execute(PerformAnimation).Every(16).Until(() => !pIsAnimating);
        }
        else
        {
            CompleteSliding();
        }
    }

    private void PerformAnimation()
    {
        if (mAnimationTimer <= 0)
        {
            UnityEngine.Debug.Log("Sliding Completed A");
            CompleteSliding();
            return;
        }

        Vector3 elementPosition = GetElementPosition();

        Vector3 totalDistance = mTargetPosition - elementPosition;
        Vector3 deltaDistance = (totalDistance / mAnimationTimer) * Time.deltaTime;

        if (deltaDistance.magnitude > totalDistance.magnitude)
            deltaDistance = totalDistance;

        SetElementPosition(elementPosition + deltaDistance);
        mAnimationTimer -= Time.deltaTime;
    }

    private void CompleteSliding()
    {
        UnityEngine.Debug.Log("Sliding Completed: " + mTargetPosition);
        pIsAnimating = false;
        pOnShow = mIsShowing;
        SetElementPosition(mTargetPosition);

        if (!pOnShow && mDestroyWhenHidden)
        {
            mPanel.RemoveFromHierarchy(); // Removes the panel from the visual tree
        }
    }

    private Vector3 GetElementPosition()
    {
        float x = mPanel.resolvedStyle.translate.x;
        float y = mPanel.resolvedStyle.translate.y;
        float z = mPanel.transform.position.z;
        return new Vector3(x, y, z);
    }

    private void SetElementPosition(Vector3 position)
    {
        mPanel.style.translate =
            new StyleTranslate(new Translate(new Length(position.x), new Length(position.y)));
    }
}
