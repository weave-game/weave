using Godot;

namespace Weave.QR;

public interface IQrCodeGenerator
{
    ImageTexture GenerateQrCodeFromString(string str);
}
