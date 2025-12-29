using System;
using System.IO;
using System.Drawing;
using System.Drawing.Imaging;
using System.Runtime.CompilerServices;
using System.Text;

namespace ImageEdit;

public class ImageTransformer
{
    public static byte[] TransformImage(
        byte[] inputImage,
        ImageFormat outputFormat,
        long? targetFileSizeBytes,
        (int width, int height)? targetResolution,
        bool inflateWithMetadata
    )
{
    using var inputStream = new MemoryStream(inputImage);
    using var originalBitmap = new Bitmap(inputStream);

    Bitmap workingBitmap = originalBitmap;

    // ─────────────────────────────
    // 1. Spatial Resampling
    // ─────────────────────────────
    if (targetResolution.HasValue)
    {
        workingBitmap = new Bitmap(
            originalBitmap,
            new Size(
                targetResolution.Value.width,
                targetResolution.Value.height
            )
        );
    }

    // ─────────────────────────────
    // 2. Metadata Inflation (Optional)
    // ─────────────────────────────
    if (inflateWithMetadata)
    {
        PropertyItem padding = (PropertyItem)
            RuntimeHelpers.GetUninitializedObject(typeof(PropertyItem));

        byte[] filler = Encoding.ASCII.GetBytes(
            new string('Z', 1024 * 200) // 200 KB EXIF padding
        );

        padding.Id = 0x010E; // ImageDescription
        padding.Type = 2;    // ASCII
        padding.Len = filler.Length;
        padding.Value = filler;

        workingBitmap.SetPropertyItem(padding);
    }

    // ─────────────────────────────
    // 3. Encoder Selection
    // ─────────────────────────────
    ImageCodecInfo? codec = null;
    foreach (var c in ImageCodecInfo.GetImageEncoders())
    {
        if (c.FormatID == outputFormat.Guid)
        {
            codec = c;
            break;
        }
    }

    if (codec == null)
        throw new InvalidOperationException("Unsupported image format");

    // ─────────────────────────────
    // 4. Adaptive Encoding Loop
    // ─────────────────────────────
    long quality = 90;
    byte[] output;

    do
    {
        using var outputStream = new MemoryStream();
        using var encoderParams = new EncoderParameters(1);

        encoderParams.Param[0] = new EncoderParameter(
            System.Drawing.Imaging.Encoder.Quality,
            quality
        );

        workingBitmap.Save(outputStream, codec, encoderParams);
        output = outputStream.ToArray();

        if (!targetFileSizeBytes.HasValue)
            break;

        if (output.Length > targetFileSizeBytes)
            quality -= 5;
        else
            break;

    } while (quality > 5);

    return output;
}
}
