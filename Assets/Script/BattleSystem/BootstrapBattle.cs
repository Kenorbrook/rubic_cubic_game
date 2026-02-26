using System.Collections.Generic;
using UnityEngine;

namespace BattleSystem
{
    public class BootstrapBattle : MonoBehaviour
    {
        [SerializeField] private List<Pattern> patterns;
        [SerializeField] private RubikCubeView _cubeView;
        [SerializeField] private FrontFaceInputController _frontFaceInputController;

        private PatternContext _context;
        private RubikCubeViewModel _viewModel;
        private bool _isInitialized;
        
        public FrontFaceInputController FrontFaceInputController => _frontFaceInputController;

        public void Initialize(PatternContext patternContext)
        {
            if (_isInitialized)
                return;

            _context = patternContext ?? new PatternContext();

            var model = new RubikCubeModel();
            var matcher = new Matcher(patterns);
            var applier = new PatternApplier(_context);

            _viewModel = new RubikCubeViewModel(model);
            _viewModel.Bind(matcher, applier);
            _viewModel.OnLogicalMoveCompleted += OnLogicalMoveCompleted;
            _viewModel.OnShuffleStateChanged += OnShuffleStateChanged;

            _cubeView.Initialize(_viewModel);
            _isInitialized = true;
        }

        private void OnDestroy()
        {
            if (_viewModel != null)
            {
                _viewModel.OnLogicalMoveCompleted -= OnLogicalMoveCompleted;
                _viewModel.OnShuffleStateChanged -= OnShuffleStateChanged;
            }
        }

        private void OnLogicalMoveCompleted(MoveSource moveSource)
        {
            if (moveSource != MoveSource.User)
                return;

            _context?.PlayerSwiped?.Invoke();
        }

        private void OnShuffleStateChanged(bool isShuffling)
        {
            _context?.ShuffleStateChanged?.Invoke(isShuffling);
        }
    }
}
