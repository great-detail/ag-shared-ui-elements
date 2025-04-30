using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;

namespace AwesomeGolf.UI.Utilities
{
    /// <summary>
    /// An AG modified safe area script originally found at
    /// https://github.com/artstorm/ui-toolkit-safe-area (MIT License)
    /// </summary>
    [UxmlElement]
    public partial class SafeArea : VisualElement
    {
        private struct Offset
        {
            public float Left, Right, Top, Bottom;
        }
        
        
        
        //***************************************************************************
        // Public Variables
        //***************************************************************************
        
        [UxmlAttribute] public bool CollapseMargins { get; set; }
        [UxmlAttribute] public bool ExcludeLeft { get; set; }
        [UxmlAttribute] public bool ExcludeRight { get; set; }
        [UxmlAttribute] public bool ExcludeTop { get; set; }
        [UxmlAttribute] public bool ExcludeBottom { get; set; }

        
        
        //***************************************************************************
        // Getter/Setters
        //***************************************************************************
        
        private VisualElement _contentContainer;
        public override VisualElement contentContainer {
            get => _contentContainer;
        }

        
        
        //***************************************************************************
        // Constructor
        //***************************************************************************
        
        public SafeArea()
        {
            // By using absolute position instead of flex to fill the full screen,
            // SafeArea containers can be stacked.
            style.position = Position.Absolute;
            style.top = 0;
            style.bottom = 0;
            style.left = 0;
            style.right = 0;
            pickingMode = PickingMode.Ignore;
            
            _contentContainer = new VisualElement();
            _contentContainer.name = "safe-area-content-container";
            _contentContainer.pickingMode = PickingMode.Ignore;
            _contentContainer.style.flexGrow = 1;
            _contentContainer.style.flexShrink = 0;
            hierarchy.Add(_contentContainer);

            RegisterCallback<GeometryChangedEvent>(OnGeometryChanged);
        }

        
        
        //***************************************************************************
        // Handle Callbacks
        //***************************************************************************
        
        private void OnGeometryChanged(GeometryChangedEvent evt)
        {
            // Prevent it from running in UIBuilder
#if UNITY_EDITOR
            if (EditorApplication.isPlaying)
            {
                ApplySafeAreaAndMargin();
            }
#else
            ApplySafeAreaAndMargin();
#endif
        }

        
        
        //***************************************************************************
        // Private Methods
        //***************************************************************************
        
        private void ApplySafeAreaAndMargin()
        {
            var safeArea = GetSafeAreaOffset();
            var margin = GetMarginOffset();

            if (CollapseMargins)
            {
                _contentContainer.style.marginLeft = Mathf.Max(margin.Left, safeArea.Left) - margin.Left;
                _contentContainer.style.marginRight = Mathf.Max(margin.Right, safeArea.Right) - margin.Right;
                _contentContainer.style.marginTop = Mathf.Max(margin.Top, safeArea.Top) - margin.Top;
                _contentContainer.style.marginBottom = Mathf.Max(margin.Bottom, safeArea.Bottom) - margin.Bottom;
            }
            else
            {
                _contentContainer.style.marginLeft = safeArea.Left;
                _contentContainer.style.marginRight = safeArea.Right;
                _contentContainer.style.marginTop = safeArea.Top;
                _contentContainer.style.marginBottom = safeArea.Bottom;
            }
        }

        private Offset GetSafeAreaOffset()
        {
            var safeArea = Screen.safeArea;
            var leftTop = RuntimePanelUtils.ScreenToPanel(
                panel, 
                new Vector2(safeArea.xMin, Screen.height - safeArea.yMax));
            var rightBottom = RuntimePanelUtils.ScreenToPanel(
                panel, 
                new Vector2(Screen.width - safeArea.xMax, safeArea.yMin));

            // If an edge has been marked as excluded, set that edge to 0.
            return new Offset()
            {
                Left = ExcludeLeft ? 0 : leftTop.x,
                Right = ExcludeRight ? 0 : rightBottom.x,
                Top = ExcludeTop ? 0 : leftTop.y,
                Bottom = ExcludeBottom ? 0 : rightBottom.y
            };
        }

        private Offset GetMarginOffset()
        {
            return new Offset()
            {
                Left = resolvedStyle.marginLeft,
                Right = resolvedStyle.marginRight,
                Top = resolvedStyle.marginTop,
                Bottom = resolvedStyle.marginBottom,
            };
        }
    }
}