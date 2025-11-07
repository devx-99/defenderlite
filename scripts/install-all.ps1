<#
.SYNOPSIS
One-click installer for DefenderLite.

.DESCRIPTION
This script:
- Detects system architecture (x64, x86, ARM64)
- Copies the correct executable to C:\DefenderLite
- Creates watch & quarantine folders
- Installs and starts the Windows service
- Optionally adds a signature hash to signatures.txt

Run as Administrator!
#>

# Allow script to run without execution policy blocking
Set-ExecutionPolicy -Scope Process -ExecutionPolicy Bypass -Force

# Determine system architecture
$arch = if ([Environment]::Is64BitOperatingSystem) {
    "x64"
} else {
    "x86"
}

# Use ARM64 if detected
if ($env:PROCESSOR_ARCHITECTURE -eq "ARM64") { $arch = "arm64" }

Write-Host "Detected architecture: $arch"

# Paths
$sourceExe = ".\publish\$arch\DefenderLite.exe"
$destFolder = "C:\DefenderLite"
$destExe = "$destFolder\DefenderLite.exe"
$watchFolder = "$destFolder\watch"
$quarantineFolder = "$destFolder\quarantine"
$signatureFile = "$destFolder\signatures.txt"

# Create main folder and folders for watch/quarantine
Write-Host "Creating folders..."
New-Item -ItemType Directory -Path $destFolder -Force | Out-Null
New-Item -ItemType Directory -Path $watchFolder -Force | Out-Null
New-Item -ItemType Directory -Path $quarantineFolder -Force | Out-Null

# Copy the correct executable
Write-Host "Copying executable..."
Copy-Item -Path $sourceExe -Destination $destExe -Force

# Install the service
Write-Host "Installing Windows service..."
$serviceName = "DefenderLite"
if (Get-Service -Name $serviceName -ErrorAction SilentlyContinue) {
    Write-Host "Service already exists. Restarting..."
    sc stop $serviceName
    sc delete $serviceName
    Start-Sleep -Seconds 2
}
sc create $serviceName binPath= $destExe start= auto
sc start $serviceName

Write-Host "Service installed and started successfully!"

# Optionally add a signature
$addSig = Read-Host "Do you want to add a SHA256 signature to the signature DB? (y/n)"
if ($addSig -eq "y") {
    $hash = Read-Host "Enter the SHA256 hash"
    Add-Content -Path $signatureFile -Value $hash
    Write-Host "Hash added to signatures.txt"
}

Write-Host "`n✅ DefenderLite is installed and running."
Write-Host "Watch folder: $watchFolder"
Write-Host "Quarantine folder: $quarantineFolder"
