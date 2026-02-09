using System;

public class CubeRotationEvents
{
    internal Action<int, RotationDirection, Action> OnRotateX;
    internal Action<int, RotationDirection, Action> OnRotateY;
    internal Action<int,RotationDirection, Action> OnRotateZ;
}