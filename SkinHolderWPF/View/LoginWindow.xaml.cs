using System.IO;
using System.Text.Json;
using System.Windows;
using System.Windows.Input;
using System.Windows.Media.Imaging;
using SkinHolderWPF.Enums.Api;
using SkinHolderWPF.ViewModel;

namespace SkinHolderWPF.View
{
    /// <summary>
    /// Lógica de interacción para LoginWindow.xaml
    /// </summary>
    public partial class LoginWindow
    {
        public LoginWindow()
        {
            InitializeComponent();
            Logo.Source = new BitmapImage(new Uri(App.ExecutionPath + "/Images/icono.png"));
            CargarUltimoUsername();
        }

        private void IniciarSesionButton_Click(object sender, RoutedEventArgs e)
        {
            ErrorInicioSesionTextBlock.Text = "";
            ProcesarInicioSesion();
        }

        private void NuevoUsuarioButton_Click(object sender, RoutedEventArgs e)
        {
            MessageBox.Show("La aplicación sigue en desarrollo. Para acceder a la Alpha puedes contactarme a Discord: jagobainda#5551");
            // TODO: Hacer la lógica del register
        }

        private void TextBox_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key != Key.Enter) return;

            ErrorInicioSesionTextBlock.Text = "";
            ProcesarInicioSesion();
        }

        private void CargarUltimoUsername()
        {
            if (!File.Exists(App.ExecutionPath + "last_username.json")) return;

            var jsonString = File.ReadAllText("last_username.json");
            var data = JsonSerializer.Deserialize<JsonElement>(jsonString);
            UsernameTextBox.Text = data.GetProperty("last_username").GetString()!;
        }

        private async void ProcesarInicioSesion()
        {
            var userViewModel = new UserViewModel
            {
                UserName = UsernameTextBox.Text.Trim(),
                Password = PasswordTextBox.Password.Trim()
            };

            var json = await ApiInfo.GetJsonFromPost(ApiInfo.BaseUsersUrl + EUsersApiMethods.Login, userViewModel);

            if (json == "true")
            {
                GuardarUsername();

                App.CurrentUserViewModel = new UserViewModel()
                {
                    UserName = UsernameTextBox.Text.Trim(),
                    Password = PasswordTextBox.Password.Trim()
                };

                var menuPrincipal = new MainWindow();
                menuPrincipal.Show();

                Close();

                return;
            }

            ErrorInicioSesionTextBlock.Text = "Usuario o contraseña incorrectos";
        }

        private void GuardarUsername()
        {
            var data = new { last_username = UsernameTextBox.Text.Trim() };
            var jsonString = JsonSerializer.Serialize(data);

            File.WriteAllText(App.ExecutionPath + "last_username.json", jsonString);
        }
    }
}
