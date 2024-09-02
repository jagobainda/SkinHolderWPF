using System.Text.Json;
using System.Windows;
using SkinHolderWPF.Enums;
using SkinHolderWPF.Enums.Api;
using SkinHolderWPF.Utils;
using SkinHolderWPF.ViewModel;

namespace SkinHolderWPF.View;

/// <summary>
/// Lógica de interacción para RegistrosMenu.xaml
/// </summary>
public partial class RegistrosMenu
{
    public RegistroViewModel RegistroActualViewModel = null!;

    public List<ItemPrecioViewModel> ItemPrecioViewModelsList = null!;

    public int ContadorSteam;

    public object Lock = new();

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

        lock (Lock) ContadorSteam = 0;
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

        if (estado) RegistroActualViewModel.FechaHora = DateTime.Now;
    }

    public void ActualizarProgresoSteam()
    {
        ProgressBarSteam.Value = ContadorSteam;

        SteamProgress.Text = ContadorSteam.ToString();
    }

    private async void BtnConsultarSteam_Click(object sender, RoutedEventArgs e)
    {
        try
        {
            CambiarEstadoBotones(false);

            ResetearObjetosRegistro();

            if (!await GenerarItemPrecios()) return;

            _ = await GenerarRegistroSteam();
        }
        catch (Exception ex)
        {
            // TODO: log
        }
        finally
        {
            CambiarEstadoBotones(true);
        }

        RegistroActualViewModel.RegistroTypeId = ERegistroType.Steam.GetHashCode();

        await GuardarRegistro();
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

        RegistroActualViewModel.RegistroTypeId = ERegistroType.GamerPay.GetHashCode();

        await GuardarRegistro();
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

        RegistroActualViewModel.RegistroTypeId = ERegistroType.All.GetHashCode();

        await GuardarRegistro();
    }


    private void BtnConsultarHistorialRegistros_Click(object sender, RoutedEventArgs e)
    {
        MessageBox.Show("No disponible todavía");
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
            GamerPayNombre = userItem.GetProperty("item").GetProperty("gamerPayNombre").GetString()!,
            Fallo = false
        }).ToList();

        ItemPrecioViewModelsList.AddRange(itemPrecioViewModels);

        ProgressBarSteam.Maximum = ItemPrecioViewModelsList.Count;
        ProgressBarGamerPay.Maximum = ItemPrecioViewModelsList.Count;

        return true;
    }

    public async Task<bool> GenerarRegistroGamerPay()
    {
        var totalItems = ItemPrecioViewModelsList.Count;
        GamerPayTotalItems.Text = totalItems.ToString();

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

        ProgressBarGamerPay.Value = totalItems;
        GamerPayProgressTextBlock.Text = $"{totalItems} / {totalItems}";

        return true;
    }

    public async Task<bool> GenerarRegistroSteam()
    {
        var totalItems = ItemPrecioViewModelsList.Count;
        SteamTotalItems.Text = totalItems.ToString();

        var listApi = ItemPrecioViewModelsList
            .Select(itemPrecioViewModel => itemPrecioViewModel.SteamHashName)
            .Take(20)
            .ToList();

        var listResto = ItemPrecioViewModelsList
            .Select(itemPrecioViewModel => itemPrecioViewModel.SteamHashName)
            .Skip(20)
            .ToList();

        var itemPrecioDictionary = ItemPrecioViewModelsList.ToDictionary(it => it.SteamHashName);

        #if DEBUG
            var jsonPreciosApi = await ApiInfo.GetJsonFromPost(ApiInfo.BaseRegistrosUrl + ERegistrosApiMethods.GetPreciosSteam, listApi);

            RespuestaPeticion[] arrayRespuestaPeticiones = [];
            if (listResto is { Count: > 0 }) arrayRespuestaPeticiones = await SteamRequests.EjecutarPeticiones(listResto);
        #elif RELEASE
            var jsonPreciosApiTask = ApiInfo.GetJsonFromPost(ApiInfo.BaseRegistrosUrl + ERegistrosApiMethods.GetPreciosSteam, listApi);
            var arrayRespuestaPeticionesTask = Task.FromResult(Array.Empty<RespuestaPeticion>());
            
            if (listResto is { Count: > 0 }) { arrayRespuestaPeticionesTask = SteamRequests.EjecutarPeticiones(listResto); }

            await Task.WhenAll(jsonPreciosApiTask, arrayRespuestaPeticionesTask);

            var jsonPreciosApi = await jsonPreciosApiTask;
            var arrayRespuestaPeticiones = await arrayRespuestaPeticionesTask;
        #endif

        using var docPreciosSteam = JsonDocument.Parse(jsonPreciosApi);

        var root = docPreciosSteam.RootElement.EnumerateArray();

        foreach (var itemPrecioSteam in root)
        {
            var hashName = itemPrecioSteam.GetProperty("hashName").GetString();

            if (!itemPrecioDictionary.TryGetValue(hashName!, out var item)) continue;

            item.PrecioSteam = (float)itemPrecioSteam.GetProperty("precio").GetDouble();
            item.Fallo = itemPrecioSteam.GetProperty("fallo").GetBoolean();

            lock (Lock) ContadorSteam++;
            
            ActualizarProgresoSteam();
        }

        foreach (var respuestaPeticion in arrayRespuestaPeticiones)
        {
            if (!itemPrecioDictionary.TryGetValue(respuestaPeticion.HashName, out var item)) continue;

            item.PrecioSteam = respuestaPeticion.Precio;
            item.Fallo = respuestaPeticion.Fallo;
        }

        RegistroActualViewModel.TotalSteam = ItemPrecioViewModelsList.Sum(item => item.PrecioSteam * item.Cantidad);

        SteamTotal.Text = RegistroActualViewModel.TotalSteam.ToString("F2");

        ItemsConWarning.Text = ItemPrecioViewModelsList.Count(ip => ip is { Fallo: true, PrecioSteam: > 0 }).ToString();

        ItemsConError.Text = ItemPrecioViewModelsList.Count(ip => ip.PrecioSteam < 0).ToString();

        return true;
    }

    private async Task GuardarRegistro()
    {
        var registroIdString = await ApiInfo.GetJsonFromPost(ApiInfo.BaseRegistrosUrl + ERegistrosApiMethods.CreateRegistro, RegistroActualViewModel);

        if (!int.TryParse(registroIdString, out var registroId)) return;

        ItemPrecioViewModelsList.ForEach(itemPrecioViewModel => itemPrecioViewModel.RegistroId = registroId);

        var confirm = await ApiInfo.GetJsonFromPost(ApiInfo.BaseItemPrecioUrl + EItemsPrecioApiMethods.CreateItemPrecios, ItemPrecioViewModelsList);

        if (confirm != "true" || Window.GetWindow(this) is not MainWindow window) return;

        window.GetLastRegistroPrecioTotal();
    }
}