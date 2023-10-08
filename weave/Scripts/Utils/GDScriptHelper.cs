using Godot;

namespace weave;

public static class GDScriptHelper
{
    public static ImageTexture GenerateQRCodeFromString(string str)
    {
        GDScript MyGDScript = (GDScript)GD.Load("res://Scripts/QRCodeGenerator/qr_code.gd");
        GodotObject myGDScriptNode = (GodotObject)MyGDScript.New();
        return (ImageTexture)myGDScriptNode.Call("get_texture", str);
    }
}
