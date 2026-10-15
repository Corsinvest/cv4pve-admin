/*
 * SPDX-FileCopyrightText: Copyright Corsinvest Srl
 * SPDX-License-Identifier: AGPL-3.0-only
 */
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.Formats;
using SixLabors.ImageSharp.Formats.Jpeg;
using SixLabors.ImageSharp.Formats.Png;
using SixLabors.ImageSharp.Formats.Webp;
using SixLabors.ImageSharp.Processing;

namespace Corsinvest.ProxmoxVE.Admin.Core.Helpers;

public static class ImageProcessorHelper
{
    // Only the formats a profile picture needs, without metadata: the known vulnerabilities of 3.1.12
    // are in TIFF decoding, histogram processing and color profiles
    private static readonly DecoderOptions _decoderOptions = new()
    {
        Configuration = new SixLabors.ImageSharp.Configuration(new JpegConfigurationModule(),
                                                               new PngConfigurationModule(),
                                                               new WebpConfigurationModule()),
        SkipMetadata = true
    };

    public static async Task<MemoryStream> ToJpegAsync(Stream source, int width, int height, int quality)
    {
        source.Position = 0;
        using var image = await Image.LoadAsync(_decoderOptions, source);
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
