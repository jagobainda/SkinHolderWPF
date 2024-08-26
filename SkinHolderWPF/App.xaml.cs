using System.Windows.Media;
using SkinHolderWPF.View;
using SkinHolderWPF.ViewModel;

namespace SkinHolderWPF;

/// <summary>
/// Interaction logic for App.xaml
/// </summary>
public partial class App
{
    public static SolidColorBrush PrimaryColor = new((Color) ColorConverter.ConvertFromString("#B8D600"));

    public static string ExecutionPath = AppDomain.CurrentDomain.BaseDirectory;

    public static UserViewModel CurrentUserViewModel = new() { UserName = "", Password = "" };

    public static bool SteamActivo;
    public static bool GamerPayActivo;
    public static bool SkinHolderDbActivo;

    // Acceso global a UserControls
    public static RegistrosMenu? GlobalRegistrosMenu;
}