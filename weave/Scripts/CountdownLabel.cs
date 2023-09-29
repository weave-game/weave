using Godot;

namespace weave;

public partial class CountdownLabel : RichTextLabel
{
    public void UpdateLabelText(string text)
    {
        Text = $"[center]{text}[/center]";
    }
}
