using Zenject;

namespace UI.ViewSystem
{
    /// <summary>
    /// Binds the <see cref="UIViewController"/> instance into the DI container.
    /// </summary>
    public sealed class UIViewInstaller : Installer<UIViewController, UIViewInstaller>
    {
        private readonly UIViewController _controller;

        public UIViewInstaller(UIViewController controller)
        {
            _controller = controller;
        }

        public override void InstallBindings()
        {
            Container.Bind<IUIViewController>()
                .FromInstance(_controller)
                .AsSingle();
        }
    }
}
