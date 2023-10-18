using Godot;
using GodotSharper;

namespace Weave;

public partial class Camera : Camera2D
{
    private Vector2 _desiredPosition;
    private Vector2 _desiredZoom;
    private float _desiredRotation;
    private float _desiredLerpStrength = 3f;
    private float _lerpStrength = 3f;

    public override void _Ready()
    {
        Reset();
    }

    public override void _Process(double delta)
    {
        Position = Position.Lerp(_desiredPosition, (float)delta * _lerpStrength);
        Zoom = Zoom.Lerp(_desiredZoom, (float)delta * _lerpStrength);
        Rotation = Mathf.Lerp(Rotation, _desiredRotation, (float)delta * _lerpStrength);
        _lerpStrength = Mathf.Lerp(_lerpStrength, _desiredLerpStrength, (float)delta * 0.01f);
    }

    private void Reset()
    {
        _desiredPosition = new(800, 450);
        _desiredZoom = new(1, 1);
        _desiredRotation = 0;
    }

    public void OnGameOver(Vector2 collisionPosition)
    {
        _desiredPosition = collisionPosition;
        _desiredRotation = 0.3f;
        _desiredZoom = new(2f, 2f);

        AddChild(
            TimerFactory.StartedSelfDestructingOneShot(3, () =>
            {
                _lerpStrength = 0f;
                Reset();
            })
        );
    }
}
