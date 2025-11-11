using UnityEngine;

namespace UI
{
    /// <summary>
    /// View wrapper for screens located under BaseUI canvas.
    /// </summary>
    public sealed class BaseUIScreenView : UIView
    {
        [SerializeField] private BaseUIScreenId id = BaseUIScreenId.Menu;

        public BaseUIScreenId Id => id;
    }

    /// <summary>
    /// View wrapper for popups located under PopupUI canvas.
    /// </summary>
    public sealed class PopupView : UIView
    {
        [SerializeField] private PopupId id = PopupId.ConnectionError;

        public PopupId Id => id;
    }
}
