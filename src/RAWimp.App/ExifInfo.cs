using MetadataExtractor;
using MetadataExtractor.Formats.Exif;
using MetadataExtractor.Formats.GeoTiff;
using Directory = System.IO.Directory;

namespace RAWimp.App;

// Reads the metadata shown in the info pane. Pure: a path in, display rows out — no UI, so it can
// run off the UI thread (and be tested on its own).
static class ExifInfo
{
    public static List<ExifRow> Read(string path)
    {
        var rows = new List<ExifRow>();
        try { rows.Add(new ExifRow(Loc.Get("Exif_File"), Path.GetFileName(path))); } catch { }

        try
        {
            var dirs = ImageMetadataReader.ReadMetadata(path);
            var exif = dirs.OfType<ExifDirectoryBase>().ToList();
            var gps = dirs.OfType<GpsDirectory>().FirstOrDefault();

            // Search across every EXIF IFD (IFD0, the real Exif SubIFD, preview SubIFDs, …).
            // Different RAW makers scatter the same tags into different sub-IFDs, so picking one
            // directory misses fields (NEF/DNG put exposure/ISO in a SubIFD that isn't the first).
            string? Val(int tag) => exif.Select(d => d.GetDescription(tag))
                                        .FirstOrDefault(v => !string.IsNullOrWhiteSpace(v));
            void Add(string label, string? val) { if (!string.IsNullOrWhiteSpace(val)) rows.Add(new ExifRow(label, val!)); }

            var cam = string.Join(" ", new[] { Val(ExifDirectoryBase.TagMake), Val(ExifDirectoryBase.TagModel) }
                .Where(x => !string.IsNullOrWhiteSpace(x)));
            Add(Loc.Get("Exif_Camera"), cam);
            Add(Loc.Get("Exif_Lens"), Val(ExifDirectoryBase.TagLensModel));
            Add(Loc.Get("Exif_DateTaken"), Val(ExifDirectoryBase.TagDateTimeOriginal) ?? Val(ExifDirectoryBase.TagDateTime));
            Add(Loc.Get("Exif_Exposure"), Val(ExifDirectoryBase.TagExposureTime));
            Add(Loc.Get("Exif_Aperture"), Val(ExifDirectoryBase.TagFNumber));
            Add("ISO", Val(ExifDirectoryBase.TagIsoEquivalent));
            Add(Loc.Get("Exif_FocalLength"), Val(ExifDirectoryBase.TagFocalLength));
            Add(Loc.Get("Exif_ExposureBias"), Val(ExifDirectoryBase.TagExposureBias));
            Add(Loc.Get("Exif_Flash"), Val(ExifDirectoryBase.TagFlash));

            var w = Val(ExifDirectoryBase.TagExifImageWidth);
            var h = Val(ExifDirectoryBase.TagExifImageHeight);
            if (w is not null && h is not null) Add(Loc.Get("Exif_Dimensions"), $"{w} × {h}");
            if (gps?.GetGeoLocation() is { } loc) Add("GPS", $"{loc.Latitude:F5}, {loc.Longitude:F5}");
        }
        catch (Exception e) { Diag.Log("exif fail: " + e.Message); }

        try { rows.Add(new ExifRow(Loc.Get("Exif_Size"), $"{new FileInfo(path).Length / 1048576.0:F1} MB")); } catch { }
        return rows;
    }
}
