using System.Drawing;
using PsDotNet.Colors;
using PsDotNet.LayerStyles;
using PsDotNet.Options.Save;

namespace PsDotNet.QuickStart;

class Program
{
    static void Main(string[] args)
    {
        //  IMPORTANT - If you get the following error: "No more virtual tiles can be allocated", make sure to run Visual Studio as a Admin

        //  Initialization only needs to be done once at the appropriate place in your code
        IPsApplication app = PsConnection.StartAndConnect(EPsVersion.CC2024);
        //  Or
        //IPsApplication app = PsConnection.StartAndConnect();  // Will start the highest version installed

        //  Create a sample document
        IPsDocument sampleDocument = CreateSampleDocument();

        //  Flatten layers
        sampleDocument.Flatten();

        //  Save active document as a .jpg
        SaveDocumentAsJpeg(sampleDocument);
    }

    private static IPsDocument CreateSampleDocument()
    {
        var app = PsConnection.Application;

        //  Create from rgb values
        app.ForegroundColor = app.CreateSolidColor(90, 0, 90);
        //  Create using System.Drawing.Color
        app.BackgroundColor = Color.LawnGreen.ToPsSolidColor();

        // Create a new document
        IPsDocument document = app.Documents.Add(512, 512, 72, "Welcome", EPsDocumentMode.psRGB);

        // Create a new layer
        IPsArtLayer cloudLayer = document.ArtLayers.AddNormalLayer("Clouds");

        //  Apply a few filters with default settings
        cloudLayer.ApplyClouds();
        cloudLayer.ApplyTwirl();
        cloudLayer.ApplyChalkAndCharcoal();
        cloudLayer.ApplyAccentedEdges();

        // Get a list of all of the available fonts
        IEnumerable<IPsTextFont> fonts = app.Fonts.ToList();

        // Create a new text layer
        IPsArtLayer textLayer = document.ArtLayers.AddTextLayer("Text");
        textLayer.TextItem.Contents = ("Welcome to PsDotNet\r" + Environment.UserName);
        int fontSize = 30;
        textLayer.TextItem.Size = fontSize;
        textLayer.TextItem.Position = new PointF((float)document.Width / 2, (float)document.Height / 4 + (float)fontSize / 2);
        textLayer.TextItem.Justification = EPsJustification.psCenter;
        textLayer.TextItem.Color = Color.OrangeRed.ToPsSolidColor();
        textLayer.TextItem.Font = fonts.Skip(26).First();
        textLayer.TextItem.HorizontalScale = 150;

        // Create a layerStyle to apply to the Text layer
        IPsLayerStyle layerStyle = app.CreateLayerStyle();

        //  Get the opposite color used from the foreground        
        IPsSolidColor pixelColor = app.ForegroundColor;
        pixelColor.HSB.OffsetHue(270);
        pixelColor.HSB.Saturation = 100;
        pixelColor.HSB.Brightness = 100;

        // Apply a glowStyle
        IPsOuterGlowStyle glowStyle = layerStyle.AddOuterGlowStyle(pixelColor);
        glowStyle.BlendMode = EPsLayerStyleBlendMode.Normal;
        glowStyle.Opacity = 100;
        glowStyle.Spread = 40;
        glowStyle.Size = 20;

        // Apply a strokeStyle
        IPsStrokeStyleColorFillType strokeFill = app.CreateStrokeStyleColorFillType(Color.Black.ToPsSolidColor());
        IPsStrokeStyle stroke = layerStyle.AddStrokeStyle(strokeFill);
        stroke.Size = 2;
        stroke.Position = EStrokePosition.Outside;

        // Apply to the text layer
        textLayer.ApplyLayerStyle(layerStyle);

        return document;
    }

    private static void SaveDocumentAsJpeg(IPsDocument document)
    {
        IPsApplication app = PsConnection.Application;

        //  Get current user directory to build filepath
        string userDirectory = Environment.GetFolderPath(Environment.SpecialFolder.UserProfile);
        string pictureDirectory = Path.Combine(userDirectory, "Desktop");
        string pictureFileName = Path.Combine(pictureDirectory, "sampleDocument");

        //  To specify what type of file you want to save, create a IPsSaveOptions object
        IPsJPEGSaveOptions jpegSaveOptions = app.CreateJPEGSaveOptions();

        //  Each IPsSaveOption class has different properties
        jpegSaveOptions.Quality = 10;
        jpegSaveOptions.FormatOptions = EPsFormatOptionsType.psProgressive;
        jpegSaveOptions.Scans = 3;

        IPsPNGSaveOptions pngSaveOptions = app.CreatePNGSaveOptions();

        //    Call the SaveAs method with full file path, save options, and if you want to save as a copy
        document.SaveAs(pictureFileName, pngSaveOptions, false);
    }
}