using Godot;

namespace Weave;

public static class GDScriptHelper
{
    public static ImageTexture GenerateQRCodeFromString(string str)
    {
        var MyGDScript = (GDScript)GD.Load("res://Scripts/QRCodeGenerator/qr_code.gd");
        var myGDScriptNode = (GodotObject)MyGDScript.New();
        return (ImageTexture)myGDScriptNode.Call("get_texture", str);
    }
}
