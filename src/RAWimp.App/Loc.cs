using Microsoft.Windows.ApplicationModel.Resources;

namespace RAWimp.App;

// Translations. Strings live in Strings/<language>/Resources.resw and compile into resources.pri;
// XAML reaches them with x:Uid, code through here.
//
// Microsoft.Windows.ApplicationModel.Resources (MRT Core from the Windows App SDK), not the WinRT
// Windows.ApplicationModel.Resources: the latter needs package identity, and this app is unpackaged.
static class Loc
{
    private static readonly ResourceLoader Loader = new();

    /// The translation for `key`, or the key itself when it is missing — a visible, searchable
    /// placeholder beats an empty label or an exception in a dialog.
    public static string Get(string key)
    {
        try
        {
            var value = Loader.GetString(key);
            if (!string.IsNullOrEmpty(value)) return value;
            Diag.Log("missing translation: " + key);
        }
        catch (Exception e) { Diag.Log($"translation {key}: {e.Message}"); }
        return key;
    }

    /// Formats a translated string that contains {0}, {1}, … placeholders.
    public static string Get(string key, params object[] args)
    {
        var format = Get(key);
        try { return string.Format(format, args); }
        catch (FormatException e) { Diag.Log($"translation {key} format: {e.Message}"); return format; }
    }
}
