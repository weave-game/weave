using Godot;

namespace Weave.Utils;

public static class GdScriptHelper
{
    public static ImageTexture GenerateQrCodeFromString(string str)
    {
        var myGdScript = (GDScript)GD.Load("res://Scripts/QRCodeGenerator/qr_code.gd");
        var myGdScriptNode = (GodotObject)myGdScript.New();

        return (ImageTexture)myGdScriptNode.Call("get_texture", str);
    }
}
