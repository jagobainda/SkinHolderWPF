using System.ComponentModel.DataAnnotations.Schema;

namespace SkinHolderWPF.ViewModel;

public class RegistroViewModel
{
    public required long RegistroId { get; set; }
    public required DateTime FechaHora { get; set; }
    [Column(TypeName = "decimal(10, 2)")]
    public required double TotalSteam { get; set; }
    [Column(TypeName = "decimal(10, 2)")]
    public required double TotalGamerPay { get; set; }
    public required int UserId { get; set; }
    public required int RegistroTypeId { get; set; }
}