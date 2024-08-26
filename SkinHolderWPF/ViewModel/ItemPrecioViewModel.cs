using System.ComponentModel.DataAnnotations.Schema;

namespace SkinHolderWPF.ViewModel;

public class ItemPrecioViewModel
{
    public required int ItemPrecioId { get; set; }
    [Column(TypeName = "decimal(10, 2)")]
    public required float PrecioSteam { get; set; }
    [Column(TypeName = "decimal(10, 2)")]
    public required float PrecioGamerPay { get; set; }
    public required int UserItemId { get; set; }
    public required int RegistroId { get; set; }

    // Extra que no se guardará en DB
    public required int Cantidad { get; set; }
    public required string SteamHashName { get; set; }
    public required string GamerPayNombre { get; set; }
    public required bool Fallo { get; set; }
}