namespace DefenderLite;
using System;
using System.IO;
using Microsoft.Extensions.Logging;

public static class QuarantineHelper
{
    public static void MoveToQuarantine(string path, string quarantineDir, ILogger logger)
    {
        try
        {
            var fileName = Path.GetFileName(path);
            var target = Path.Combine(quarantineDir, $"{DateTime.UtcNow:yyyyMMddHHmmss}_{fileName}");
            File.Move(path, target);
            logger.LogInformation("Moved {file} to quarantine {q}", path, target);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Failed to move {file} to quarantine", path);
        }
    }
}
