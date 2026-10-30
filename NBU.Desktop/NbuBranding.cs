namespace NBU.Desktop;

internal static class NbuBranding
{
    public static Image? LoadMark()
    {
        var path = Path.Combine(AppContext.BaseDirectory, "nbu-mark.png");
        if (!File.Exists(path)) path = Path.Combine(AppContext.BaseDirectory, "Resources", "nbu-mark.png");
        if (!File.Exists(path)) return null;

        using var source = Image.FromFile(path);
        return new Bitmap(source);
    }
}
