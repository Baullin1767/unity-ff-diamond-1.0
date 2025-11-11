using System;
using UnityEngine;
using Zenject;

namespace UI.ViewSystem
{
    /// <summary>
    /// Entry-point installer that wires up all gameplay systems via Zenject.
    /// Currently only installs the <see cref="UIViewController"/>.
    /// </summary>
    public sealed class GameRootInstaller : MonoInstaller
    {
        [SerializeField] private UIViewController viewController;

        public override void InstallBindings()
        {
            if (!viewController)
                viewController = FindObjectOfType<UIViewController>(includeInactive: true);

            if (!viewController)
                throw new InvalidOperationException($"No {nameof(UIViewController)} assigned to {nameof(GameRootInstaller)}.");

            UIViewInstaller.Install(Container, viewController);
        }
    }
}
