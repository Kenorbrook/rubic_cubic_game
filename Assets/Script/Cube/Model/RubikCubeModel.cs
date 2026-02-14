using System;
using System.Collections.Generic;
using UnityEngine;
using Random = System.Random;

public class RubikCubeModel
{
    private Dictionary<CubeSide, CubeFaceModel> _faces;

    public event Action OnCubeChanged;

    public Dictionary<CubeSide, CubeFaceModel> Faces => _faces;

    public RubikCubeModel()
    {
        InitializeCube();
    }

    private void InitializeCube()
    {
        _faces = new Dictionary<CubeSide, CubeFaceModel>
        {
            { CubeSide.Front,  new CubeFaceModel(CubeColor.White) },
            { CubeSide.Back,   new CubeFaceModel(CubeColor.Blue) },
            { CubeSide.Right,  new CubeFaceModel(CubeColor.Red) },
            { CubeSide.Left,   new CubeFaceModel(CubeColor.Orange) },
            { CubeSide.Top,    new CubeFaceModel(CubeColor.Yellow) },
            { CubeSide.Bottom, new CubeFaceModel(CubeColor.Green) },
        };
    }

    public CubeFaceModel GetFace(CubeSide side) => _faces[side];

    // ========================= PUBLIC API =========================

    public void Rotate(CubeAxis axis,
        int layer,
        RotationDirection dir)
    {
        switch (axis)
        {
            case CubeAxis.X:
                RotateLayerX(layer, dir);
                break;
            case CubeAxis.Y:
                RotateLayerY(layer, dir);
                break;
            case CubeAxis.Z:
                RotateLayerZ(layer, dir);
                break;
        }
    }

    private void RotateLayerX(int layer, RotationDirection dir)
    {
        CubeColor[] front  = _faces[CubeSide.Front].GetColumn(layer);
        CubeColor[] top    = _faces[CubeSide.Top].GetColumn(layer);
        CubeColor[] back   = _faces[CubeSide.Back].GetColumn(2 - layer);
        CubeColor[] bottom = _faces[CubeSide.Bottom].GetColumn(layer);

        if (dir == RotationDirection.CounterClockwise)
        {
            
            _faces[CubeSide.Top].SetColumn(layer, front);
            _faces[CubeSide.Back].SetColumn(2 - layer, top);
            _faces[CubeSide.Bottom].SetColumn(layer, back);
            _faces[CubeSide.Front].SetColumn(layer, bottom);
            
        }
        else
        {
            _faces[CubeSide.Bottom].SetColumn(layer, front);
            _faces[CubeSide.Back].SetColumn(2 - layer, bottom);
            _faces[CubeSide.Top].SetColumn(layer, back);
            _faces[CubeSide.Front].SetColumn(layer, top);
        }

        switch (layer)
        {
            case 2:
                RotateFace(CubeSide.Right, Opposite(dir));
                break;
            case 0:
                RotateFace(CubeSide.Left, dir);
                break;
        }

        OnCubeChanged?.Invoke();
    }

    
    private void RotateLayerY(int layer, RotationDirection dir)
    {
        CubeColor[] front = _faces[CubeSide.Front].GetRow(layer);
        CubeColor[] right = _faces[CubeSide.Right].GetRow(layer);
        CubeColor[] back  = _faces[CubeSide.Back].GetRow(layer);
        CubeColor[] left  = _faces[CubeSide.Left].GetRow(layer);

        if (dir == RotationDirection.Clockwise)
        {
            _faces[CubeSide.Right].SetRow(layer, front);
            _faces[CubeSide.Back].SetRow(layer, right);
            _faces[CubeSide.Left].SetRow(layer, back);
            _faces[CubeSide.Front].SetRow(layer, left);
        }
        else
        {
            _faces[CubeSide.Left].SetRow(layer, front);
            _faces[CubeSide.Back].SetRow(layer, left);
            _faces[CubeSide.Right].SetRow(layer, back);
            _faces[CubeSide.Front].SetRow(layer, right);
        }

        switch (layer)
        {
            case 0:
                RotateFace(CubeSide.Top, Opposite(dir));
                break;
            case 2:
                RotateFace(CubeSide.Bottom, dir);
                break;
        }

        OnCubeChanged?.Invoke();
    }

    
    private void RotateLayerZ(int layer, RotationDirection dir)
    {
        var top    = _faces[CubeSide.Top].GetRow(2 - layer);
        var right  = _faces[CubeSide.Right].GetColumn(layer);
        var bottom = _faces[CubeSide.Bottom].GetRow(layer);
        var left   = _faces[CubeSide.Left].GetColumn(2 - layer);

        if (dir == RotationDirection.Clockwise)
        {
            _faces[CubeSide.Right].SetColumn(layer, Reverse(top));
            _faces[CubeSide.Bottom].SetRow(layer, right);
            _faces[CubeSide.Left].SetColumn(2 - layer, Reverse(bottom));
            _faces[CubeSide.Top].SetRow(2 - layer, left);
        }
        else
        {
            _faces[CubeSide.Left].SetColumn(2 - layer, Reverse(top));
            _faces[CubeSide.Bottom].SetRow(layer, left);
            _faces[CubeSide.Right].SetColumn(layer, Reverse(bottom));
            _faces[CubeSide.Top].SetRow(2 - layer, right);
        }

        if (layer == 2) RotateFace(CubeSide.Front, dir);
        if (layer == 0) RotateFace(CubeSide.Back, Opposite(dir));

        OnCubeChanged?.Invoke();
    }

    // ========================= INTERNAL =========================

    private void RotateFace(CubeSide side, RotationDirection dir)
    {
        if (dir == RotationDirection.Clockwise)
            _faces[side].RotateClockwise();
        else
            _faces[side].RotateCounterClockwise();
    }

    private RotationDirection Opposite(RotationDirection d)
    {
        return d == RotationDirection.Clockwise
            ? RotationDirection.CounterClockwise
            : RotationDirection.Clockwise;
    }

    private CubeColor[] Reverse(CubeColor[] src)
    {
        return new CubeColor[] { src[2], src[1], src[0] };
    }

    // ========================= UTILS =========================

    public bool IsSolved()
    {
        foreach (var face in _faces.Values)
        {
            CubeColor c = face.GetCell(0, 0);
            for (int r = 0; r < 3; r++)
                for (int c2 = 0; c2 < 3; c2++)
                    if (face.GetCell(r, c2) != c)
                        return false;
        }
        return true;
    }

    public void Shuffle(int moves = 25)
    {
        var rnd = new Random();

        for (int i = 0; i < moves; i++)
        {
            int axis = rnd.Next(3);
            int layer = rnd.Next(3);
            var dir = rnd.Next(2) == 0 ? RotationDirection.Clockwise : RotationDirection.CounterClockwise;

            switch (axis)
            {
                case 0: RotateLayerX(layer, dir); break;
                case 1: RotateLayerY(layer, dir); break;
                case 2: RotateLayerZ(layer, dir); break;
            }
        }
    }
}
