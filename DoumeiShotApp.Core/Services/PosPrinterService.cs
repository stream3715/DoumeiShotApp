using System.Text;
using DoumeiShotApp.Core.Contracts.Services;
using ESCPOS_NET;
using ESCPOS_NET.Emitters;
using ESCPOS_NET.Emitters.BaseCommandValues;
using ESCPOS_NET.Utilities;

namespace DoumeiShotApp.Core.Services;

public class PosPrinterService : IPosPrinterService
{
    private ImmediateNetworkPrinter _networkPrinter;
    private SerialPrinter _serialPrinter;
    private readonly EPSON e;

    public PosPrinterService()
    {
        Encoding.RegisterProvider(CodePagesEncodingProvider.Instance);
        e = new EPSON();
    }

    private static bool ValidateIPv4(string ipString)
    {
        if (string.IsNullOrWhiteSpace(ipString))
        {
            return false;
        }

        var splitValues = ipString.Split('.');
        if (splitValues.Length != 4)
        {
            return false;
        }

        byte tempForParsing;

        return splitValues.All(r => byte.TryParse(r, out tempForParsing));
    }

    public async Task ConnectPrinter(int method, string target)
    {
        _networkPrinter = null;
        if (_serialPrinter != null) { _serialPrinter.Dispose(); }

        if (target == null)
        {
            throw new Exception("TARGET_NULL");
        }
        else if (method == 0)
        {
            _serialPrinter = new SerialPrinter(portName: target, baudRate: 115200);
        }
        else if (method == 1)
        {
            if (!ValidateIPv4(target))
            {
                throw new Exception("IPADDRESS_ILLEGAL");
            }

            _networkPrinter = new ImmediateNetworkPrinter(new ImmediateNetworkPrinterSettings()
            {
                ConnectionString = $"{target}:9100",
                PrinterName = "PosPrinter"
            });

            try
            {
                await _networkPrinter.GetOnlineStatus(e);
            } catch
            {
                throw new Exception("PRINTER_NOT_RESPONSED");
            }
            
        }
        else
        {
            throw new Exception("UNKNOWN_METHOD");
        }
    }

    public void PrintQRCode(string url, DateTime expired)
    {
        var printBody = ByteSplicer.Combine(
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
            e.PrintQRCode(url, size: Size2DCode.NORMAL),
            e.PrintLine(""),
            ConvertToSJISLine("有効期限：" + expired.ToString()),
            e.PrintLine(""),
            ConvertToSJISLine("画像を長押しし「\"写真\"に保存」または「画像をダウンロード」より保存して下さい。"),
            e.PrintLine(""),
            ConvertToSJISLine("※有効期限を過ぎるとアクセスができなくなりますので、早めの"),
            ConvertToSJISLine("ダウンロードをお願いします。"),
            e.PrintLine(""),
            e.PrintLine(""),
            e.FeedLines(2),
            e.PartialCutAfterFeed(5)
          );

        if (_serialPrinter != null)
        {
            _serialPrinter.Write(printBody);
        }
        else if (_networkPrinter != null)
        {
            _networkPrinter.WriteAsync(printBody);
        }
        else
        {
            throw new Exception("PRINTER_NOT_INITIALIZED");
        }
    }

    private byte[] ConvertToSJISLine(string text)
    {
        var sjisEnc = Encoding.GetEncoding("shift_jis");
        return sjisEnc.GetBytes(text.Replace("\r", string.Empty).Replace("\n", string.Empty) + "\n");
    }
}
