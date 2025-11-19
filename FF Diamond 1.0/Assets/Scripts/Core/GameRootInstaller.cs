using System;
using Core;
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
        [SerializeField] private IpadChecker ipadChecker;
        [SerializeField] private UIViewController viewControllerIphone;
        [SerializeField] private UIViewController viewControllerIpad;
        private UIViewController _viewController;

        private void Awake()
        {
            ipadChecker ??= FindAnyObjectByType<IpadChecker>();
        }

        public override void InstallBindings()
        {
            UIViewInstaller.Install(Container, 
                ipadChecker.IsIPad() ? viewControllerIpad : viewControllerIphone);
        }
    }
}
