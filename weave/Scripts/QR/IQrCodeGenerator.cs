using System;
using Godot;

namespace Weave.QR;

[Obsolete("QR code generation is currently not supported")]
public interface IQrCodeGenerator
{
    ImageTexture GenerateQrCodeFromString(string str);
}
