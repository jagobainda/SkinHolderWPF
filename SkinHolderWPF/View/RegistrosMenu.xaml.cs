using System.Text.Json;
using System.Windows;
using SkinHolderWPF.Enums.Api;
using SkinHolderWPF.ViewModel;

namespace SkinHolderWPF.View;

/// <summary>
/// Lógica de interacción para RegistrosMenu.xaml
/// </summary>
public partial class RegistrosMenu
{
    public RegistroViewModel RegistroActualViewModel;

    public List<ItemPrecioViewModel> ItemPrecioViewModelsList;

    public RegistrosMenu()
    {
        InitializeComponent();

        ResetearObjetosRegistro();
    }

    public void ResetearObjetosRegistro()
    {
        ItemPrecioViewModelsList = [];

        RegistroActualViewModel = new RegistroViewModel
        {
            RegistroId = 0,
            FechaHora = DateTime.MinValue,
            TotalSteam = 0,
            TotalGamerPay = 0,
            UserId = 0,
            RegistroTypeId = 0
        };
    }

    private void CambiarEstadoBotones(bool estado)
    {
        BtnConsultarSteam.IsEnabled = estado;
        BtnConsultarGamerPay.IsEnabled = estado;
        BtnConsultarAmbos.IsEnabled = estado;
        BtnConsultarHistorialRegistros.IsEnabled = estado;
        BtnExportarRegistrosJson.IsEnabled = estado;

        if (Window.GetWindow(this) is not MainWindow window) return;

        window.BtnRegistrosMenu.IsEnabled = estado;
        window.BtnItemsMenu.IsEnabled = estado;
        window.BtnUserProfile.IsEnabled = estado;
        window.BtnExit.IsEnabled = estado;
    }

    private void BtnConsultarSteam_Click(object sender, RoutedEventArgs e)
    {

    }

    private async void BtnConsultarGamerPay_Click(object sender, RoutedEventArgs e)
    {
        try
        {
            CambiarEstadoBotones(false);

            ResetearObjetosRegistro();

            if (!await GenerarItemPrecios()) return;

            _ = await GenerarRegistroGamerPay();
        }
        catch (Exception ex)
        {
            // TODO: log
        }
        finally
        {
            CambiarEstadoBotones(true);
        }
    }


    private async void BtnConsultarAmbos_Click(object sender, RoutedEventArgs e)
    {
        try
        {
            CambiarEstadoBotones(false);

            ResetearObjetosRegistro();

            if (!await GenerarItemPrecios()) return;

            var generarGamerPayTask = GenerarRegistroGamerPay();
            var generarSteamTask = GenerarRegistroSteam();

            await Task.WhenAll(generarGamerPayTask, generarSteamTask);
        }
        catch (Exception ex)
        {
            // TODO: log
        }
        finally
        {
            CambiarEstadoBotones(true);
        }
    }


    private void BtnConsultarHistorialRegistros_Click(object sender, RoutedEventArgs e)
    {

    }

    private void BtnExportarRegistrosJson_Click(object sender, RoutedEventArgs e)
    {
        MessageBox.Show("No disponible todavía");
    }

    public async Task<bool> GenerarItemPrecios()
    {
        var jsonUserItems = await ApiInfo.GetJsonFromPost(ApiInfo.BaseUserItemsUrl + EUserItemsApiMethods.GetItemsFromUser, App.CurrentUserViewModel);
        using var docUserItems = JsonDocument.Parse(jsonUserItems);

        var items = docUserItems.RootElement.EnumerateArray().ToList();

        if (items.Any())
        {
            RegistroActualViewModel.UserId = items.First().GetProperty("user").GetProperty("userId").GetInt32();
        }

        var itemPrecioViewModels = items.Select(userItem => new ItemPrecioViewModel
        {
            ItemPrecioId = 0,
            PrecioSteam = 0,
            PrecioGamerPay = 0,
            UserItemId = userItem.GetProperty("userItemId").GetInt32(),
            RegistroId = 0,
            Cantidad = userItem.GetProperty("cantidad").GetInt32(),
            SteamHashName = userItem.GetProperty("item").GetProperty("hashNameSteam").GetString()!,
            GamerPayNombre = userItem.GetProperty("item").GetProperty("gamerPayNombre").GetString()!
        }).ToList();

        ItemPrecioViewModelsList.AddRange(itemPrecioViewModels);

        return true;
    }

    public async Task<bool> GenerarRegistroGamerPay()
    {
        var jsonPreciosGamerPay = await ApiInfo.GetJsonFromPost(ApiInfo.BaseRegistrosUrl + ERegistrosApiMethods.GetRegistroGamerPay, App.CurrentUserViewModel);

        using var docPreciosGamerPay = JsonDocument.Parse(jsonPreciosGamerPay);

        var preciosGamerPayDict = docPreciosGamerPay.RootElement
            .EnumerateArray()
            .ToDictionary(pg => pg.GetProperty("nombre").GetString()!, pg => (float)pg.GetProperty("precio").GetDouble());

        RegistroActualViewModel.TotalGamerPay = 0;
        var itemsNoListados = 0;

        foreach (var item in ItemPrecioViewModelsList)
        {
            if (preciosGamerPayDict.TryGetValue(item.GamerPayNombre, out var precio))
            {
                item.PrecioGamerPay = precio;
                RegistroActualViewModel.TotalGamerPay += precio * item.Cantidad;
            }
            else
            {
                itemsNoListados++;
            }
        }

        GamerPayTotal.Text = RegistroActualViewModel.TotalGamerPay.ToString("F2");
        ItemsNoListados.Text = itemsNoListados.ToString();

        var totalItems = ItemPrecioViewModelsList.Count;
        ProgressBarGamerPay.Value = 100;
        GamerPayProgressPercentage.Text = "100";
        GamerPayProgressTextBlock.Text = $"{totalItems} / {totalItems}";

        return true;
    }

    public async Task<bool> GenerarRegistroSteam()
    {
        return true;
    }
}