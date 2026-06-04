using System.Globalization;

namespace AppCoel.Resources;

public static class StringResource
{
    public static string GetStringByKey(string key, params object?[] args)
    {
        var message = Resource.ResourceManager.GetString(key, CultureInfo.CurrentCulture) ?? string.Empty;

        return string.Format(message, args);
    }
}
