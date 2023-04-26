using System;
using System.Windows;
using System.Threading;
using System.Security.Principal;
using System.Security.AccessControl;

using Forms = System.Windows.Forms;

using TrayAppWithMutexWpf.Classes;

namespace TrayAppWithMutexWpf {

    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App : Application {

        //Uygulama çalıştığı müddetçe system tray'de bulunacak olan NotifyIcon'u tanımladık.
        private Forms.NotifyIcon _trayIcon;

        //Uygulama için bir Mutex tanımladık.
        private Mutex _mutex;

        protected override void OnExit(ExitEventArgs e) {

            base.OnExit(e);

            //Mutex serbest bırakılıyor.
            _mutex.ReleaseMutex();

            //System tray'deki Notify Icon gizleniyor.
            _trayIcon.Visible = false;

        }

        protected override void OnStartup(StartupEventArgs e) {

            base.OnStartup(e);

            //Window'u Hide() methodu ile gizlediğimizde uygulama kapanmasın.
            ShutdownMode = ShutdownMode.OnExplicitShutdown;

            //Mutex'i hazırlıyoruz.
            InitMutex();
            try {
                //Mutex'in kontrolünü yapıyoruz.
                CheckMutex();
            } catch (AbandonedMutexException) {
                //AbandonedMutexException durumu yardımıyla Mutex'in terk edildiğini anlayıp onu release edebiliyoruz.
                //Eğer bu işlemi yapmazsak, uygulama Mutex ile ilgili beklenmedik bir hata verip kapanacaktır.
                //Mutex terk edildiği için, true parametresiyle onu release ederek işleme devam ediyoruz.
                CheckMutex(true);
            } catch (Exception ex) {
                //Farklı bir hata varsa MessageBox vasıtasıyla bildiriyoruz.
                _ = MessageBox.Show(ex.Message, "Hata", MessageBoxButton.OK, MessageBoxImage.Error);
            }

        }

        private void InitMutex() {

            //AssemblyInfo.cs üzerindeki GUID'i aldık.
            //Bu yöntem WPF üzerinde işe yaramaz.
            //Sıfırdan oluşturulan WPF projelerindeki AssemblyInfo.cs dosyası üzerinde "[assembly: Guid("{RANDOM_GUID_VALUE}")]" bölümü bulunmaz.
            //Dolayısıyla buraya string tipinde el ile rastgele bir değer girilmelidir.
            //string appGuid = ((GuidAttribute)Assembly.GetExecutingAssembly().GetCustomAttributes(typeof(GuidAttribute), false).GetValue(0)).Value;

            // AssemblyInfo.cs üzerindeki GUID'i aldık.
            string appGuid = "5fdc9494-cd5d-4554-b333-fec95f7b0130";

            // GUID'i global Mutex ID'ye çevirdik.
            string mutexId = string.Format("Global\\{{{0}}}", appGuid);

            //Uygulama için bir Mutex tanımladık.
            _mutex = new Mutex(false, mutexId);

            //Mutex'in erişim yetkilerini belirledik.
            MutexAccessRule allowEveryoneRule = new MutexAccessRule(new SecurityIdentifier(WellKnownSidType.WorldSid, null), MutexRights.FullControl, AccessControlType.Allow);
            MutexSecurity securitySettings = new MutexSecurity();
            securitySettings.AddAccessRule(allowEveryoneRule);
            _mutex.SetAccessControl(securitySettings);
        }

        private void CheckMutex(bool isMutexAbandoned = false) {
            if (isMutexAbandoned) {
                //Mutex terk edilmiş. Dolayısıyla onu release ediyoruz.
                _mutex.ReleaseMutex();
            }
            if (_mutex.WaitOne(TimeSpan.Zero, false)) {
                //Aynı Mutex tespit edilemedi.
                //Dolayısıyla uygulama sıfırdan başlatılıyor.

                //NotifyIcon'un niteliklerini belirliyoruz.
                _trayIcon = new Forms.NotifyIcon {
                    //NotifyIcon'un üzerinde fare imleci bekletildiğinde görünecek yazıyı (tooltip) tanımladık.
                    Text = "System Tray",

                    //NotifyIcon'un simgesini tanımladık.
                    Icon = System.Drawing.Icon.FromHandle(TrayAppWithMutexWpf.Properties.Resources.protect.Handle)
                };

                //NotifyIcon'un üzerine çift tıklandığında gerçekleşecek bir event tanımladık.
                _trayIcon.DoubleClick += (o, e) => {
                    //NotifyIcon'a çift tıklandı.
                    //Hali hazırda çalışmakta olan uygulama ekrana getiriliyor.
                    Utils.ShowActiveWindow();
                };

                //NotifyIcon'a sağ tıklandığında açılan menünün elemanları tanımlanıyor.
                _trayIcon.ContextMenu = new Forms.ContextMenu(new Forms.MenuItem[] {
                    new Forms.MenuItem("Göster", (s, e) => {
                        //Hali hazırda çalışmakta olan uygulamayı ekrana getirir.
                        Utils.ShowActiveWindow();
                    }),
                    new Forms.MenuItem("Kapat", (s, e) => {
                        //Uygulamayı kapatır.
                        Current.Shutdown();
                    }),
                });

                //System tray'deki NotifyIcon'umuz görünür duruma getiriliyor.
                _trayIcon.Visible = true;

                Current.StartupUri = new Uri("Views/WndMainWindow.xaml", UriKind.Relative);

            } else {
                //Aynı Mutex tespit edildi.
                _ = MessageBox.Show("Bu uygulama şu anda zaten çalışmaktadır. Mesaj kapatıldığında çalışmakta olan uygulama görüntülenecektir.", "Bilgi", MessageBoxButton.OK, MessageBoxImage.Warning);
                //Hali hazırda çalışmakta olan uygulama görüntüleniyor.
                Utils.ShowActiveWindow();
                Environment.Exit(0);
            }

        }
    }
}