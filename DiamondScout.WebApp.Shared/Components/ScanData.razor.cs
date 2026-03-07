using Microsoft.AspNetCore.Components;
using ReactorBlazorQRCodeScanner;

namespace DiamondScout.WebApp.Shared.Components;

public partial class ScanData : ComponentBase
{
    [Parameter]
    public EventCallback<string> OnScan { get; set; }

    private QRCodeScannerJsInterop? _qrCodeScannerJsInterop;

    protected override async Task OnAfterRenderAsync(bool firstRender)
    {
        if (!firstRender)
            return;

        _qrCodeScannerJsInterop = new QRCodeScannerJsInterop(Js);
        await _qrCodeScannerJsInterop.Init(async scannedValue =>
        {
            await OnScan.InvokeAsync(scannedValue);
        });
    }
}