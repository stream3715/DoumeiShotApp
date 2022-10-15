namespace DoumeiShotApp.Core.Contracts.Services;
public interface IPosPrinterService
{
    Task ConnectPrinter(int method, string target);
    void PrintQRCode(string url, DateTime expired);
}
