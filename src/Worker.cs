namespace DefenderLite;
using System;
using System.IO;
using System.Security.Cryptography;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Runtime.InteropServices;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

public class Worker : BackgroundService
{
    private readonly ILogger<Worker> _logger;
    private FileSystemWatcher? _watcher; // Nullable to fix CS8618 warning
    private readonly string _watchPath = @"C:\DefenderLite\watch"; 
    private readonly string _quarantinePath = @"C:\DefenderLite\quarantine";
    private readonly SignatureDb _sigDb = new SignatureDb();

    public Worker(ILogger<Worker> logger)
    {
        _logger = logger;
        Directory.CreateDirectory(_watchPath);
        Directory.CreateDirectory(_quarantinePath);

        // Example signature, in production load from DB
        _sigDb.Add("E3B0C44298FC1C149AFBF4C8996FB92427AE41E4649B934CA495991B7852B855"); 
    }

    protected override Task ExecuteAsync(CancellationToken stoppingToken)
    {
        _watcher = new FileSystemWatcher(_watchPath)
        {
            NotifyFilter = NotifyFilters.FileName | NotifyFilters.Size | NotifyFilters.CreationTime
        };

        _watcher.Created += OnCreated;
        _watcher.EnableRaisingEvents = true;

        _logger.LogInformation("Worker started. Watching {path}", _watchPath);
        _logger.LogInformation("Running on architecture: {arch}", RuntimeInformation.ProcessArchitecture);
        return Task.CompletedTask;
    }

    private async void OnCreated(object sender, FileSystemEventArgs e)
    {
        try
        {
            _logger.LogInformation("New file detected: {file}", e.FullPath);

            // Wait briefly for file to be fully written
            await Task.Delay(300);

            byte[] data = await File.ReadAllBytesAsync(e.FullPath);
            string sha256 = ComputeSha256(data);

            if (_sigDb.Match(sha256))
            {
                _logger.LogWarning("Signature match: {file}", e.FullPath);
                QuarantineHelper.MoveToQuarantine(e.FullPath, _quarantinePath, _logger);
                return;
            }

            // AMSI scan — disabled on ARM64 to prevent crash
            if (RuntimeInformation.ProcessArchitecture != Architecture.Arm64 &&
                RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
            {
                try
                {
                    var result = AmsiScanner.ScanBuffer(data, Path.GetFileName(e.FullPath));
                    if (result.IsMalicious)
                    {
                        _logger.LogWarning("AMSI reported malicious: {file} -> {reason}", e.FullPath, result.Reason);
                        QuarantineHelper.MoveToQuarantine(e.FullPath, _quarantinePath, _logger);
                        return;
                    }
                }
                catch (PlatformNotSupportedException)
                {
                    _logger.LogWarning("AMSI scan not supported on this platform: {file}", e.FullPath);
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "AMSI scan failed for file {file}", e.FullPath);
                }
            }
            else if (RuntimeInformation.ProcessArchitecture == Architecture.Arm64)
            {
                _logger.LogWarning("Skipping AMSI scan on ARM64 to prevent crash: {file}", e.FullPath);
            }

            _logger.LogInformation("File clean: {file}", e.FullPath);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error handling file {file}", e.FullPath);
        }
    }

    public override Task StopAsync(CancellationToken cancellationToken)
    {
        _watcher?.Dispose();
        _logger.LogInformation("Worker stopped.");
        return base.StopAsync(cancellationToken);
    }

    private static string ComputeSha256(byte[] data)
    {
        using var sha = SHA256.Create();
        var hash = sha.ComputeHash(data);
        var sb = new StringBuilder();
        foreach (var b in hash) sb.Append(b.ToString("x2"));
        return sb.ToString();
    }
}

// Simple in-memory signature database
class SignatureDb
{
    private readonly HashSet<string> _hashes = new(StringComparer.OrdinalIgnoreCase);
    public void Add(string sha256) => _hashes.Add(sha256);
    public bool Match(string sha256) => _hashes.Contains(sha256);
}
