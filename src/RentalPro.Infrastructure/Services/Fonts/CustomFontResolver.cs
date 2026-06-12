using PdfSharp.Fonts;

namespace RentalPro.Infrastructure.Services.Fonts;

public sealed class CustomFontResolver : IFontResolver
{
    private const string FontName = "Arial";

    public FontResolverInfo ResolveTypeface(
        string familyName,
        bool isBold,
        bool isItalic)
    {
        return new FontResolverInfo(FontName);
    }

    public byte[] GetFont(string faceName)
    {
        var fontPath = Path.Combine(
            Environment.GetFolderPath(Environment.SpecialFolder.Fonts),
            "arial.ttf");

        return File.ReadAllBytes(fontPath);
    }
}