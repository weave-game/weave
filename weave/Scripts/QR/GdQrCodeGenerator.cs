using Godot;

namespace Weave.QR;

public class GdQrCodeGenerator : IQrCodeGenerator
{
    private const string QrCodeGdScriptPath = "res://Scripts/QRCodeGenerator/qr_code.gd";

    public ImageTexture GenerateQrCodeFromString(string str)
    {
        var myGdScript = GD.Load<GDScript>(QrCodeGdScriptPath);
        var myGdScriptNode = (GodotObject)myGdScript.New();
        return (ImageTexture)myGdScriptNode.Call("get_texture", str);
    }
}
