namespace UI.ViewSystem
{
    public interface IUIViewController
    {
        void ShowScreen(UIScreenId id);
        void ShowPopup(UIPopupId id);
        void HidePopup(UIPopupId id);
        void HideAllPopups();
    }
}
