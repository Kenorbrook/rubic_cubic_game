using System;
using System.Collections.Generic;
using BattleSystem;
using UnityEngine;

public class RubikCubeViewModel
{
    public bool IsAnimating => _isAnimating;

    public event Action<MoveSource> OnLogicalMoveCompleted;
    public event Action OnCubeSolved;
    public event Action OnInputBlockRequested;
    public event Action OnInputUnblockRequested;
    public event Action<CubeAxis, int, RotationDirection, MoveSource> OnLayerRotationStarted;
    public event Action<Dictionary<CubeSide, CubeFaceModel>> OnCubeDataChanged;
    public event Action<bool> OnShuffleStateChanged;


    private RubikCubeModel _model;
    private bool _isAnimating;


    private Queue<(CubeAxis axis, int layer, RotationDirection dir)> _shuffleQueue;

    private MoveSource _currentMoveSource;
    private ICubeMatcher _matcher;
    private IPatternApplier _patternApplier;
    private bool _isShuffleInProgress;

    public RubikCubeViewModel(RubikCubeModel model)
    {
        _model = model;
        _isAnimating = false;
    }

    public void Dispose()
    {
        OnLogicalMoveCompleted -= MoveComplete;
    }

    public void Bind(ICubeMatcher matcher, IPatternApplier patternApplier)
    {
        _matcher = matcher;
        _patternApplier = patternApplier;
        OnLogicalMoveCompleted += MoveComplete;
    }

    public CubeFaceModel GetFaceData(CubeSide side)
    {
        return _model.GetFace(side);
    }

    public bool TryRotateLayer(CubeAxis axis, int layer, RotationDirection dir, MoveSource source)
    {
        if (_isAnimating)
            return false;

        StartLayerRotation(axis, layer, dir, source);
        return true;
    }

    public void CompleteRotation()
    {
        _isAnimating = false;

        OnCubeDataChanged?.Invoke(_model.Faces);
        OnLogicalMoveCompleted?.Invoke(_currentMoveSource);
        if (_currentMoveSource == MoveSource.User)
        {
            OnInputUnblockRequested?.Invoke();
        }
        else if (_currentMoveSource == MoveSource.Shuffle)
            PlayNextShuffleMove();
    }

    public bool IsCubeSolved()
    {
        OnCubeSolved?.Invoke();
        return _model.IsSolved();
    }

    public void ShuffleCube(int moves)
    {
        if (_isAnimating) return;

        if (!_isShuffleInProgress)
        {
            _isShuffleInProgress = true;
            OnShuffleStateChanged?.Invoke(true);
        }

        OnInputBlockRequested?.Invoke();

        EnqueueShuffleMoves(moves);
        PlayNextShuffleMove();
    }


    public void ResetCube()
    {
        if (_isAnimating)
            return;

        _model = new RubikCubeModel();
        OnCubeDataChanged?.Invoke(_model.Faces);
    }

    private void MoveComplete(MoveSource source)
    {
        if (source != MoveSource.User)
            return;
        var pattern = _matcher.TryMatch(_model.Faces);
        if (pattern != null)
        {
            _patternApplier.Apply(pattern);
            ShuffleCube(10);
        }
    }

    private void EnqueueShuffleMoves(int moves)
    {
        _shuffleQueue = new Queue<(CubeAxis, int, RotationDirection)>();

        var rnd = new System.Random();

        for (int i = 0; i < moves; i++)
        {
            var axis = (CubeAxis) rnd.Next(2);
            int layer = rnd.Next(3);
            var dir = rnd.Next(2) == 0
                ? RotationDirection.Clockwise
                : RotationDirection.CounterClockwise;

            _shuffleQueue.Enqueue((axis, layer, dir));
        }
    }


    private bool NeedFinishShuffle()
    {
        if (_matcher.TryMatch(_model.Faces))
        {
            EnqueueShuffleMoves(2);
            return false;
        }

        if (_isShuffleInProgress)
        {
            _isShuffleInProgress = false;
            OnShuffleStateChanged?.Invoke(false);
        }

        OnInputUnblockRequested?.Invoke();
        return true;
    }


    private void StartLayerRotation(
        CubeAxis axis,
        int layer,
        RotationDirection dir,
        MoveSource source)
    {
        _currentMoveSource = source;
        _isAnimating = true;

        OnLayerRotationStarted?.Invoke(axis, layer, dir, source);

        _model.Rotate(axis, layer, dir);
    }

    private void PlayNextShuffleMove()
    {
        if (_shuffleQueue.Count == 0)
        {
            if (NeedFinishShuffle())
                return;
        }

        var move = _shuffleQueue.Dequeue();
        TryRotateLayer(move.axis, move.layer, move.dir, MoveSource.Shuffle);
    }
}

public enum CubeAxis
{
    X, // Left <-> Right
    Y, // Top <-> Bottom
    Z // Back <-> Front
}

public enum MoveSource
{
    User,
    Shuffle,
    System
}
