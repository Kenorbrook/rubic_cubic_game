using System.Collections.Generic;
using UnityEngine;

public class BootstrapBattle : MonoBehaviour
{
    
    [SerializeField] private List<Pattern> patterns;
     private RubikCubeViewModel _cube;
     [SerializeField]
     private RubikCubeView _cubeView;

    private Matcher _matcher;

    public void Start()
    {

        _cube = _cubeView.ViewModel;
        var context = new PatternContext
        {
        };

        _matcher = new Matcher(patterns, context);
        _cube.OnCubeDataChanged += _matcher.OnCubeChanged;
        
    }
}
