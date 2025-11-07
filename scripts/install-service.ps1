# Create watch & quarantine folders
mkdir C:\DefenderLite\watch -Force
mkdir C:\DefenderLite\quarantine -Force

# Install service (adjust architecture folder)
$arch = "x64"
$exePath = "C:\DefenderLite\publish\$arch\DefenderLite.exe"

sc create DefenderLite binPath= $exePath start= auto
sc start DefenderLite
