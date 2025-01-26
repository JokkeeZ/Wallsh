using System.Diagnostics.CodeAnalysis;
using System.Runtime.InteropServices;
using System.Runtime.Versioning;
using Microsoft.Extensions.Logging;

namespace Wallsh.Models.Environments.Linux;

[SuppressMessage("Globalization", "CA2101:Specify marshaling for P/Invoke string arguments")]
[SupportedOSPlatform("linux")]
public class GSettings : IDisposable
{
    private readonly ILogger<GSettings> _log = App.CreateLogger<GSettings>();

    private nint _gSettingsPtr;

    public GSettings(string schema)
    {
        try
        {
            _gSettingsPtr = g_settings_new(schema);
            if (_gSettingsPtr == nint.Zero)
                _log.LogError("Failed to create GSettings pointer. Ensure the schema: '{Schema}' is installed.", schema);
        }
        catch (Exception e)
        {
            _log.LogError("Error when calling g_settings_new Exception message: {Message}", e.Message);
        }
    }

    public void Dispose()
    {
        ReleaseUnmanagedResources();
        GC.SuppressFinalize(this);
    }

    [DllImport("libgio-2.0.so", CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
    private static extern nint g_settings_new(string schema);

    [DllImport("libgio-2.0.so", CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
    [return: MarshalAs(UnmanagedType.I1)]
    private static extern bool g_settings_set_string(nint settings, string key, string value);

    [DllImport("libgio-2.0.so", CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
    private static extern nint g_settings_get_string(nint settings, string key);

    [DllImport("libgobject-2.0.so", CallingConvention = CallingConvention.Cdecl)]
    private static extern void g_object_unref(nint obj);

    [DllImport("libglib-2.0.so", CallingConvention = CallingConvention.Cdecl)]
    private static extern void g_free(nint ptr);

    public void SetString(string key, string value)
    {
        try
        {
            g_settings_set_string(_gSettingsPtr, key, value);
        }
        catch (Exception e)
        {
            _log.LogError("Error when calling 'g_settings_set_string' Exception message: {Message}", e.Message);
        }
    }

    public string? GetString(string key)
    {
        nint getStringPtr;

        try
        {
            getStringPtr = g_settings_get_string(_gSettingsPtr, key);

            if (getStringPtr == nint.Zero)
            {
                _log.LogError("'g_settings_get_string' returned zero.");
                return null;
            }
        }
        catch (Exception e)
        {
            _log.LogError("Error when calling 'g_settings_get_string' Exception message: {Message}", e.Message);
            return null;
        }

        try
        {
            var str = Marshal.PtrToStringAnsi(getStringPtr);
            g_free(getStringPtr);

            return str;
        }
        catch (Exception e)
        {
            _log.LogError("Error when calling 'g_free' Exception message: {Message}", e.Message);
            return null;
        }
    }

    private void ReleaseUnmanagedResources()
    {
        if (_gSettingsPtr != nint.Zero)
        {
            g_object_unref(_gSettingsPtr);
            _gSettingsPtr = IntPtr.Zero;
        }
    }

    ~GSettings() => ReleaseUnmanagedResources();
}
