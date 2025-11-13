using UnityEngine;

namespace UI.ViewSystem
{
    /// <summary>
    /// Base MonoBehaviour for any view that can be shown/hidden by <see cref="UIViewController"/>.
    /// </summary>
    public abstract class UIView : MonoBehaviour
    {
        public abstract void Show();
        public abstract void Hide();
    }
    public abstract class PopupUIView : MonoBehaviour
    {
        public abstract void Show(int reward, string result);
        public abstract void Hide();
    }
}
