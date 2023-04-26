using System;
using System.Windows;
using System.ComponentModel;
using System.Windows.Interop;

using TrayAppWithMutexWpf.Classes;

namespace TrayAppWithMutexWpf.Views {

    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class WndMainWindow : Window {

        private HwndSource _source;

        public WndMainWindow() {
            InitializeComponent();
            //Icon = BitmapFrame.Create(new Uri("pack://application:,,,/TrayAppWithMutexWpf;component/Resources/protect.ico"));
        }

        //WinApi mesajlarını dinleyebilmek için hook ekliyoruz.
        protected override void OnSourceInitialized(EventArgs e) {
            base.OnSourceInitialized(e);
            _source = (HwndSource)PresentationSource.FromVisual(this);
            _source.AddHook(WndProc);
        }

        //WinApi mesajları burada işleniyor.
        public IntPtr WndProc(IntPtr hwnd, int msg, IntPtr wParam, IntPtr lParam, ref bool handled) {

            if (msg == NativeMethods.WM_SHOWME) {
                //Form üzerindeki WinAPI mesajı WM_SHOWME değerine eşitse formu gösteriyoruz.
                Utils.GlobalActivateWindow(this);
            }

            if (msg == NativeMethods.WM_SYSCOMMAND) {
                if (wParam.ToInt32() == NativeMethods.SC_MINIMIZE) {
                    //Form üzerindeki WinAPI mesajı WM_SYSCOMMAND, parametresi de SC_MINIMIZE değerine eşitse form kullanıcı tarafından simge durumuna küçültülüyor demektir.
                    //Formun system tray'e taşınmasını sağlayan ComboBox işaretli olduğu için formu simge durumuna küçültme işlemini iptal edip sadece gizliyoruz.
                    Hide();
                    //Simge durumuna küçültme işlemini iptal ediyoruz.
                    return IntPtr.Zero;
                }
            }

            return IntPtr.Zero;
        }

        //Programın kapanma işlemini iptal edip sadece gizliyoruz.
        protected override void OnClosing(CancelEventArgs e) {
            if (WindowState != WindowState.Minimized) {
                Hide();
                e.Cancel = true;
            }
        }
    }
}