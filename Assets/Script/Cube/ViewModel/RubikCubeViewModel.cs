using System;
using System.Collections.Generic;
using UnityEngine;

public class RubikCubeViewModel
{
    private RubikCubeModel _model;
    private bool _isAnimating;

    // ================= EVENTS FOR VIEW =================

    // Вращение слоя: ось, слой (0-2), направление
    public event Action<CubeAxis, int, RotationDirection> OnLayerRotationStarted;

    // Когда модель уже обновлена и можно перерисовать цвета
    public event Action<Dictionary<CubeSide, CubeFaceModel>> OnCubeDataChanged;
    
    private Queue<(CubeAxis axis, int layer, RotationDirection dir)> _shuffleQueue;
    private bool _isShuffling;


    public event Action OnRotationCompleted;
    public event Action OnCubeSolved;

    public event Action OnShuffleCompleted;
    public bool IsAnimating => _isAnimating;

    // ================= INIT =================

    public RubikCubeViewModel()
    {
        _model = new RubikCubeModel();
        _isAnimating = false;
    }

    // ================= DATA =================

    public CubeFaceModel GetFaceData(CubeSide side)
    {
        return _model.GetFace(side);
    }

    // ================= ROTATION API =================

    public bool TryRotateLayer(CubeAxis axis, int layer, RotationDirection dir)
    {
        if (_isAnimating)
            return false;

        StartLayerRotation(axis, layer, dir);
        return true;
    }

    private void StartLayerRotation(CubeAxis axis, int layer, RotationDirection dir)
    {
        _isAnimating = true;

        // Сообщаем View — запускай анимацию
        OnLayerRotationStarted?.Invoke(axis, layer, dir);

        // Меняем модель (данные меняются сразу)
        switch (axis)
        {
            case CubeAxis.X:
                _model.RotateLayerX(layer, dir);
                break;
            case CubeAxis.Y:
                _model.RotateLayerY(layer, dir);
                break;
            case CubeAxis.Z:
                _model.RotateLayerZ(layer, dir);
                break;
        }
    }

    // Вызывается View после окончания анимации
    public void CompleteRotation()
    {
        Debug.Log("I complete rotation");
        _isAnimating = false;
        OnCubeDataChanged?.Invoke(_model.Faces);
        OnRotationCompleted?.Invoke();

        if (_model.IsSolved())
            OnCubeSolved?.Invoke();
        if (_isShuffling)
            PlayNextShuffleMove();

    }

    // ================= UTIL =================

    public void ShuffleCube(int moves = 25)
    {
        if (_isAnimating)
            return;

        ShuffleAnimated(moves);
    }

    public void ResetCube()
    {
        if (_isAnimating)
            return;

        _model = new RubikCubeModel();
        OnCubeDataChanged?.Invoke(_model.Faces);
    }

    public bool IsCubeSolved()
    {
        return _model.IsSolved();
    }
    
    public void ShuffleAnimated(int moves = 25)
    {
        if (_isAnimating || _isShuffling)
            return;
        _shuffleQueue = new Queue<(CubeAxis, int, RotationDirection)>();

        var rnd = new System.Random();

        for (int i = 0; i < moves; i++)
        {
            var axis = (CubeAxis)rnd.Next(2);
            int layer = rnd.Next(3);
            var dir = rnd.Next(2) == 0
                ? RotationDirection.Clockwise
                : RotationDirection.CounterClockwise;

            _shuffleQueue.Enqueue((axis, layer, dir));
        }
        _isShuffling = true;
        PlayNextShuffleMove();

    }
    
    private void PlayNextShuffleMove()
    {
        if (_shuffleQueue.Count == 0)
        {
            _isShuffling = false;
            
            OnShuffleCompleted?.Invoke();
            return;
        }

        var move = _shuffleQueue.Dequeue();

        // это вызовет анимацию во View
        TryRotateLayer(move.axis, move.layer, move.dir);
       

    }


}
public enum CubeAxis
{
    X, // Left <-> Right
    Y, // Top <-> Bottom
    Z  // Back <-> Front
}
