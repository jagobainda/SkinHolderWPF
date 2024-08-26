using System.Text.Json;
using System.Windows;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using SkinHolderWPF.Enums.Api;
using SkinHolderWPF.Utils;
using SkinHolderWPF.ViewModel;

namespace SkinHolderWPF.View;

/// <summary>
/// Interaction logic for MainWindow.xaml
/// </summary>
public partial class MainWindow
{
    private readonly Brush _originalPingForegroundBrush;

    private readonly Brush _failBrush = new SolidColorBrush(Colors.DarkRed);
    public MainWindow()
    {
        InitializeComponent();
        
        Icon = new BitmapImage(new Uri(App.ExecutionPath + "/Images/icono.png"));

        _originalPingForegroundBrush = SteamConnectionPing.Foreground;

        GetPings();
        GetLastRegistroPrecioTotal();
    }

    private void BtnRegistrosMenu_Click(object sender, RoutedEventArgs e)
    {
        ContenedorUserControl.Content = ContenedorUserControl.Content is RegistrosMenu ? new Bienvenida() : new RegistrosMenu();

        App.GlobalRegistrosMenu = ContenedorUserControl.Content as RegistrosMenu;
    }

    private void BtnItemsMenu_Click(object sender, RoutedEventArgs e)
    {

    }

    private void BtnUserProfile_Click(object sender, RoutedEventArgs e)
    {

    }

    private void BtnExit_Click(object sender, RoutedEventArgs e)
    {
        Application.Current.Shutdown();
    }

    private void GetPings()
    {
        Task.Run(async () =>
        {
            var c = 0;
            
            while (true)
            {
                if (c > 4) return;

                var pingSteam = ConnectionPing.GetPingTime("https://store.steampowered.com");
                var pingGamerPay = ConnectionPing.GetPingTime("https://gamerpay.gg");
                var pingSkinHolderDb = ConnectionPing.GetPingTime(ApiInfo.ApiHostProd[..^1]);

                if (pingSteam == -1 || pingGamerPay == -1 || pingSkinHolderDb == -1) c++; 
                
                Dispatcher.Invoke(() =>
                {
                    SteamConnectionPing.Text = pingSteam.ToString();
                    SteamConnectionPing.Foreground = pingSteam > -1 ? App.PrimaryColor : _failBrush;
                    App.SteamActivo = pingSteam > -1;
                    Task.Delay(1000).ContinueWith(_ => Dispatcher.Invoke(() => SteamConnectionPing.Foreground = _originalPingForegroundBrush));
                });
                
                Dispatcher.Invoke(() =>
                {
                    GamerPayConnectionPing.Text = pingGamerPay.ToString();
                    GamerPayConnectionPing.Foreground = pingGamerPay > -1 ? App.PrimaryColor : _failBrush;
                    App.GamerPayActivo = pingGamerPay > -1;
                    Task.Delay(1000).ContinueWith(_ => Dispatcher.Invoke(() => GamerPayConnectionPing.Foreground = _originalPingForegroundBrush));
                });
                
                Dispatcher.Invoke(() =>
                {
                    App.SkinHolderDbActivo = pingSkinHolderDb > -1;

                    SkinHolderDbConnectionPing.Text = pingSkinHolderDb.ToString();
                    SkinHolderDbConnectionPing.Foreground = App.PrimaryColor;

                    EstadoConSkinHolderDb.Text = App.SkinHolderDbActivo ? "ACTIVO" : "INACTIVO";
                    EstadoConSkinHolderDb.Foreground = App.SkinHolderDbActivo ? App.PrimaryColor : _failBrush;
                    
                    Task.Delay(1000).ContinueWith(_ => Dispatcher.Invoke(() =>
                    {
                        SkinHolderDbConnectionPing.Foreground = _originalPingForegroundBrush;
                        EstadoConSkinHolderDb.Foreground = _originalPingForegroundBrush;
                    }));
                });
                
                await Task.Delay(10000); 
            }
        });
    }

    private void GetLastRegistroPrecioTotal()
    {
        Task.Run(async () =>
        {
            var registroJson = await ApiInfo.GetJsonFromPost(ApiInfo.BaseRegistrosUrl + ERegistrosApiMethods.GetLastRegistro, "1");

            using var registroDoc = JsonDocument.Parse(registroJson);
            var json = registroDoc.RootElement;

            PreciosViewModel preciosViewModel = new()
            {
                PrecioSteam = json.GetProperty("totalSteam").GetDouble(),
                PrecioGamerPay = json.GetProperty("totalGamerPay").GetDouble()
            };

            Dispatcher.Invoke(() =>
            {
                SteamLast.Text = preciosViewModel.PrecioSteam.Value.ToString("F2");
                GamerPayLast.Text = preciosViewModel.PrecioGamerPay.Value.ToString("F2");
            });
        });
    }
}