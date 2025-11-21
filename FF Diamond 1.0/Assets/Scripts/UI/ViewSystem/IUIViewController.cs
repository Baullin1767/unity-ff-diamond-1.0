namespace UI.ViewSystem
{
    public interface IUIViewController
    {
        void ShowScreen(UIScreenId id);
        void ShowPopup(UIPopupId id, float reward, string result = "");
        void HidePopup(UIPopupId id);
        void HideAllPopups();
        bool TryGetPopupView(UIPopupId id, out PopupUIView view);
    }
}
