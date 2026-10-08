# Register Native Messaging Host for Google Chrome and Microsoft Edge
param(
    [string]$ManifestPath = "$PSScriptRoot\..\installer\com.universal.signing.bridge.json"
)

$resolved = (Resolve-Path $ManifestPath).Path
Write-Host "Registering Native Messaging Host manifest: $resolved" -ForegroundColor Cyan

# Chrome Registry Path
$chromeKey = "HKCU:\Software\Google\Chrome\NativeMessagingHosts\com.universal.signing.bridge"
New-Item -Path $chromeKey -Force | Out-Null
Set-ItemProperty -Path $chromeKey -Name "(Default)" -Value $resolved
Write-Host "✓ Registered in Chrome Native Messaging Hosts" -ForegroundColor Green

# Edge Registry Path
$edgeKey = "HKCU:\Software\Microsoft\Edge\NativeMessagingHosts\com.universal.signing.bridge"
New-Item -Path $edgeKey -Force | Out-Null
Set-ItemProperty -Path $edgeKey -Name "(Default)" -Value $resolved
Write-Host "✓ Registered in Edge Native Messaging Hosts" -ForegroundColor Green

Write-Host "Browser Extension Native Messaging Host successfully installed." -ForegroundColor Green
