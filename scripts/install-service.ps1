# Ensure script is run as Administrator
if (-not ([Security.Principal.WindowsPrincipal] [Security.Principal.WindowsIdentity]::GetCurrent()).IsInRole([Security.Principal.WindowsBuiltInRole] "Administrator")) {
    Write-Host "Please run this script as Administrator!"
    exit
}

# Create watch & quarantine folders
$watchPath = "C:\DefenderLite\watch"
$quarantinePath = "C:\DefenderLite\quarantine"

if (-not (Test-Path $watchPath)) { New-Item -ItemType Directory -Path $watchPath }
if (-not (Test-Path $quarantinePath)) { New-Item -ItemType Directory -Path $quarantinePath }

# Determine architecture (x86, x64, arm64)
$arch = if ([Environment]::Is64BitOperatingSystem) { "x64" } else { "x86" }
$exePath = "C:\DefenderLite\publish\$arch\DefenderLite.exe"

# Stop service if already exists
if (Get-Service -Name "DefenderLite" -ErrorAction SilentlyContinue) {
    sc.exe stop DefenderLite
    sc.exe delete DefenderLite
    Start-Sleep -Seconds 2
}

# Install service
sc.exe create DefenderLite binPath= "`"$exePath`"" start= auto
sc.exe start DefenderLite

Write-Host "DefenderLite installed and running as a Windows Service!"
