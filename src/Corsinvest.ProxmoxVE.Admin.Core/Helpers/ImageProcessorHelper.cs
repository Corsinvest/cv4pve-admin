using SixLabors.ImageSharp;
using SixLabors.ImageSharp.Formats.Jpeg;
using SixLabors.ImageSharp.Processing;

namespace Corsinvest.ProxmoxVE.Admin.Core.Helpers;

public static class ImageProcessorHelper
{
    public static async Task<MemoryStream> ToJpegAsync(Stream source, int width, int height, int quality)
    {
        source.Position = 0;
        using var image = await Image.LoadAsync(source);
        image.Mutate(x => x.Resize(new ResizeOptions
        {
            Size = new Size(width, height),
            Mode = ResizeMode.Max
        }));

        var ret = new MemoryStream();
        await image.SaveAsJpegAsync(ret, new JpegEncoder { Quality = quality });
        ret.Position = 0;
        return ret;
    }
}
