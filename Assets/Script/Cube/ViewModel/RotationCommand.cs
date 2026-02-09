/// <summary>
/// Команда для вращения грани (используется в ViewModel)
/// </summary>
public class RotationCommand
{
    public CubeSide Side { get; private set; }
    public RotationDirection Direction { get; private set; }

    public RotationCommand(CubeSide side, RotationDirection direction)
    {
        Side = side;
        Direction = direction;
    }
}
