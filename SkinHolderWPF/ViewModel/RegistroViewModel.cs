namespace SkinHolderWPF.ViewModel;

public class RegistroViewModel
{
    public required long RegistroId { get; set; }
    public required DateTime FechaHora { get; set; }
    public required double TotalSteam { get; set; }
    public required double TotalGamerPay { get; set; }
    public required int UserId { get; set; }
    public required int RegistroTypeId { get; set; }
}