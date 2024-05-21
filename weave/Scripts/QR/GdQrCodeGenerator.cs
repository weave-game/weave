using System;
using Godot;

namespace Weave.QR;

[Obsolete("QR code generation is currently not supported")]
public class GdQrCodeGenerator : IQrCodeGenerator
{
    private const string QrCodeGdScriptPath = "res://Scripts/QRCodeGenerator/qr_code.gd";

    public ImageTexture GenerateQrCodeFromString(string str)
    {
        throw new ObsoleteException("QR code generation is currently not supported");

        var myGdScript = GD.Load<GDScript>(QrCodeGdScriptPath);
        var myGdScriptNode = (GodotObject)myGdScript.New();
        return (ImageTexture)myGdScriptNode.Call("get_texture", str);
    }
}
