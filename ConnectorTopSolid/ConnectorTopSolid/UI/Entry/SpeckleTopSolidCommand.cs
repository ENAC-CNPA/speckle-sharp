using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Runtime.InteropServices;
using System.Threading;

using Avalonia;
using Avalonia.Controls;
using Avalonia.ReactiveUI;

using DesktopUI2;
using DesktopUI2.ViewModels;
using DesktopUI2.Views;
using Speckle.ConnectorTopSolid.UI;
using Application = TopSolid.Kernel.UI.Application;
using Avalonia.Collections;


namespace Speckle.ConnectorTopSolid.UI.Entry
{
    class SpeckleTopSolidCommand
    {
        #region Avalonia parent window
        [DllImport("user32.dll", SetLastError = true)]
        static extern IntPtr SetWindowLongPtr(IntPtr hWnd, int nIndex, IntPtr value);
        const int GWL_HWNDPARENT = -8;
        private const UInt32 DLGC_WANTARROWS = 0x0001;
        private const UInt32 DLGC_HASSETSEL = 0x0008;
        private const UInt32 DLGC_WANTCHARS = 0x0080;
        private const UInt32 WM_GETDLGCODE = 0x0087;
        #endregion

        private static Avalonia.Application AvaloniaApp { get; set; }
        public static Window MainWindow { get; private set; }
        private static CancellationTokenSource Lifetime = null;
        public static ConnectorBindingsTopSolid Bindings { get; set; }

        public static AppBuilder BuildAvaloniaApp() => AppBuilder.Configure<DesktopUI2.App>()
          .UsePlatformDetect()
          .With(new SkiaOptions { MaxGpuResourceSizeBytes = 8096000 })
          .With(new Win32PlatformOptions { AllowEglInitialization = true, EnableMultitouch = false })
          .LogToTrace()
          .UseReactiveUI();


        public static void SpeckleCommand()
        {
            CreateOrFocusSpeckle();
        }

        public static void InitAvalonia()
        {
            BuildAvaloniaApp().Start(AppMain, null);
        }

        /// <summary>
        /// WPF was handling all the text input events and they where not being passed to the Avalonia control
        /// This ensures they are passed, see: https://github.com/AvaloniaUI/Avalonia/issues/8198#issuecomment-1168634451
        /// </summary>
        private IntPtr AvaloniaHost_MessageHook(IntPtr hwnd, int msg, IntPtr wParam, IntPtr lParam, ref bool handled)
        {
            if (msg != WM_GETDLGCODE) return IntPtr.Zero;
            handled = true;
            return new IntPtr(DLGC_WANTCHARS | DLGC_WANTARROWS | DLGC_HASSETSEL);
        }

        public static void CreateOrFocusSpeckle(bool showWindow = true)
        {

            TopSolid.Kernel.SX.UI.Application.IsMouseWheelInterceptedByGraphics = false; // TODO : Charger true lors de la fermeture

            if (Bindings == null) {
                App newApp = new App();
                newApp.Initialize();
            }

            if (MainWindow == null)
            {
                var viewModel = new MainViewModel(Bindings);
                MainWindow = new MainWindow
                {
                    DataContext = viewModel
                };
            }

            try
            {
                if (showWindow)
                {
                    MainWindow.Show();
                    MainWindow.Activate();

                    //required to gracefully quit avalonia and the skia processes
                    //https://github.com/AvaloniaUI/Avalonia/wiki/Application-lifetimes
                    if (Lifetime == null)
                    {
                        Lifetime = new CancellationTokenSource();
                        Task.Run(() => AvaloniaApp.Run(Lifetime.Token));
                    }

                    if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
                    {
                        var parentHwnd = Application.ActiveDocumentWindow.Handle;
                        var hwnd = MainWindow.PlatformImpl.Handle.Handle;
                        SetWindowLongPtr(hwnd, GWL_HWNDPARENT, parentHwnd);
                    }
                }
            }
            catch { }
        }

        private static void AppMain(Avalonia.Application app, string[] args)
        {
            AvaloniaApp = app;
        }


    }
}
