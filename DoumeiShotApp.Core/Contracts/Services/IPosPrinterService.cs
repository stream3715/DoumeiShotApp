using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DoumeiShotApp.Core.Contracts.Services;
public interface IPosPrinterService
{
    void ConnectPrinter();
    void PrintQRCode(string url, DateTime expired);
}
