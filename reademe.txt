==============================
DefenderLite
==============================

Version: 1.0
Author: Dev
License: MIT
GitHub: https://github.com/devx-99
Release: https://github.com/devx-99/defenderlite/releases/tag/v1.0.0

----------------------------------------
DESCRIPTION
----------------------------------------
DefenderLite is a lightweight file monitoring and antivirus tool for Windows.
It monitors a designated folder ("watch folder") for new files, compares
their SHA256 hash against a signature database, and quarantines suspicious files.
It supports multiple architectures: x86, x64, and ARM64.

Note: This software is not a full antivirus solution but a folder-level scanner
for demonstration and learning purposes.

----------------------------------------
FEATURES
----------------------------------------
- Monitors a specified folder for new files.
- Detects known malicious files using SHA256 signature database.
- Quarantines detected files in a separate folder.
- Supports Windows services for background monitoring.
- Supports multiple architectures: x86, x64, ARM64.
- Optional AMSI scanning for Windows scripts (limited support on ARM64).

----------------------------------------
FOLDER STRUCTURE
----------------------------------------
C:\DefenderLite
│
├─ DefenderLite.exe          <-- The executable (installed automatically by script)
├─ watch                     <-- Monitored folder (files here are scanned)
├─ quarantine                <-- Quarantined suspicious files
└─ signatures.txt            <-- SHA256 signatures of known malicious files

----------------------------------------
INSTALLATION (FOR NON-TECHNICAL USERS)
----------------------------------------
1. Download the latest release from GitHub.
2. Extract the archive to any folder (default recommended: C:\DefenderLite).
3. Run PowerShell as Administrator.
4. Execute the provided installation script:
   .\install-all.ps1
5. Follow prompts to optionally add SHA256 signatures.
6. DefenderLite service will automatically start and monitor the watch folder.

----------------------------------------
USAGE
----------------------------------------
- Add files to the "C:\DefenderLite\watch" folder.
- Detected malicious files are moved to "C:\DefenderLite\quarantine".
- To stop monitoring, stop the Windows service "DefenderLite".
- To update signatures, edit or append to "C:\DefenderLite\signatures.txt"
  or use the add-signature script.

----------------------------------------
DEVELOPER USAGE
----------------------------------------
- Clone the repository:
  git clone https://github.com/devx-99/defenderlite.git
- Restore packages:
  dotnet restore
- Build:
  dotnet build
- Run locally:
  dotnet run

----------------------------------------
TROUBLESHOOTING
----------------------------------------
- If DefenderLite.exe cannot be signed, ensure you run PowerShell as Administrator.
- On ARM64, AMSI scanning may be skipped to prevent crashes.
- If the service fails to start, check that no previous "DefenderLite" service exists.

----------------------------------------
LICENSE
----------------------------------------
MIT License - see LICENSE file in the repository.

----------------------------------------
CONTACT
----------------------------------------
GitHub Issues: https://github.com/devx-99/defenderlite/issues
Email: thona6473@gmail.com
Website: https://www.romthokna.site
