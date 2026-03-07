using Microsoft.AspNetCore.Components;
using ReactorBlazorQRCodeScanner;

namespace DiamondScout.WebApp.Shared.Components;

public partial class ScanData(Action<string> onScan) : ComponentBase
{
    private QRCodeScannerJsInterop? _qrCodeScannerJsInterop;
    
    protected override async Task OnInitializedAsync()
    {
        _qrCodeScannerJsInterop = new QRCodeScannerJsInterop(Js);
        await _qrCodeScannerJsInterop.Init(onScan);
    }
}