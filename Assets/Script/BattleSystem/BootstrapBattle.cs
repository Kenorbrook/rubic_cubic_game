using System.Collections.Generic;
using UnityEngine;

namespace BattleSystem
{
    public class BootstrapBattle : MonoBehaviour
    {

        [SerializeField]
        private List<Pattern> patterns;

        [SerializeField]
        private RubikCubeView _cubeView;


        public void Start()
        {
            var context = new PatternContext
            {
            };


            var model = new RubikCubeModel();
            var matcher = new Matcher(patterns);
            var applier = new PatternApplier(context);

            var vm = new RubikCubeViewModel(model);
            vm.Bind(matcher, applier);

            _cubeView.Initialize(vm);

        }
    }
}