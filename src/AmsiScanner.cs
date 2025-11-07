using System;
using System.Runtime.InteropServices;

public static class AmsiScanner
{
    private static IntPtr _amsiContext = IntPtr.Zero;
    private static IntPtr _amsiSession = IntPtr.Zero;
    private static bool _initialized = false;

    [DllImport("amsi.dll", CharSet = CharSet.Unicode)]
    private static extern IntPtr AmsiInitialize(string appName);

    [DllImport("amsi.dll")]
    private static extern void AmsiUninitialize(IntPtr amsiContext);

    // Scan buffer: returns result via out result (0 = AMSI_RESULT_CLEAN)
    [DllImport("amsi.dll", CharSet = CharSet.Unicode)]
    private static extern int AmsiScanBuffer(IntPtr amsiContext, byte[] buffer, uint length, string contentName, IntPtr amsiSession, out int result);

    public static (bool IsMalicious, int Reason) ScanBuffer(byte[] buffer, string contentName)
    {
        try
        {
            if (!_initialized)
            {
                _amsiContext = AmsiInitialize("DefenderLite");
                _initialized = (_amsiContext != IntPtr.Zero);
            }

            if (!_initialized)
                return (false, 0); // AMSI not available — treat as clean here, but log in production

            int result;
            int hr = AmsiScanBuffer(_amsiContext, buffer, (uint)buffer.Length, contentName, IntPtr.Zero, out result);

            // result values: AMSI_RESULT_CLEAN (0), AMSI_RESULT_NOT_DETECTED (usually 0), AMSI_RESULT_DETECTED (>0)
            bool malicious = result != 0;
            return (malicious, result);
        }
        catch
        {
            return (false, 0);
        }
    }

    public static void Close()
    {
        if (_initialized && _amsiContext != IntPtr.Zero)
        {
            AmsiUninitialize(_amsiContext);
            _amsiContext = IntPtr.Zero;
            _initialized = false;
        }
    }
}
