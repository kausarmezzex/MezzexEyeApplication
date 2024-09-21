using Microsoft.Maui.Controls;
using Microsoft.Maui.LifecycleEvents;
using System.Runtime.InteropServices;
using System.Windows;

#if WINDOWS
using Microsoft.UI.Windowing;
using Microsoft.UI;
using Windows.Graphics;
using WinRT.Interop;
#endif

namespace MezzexEyeApplication
{
    public partial class MainPage : ContentPage
    {
        public MainPage()
        {

#if WINDOWS
            MakeFullScreen();
#endif
        }

#if WINDOWS
        private void MakeFullScreen()
        {
            // Get the current window handle
            var windowHandle = WindowNative.GetWindowHandle(this.Handler.PlatformView as Microsoft.UI.Xaml.Window);

            // Get the AppWindow for the current window
            var appWindow = AppWindow.GetFromWindowId(Win32Interop.GetWindowIdFromWindow(windowHandle));

            // Make the window fullscreen and remove minimize/maximize/close buttons
            appWindow.SetPresenter(AppWindowPresenterKind.FullScreen);
        }
#endif
    }
}
