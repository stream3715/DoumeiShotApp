using System.Text;
using DoumeiShotApp.Core.Contracts.Services;
using ESCPOS_NET;
using ESCPOS_NET.Emitters;
using ESCPOS_NET.Utilities;

namespace DoumeiShotApp.Core.Services;

public class PosPrinterService : IPosPrinterService
{
    private ImmediateNetworkPrinter _printer;
    public PosPrinterService()
    {
        Encoding.RegisterProvider(CodePagesEncodingProvider.Instance);
    }

    public void ConnectPrinter()
    {
        // _printer = new SerialPrinter(portName: "COM2", baudRate: 115200);
        _printer = new ImmediateNetworkPrinter(new ImmediateNetworkPrinterSettings()
        {
            ConnectionString = "192.168.1.240:9100",
            PrinterName = "TestPrinter"
        });

        if (_printer == null)
        {
            throw new Exception("PRINTER_NOT_RESPONSED");
        }
    }

    public void PrintQRCode(string url, DateTime expired)
    {
        if (_printer == null) ConnectPrinter();
        var e = new EPSON();

        _printer.WriteAsync( // or, if using and immediate printer, use await printer.WriteAsync
          ByteSplicer.Combine(
              // SJIS対応部分
              new byte[3] {
                28, 67, 1
            },
              new byte[2] {
                28, 38
            },
            e.CenterAlign(),
            e.PrintImage(File.ReadAllBytes("Assets/moa_logo.png"), true),
            e.PrintLine(""),
            ConvertToSJISLine("早稲田祭2022 同盟ショット"),
            e.PrintLine(""),
            e.PrintLine(""),
            e.PrintQRCode(url, size: Size2DCode.LARGE),
            e.PrintLine(""),
            ConvertToSJISLine("有効期限：" + expired.ToString()),
            e.PrintLine(""),
            ConvertToSJISLine("※有効期限を過ぎるとアクセスができなくなりますので、早めの"),
            ConvertToSJISLine("ダウンロードをお願いします。"),
            e.PrintLine(""),
            e.PrintLine(""),
            e.PartialCutAfterFeed(5)
          )
        );
    }

    private byte[] ConvertToSJISLine(string text)
    {
        var sjisEnc = Encoding.GetEncoding("shift_jis");
        return sjisEnc.GetBytes(text.Replace("\r", string.Empty).Replace("\n", string.Empty) + "\n");
    }
}
