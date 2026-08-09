using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.Android;
using UnityEngine.UI;

public sealed class RubikCube3DPresenter : MonoBehaviour, IPointerDownHandler, IDragHandler, IPointerUpHandler
{
    private readonly List<Transform> _cubies = new();
    private readonly Dictionary<Transform, Vector3> _canonicalCubiePositions = new();
    private readonly Dictionary<CubeSide, Renderer[]> _stickers = new();
    private readonly Dictionary<CubeColor, Material> _colorMaterials = new();
    private Material _bodyMaterial;
    private RubikCubeView _owner;
    private RubikCubeViewModel _viewModel;
    private RectTransform _surface;
    private RawImage _image;
    private Camera _camera;
    private Transform _cubeRoot;
    private RenderTexture _texture;
    private Vector2 _pressPosition;
    private Vector2 _lastPointer;
    private Vector2 _pressedProjectedRight;
    private Vector2 _pressedProjectedUp;
    private CubeStickerHit _pressedSticker;
    private bool _viewDrag;
    private GravitySensor _gravitySensor;
    private AttitudeSensor _attitudeSensor;
    private bool _sensorCalibrated;
    private float _sensorOriginHorizontal;
    private float _sensorOriginVertical;
    private float _yaw;
    private float _pitch;
    private bool _renderDirty;

    public bool IsVisible => _image != null && _image.gameObject.activeSelf;

    public void Initialize(RubikCubeView owner, RubikCubeViewModel viewModel)
    {
        _owner = owner;
        _viewModel = viewModel;
        _gravitySensor = FindGravitySensor();
        _attitudeSensor = FindAttitudeSensor();
        BuildSurface();
        BuildCube();
        SetVisible(CubeViewPreferences.Use3D);
    }

    public void SetVisible(bool visible)
    {
        if (_image == null) return;
        if (visible)
            RecenterView();
        _image.gameObject.SetActive(visible);
        _renderDirty |= visible;
        CubeViewPreferences.Use3D = visible;
    }

    public void RecenterView()
    {
        _yaw = 0f;
        _pitch = 0f;
        _sensorOriginHorizontal = 0f;
        _sensorOriginVertical = 0f;
        _sensorCalibrated = false;

        if (_cubeRoot != null)
            _cubeRoot.localRotation = Quaternion.identity;

        _renderDirty = true;
    }

    public void AnimateLayer(CubeAxis axis, int index, RotationDirection direction, MoveSource source)
    {
        StartCoroutine(RotateLayer(axis, index, direction, source == MoveSource.Shuffle ? 0.035f : 0.24f));
    }

    public void RefreshColors()
    {
        ApplyFace(CubeSide.Front, _viewModel.GetFaceData(CubeSide.Front));
        ApplyFace(CubeSide.Back, _viewModel.GetFaceData(CubeSide.Back));
        ApplyFace(CubeSide.Left, _viewModel.GetFaceData(CubeSide.Left));
        ApplyFace(CubeSide.Right, _viewModel.GetFaceData(CubeSide.Right));
        ApplyFace(CubeSide.Top, _viewModel.GetFaceData(CubeSide.Top));
        ApplyFace(CubeSide.Bottom, _viewModel.GetFaceData(CubeSide.Bottom));
        _renderDirty = true;
    }

    private void Update()
    {
        if (!IsVisible || _cubeRoot == null) return;
        if (_pressedSticker == null && !_viewDrag && TryReadTiltAngles(out var horizontal, out var vertical))
        {
            if (!_sensorCalibrated)
            {
                _sensorOriginHorizontal = horizontal;
                _sensorOriginVertical = vertical;
                _sensorCalibrated = true;
            }
            else
            {
                ApplySensorTilt(
                    Mathf.DeltaAngle(_sensorOriginHorizontal, horizontal),
                    Mathf.DeltaAngle(_sensorOriginVertical, vertical));
            }
        }
        var targetRotation = Quaternion.Euler(_pitch, _yaw, 0f);
        if (Quaternion.Angle(_cubeRoot.localRotation, targetRotation) > 0.01f)
        {
            _cubeRoot.localRotation = targetRotation;
            _renderDirty = true;
        }
    }

    private bool TryReadTiltAngles(out float horizontal, out float vertical)
    {
        horizontal = 0f;
        vertical = 0f;

        _gravitySensor ??= FindGravitySensor();
        if (_gravitySensor != null)
        {
            if (!_gravitySensor.enabled)
            {
                InputSystem.EnableDevice(_gravitySensor);
                _sensorCalibrated = false;
                return false;
            }

            if (!_gravitySensor.wasUpdatedThisFrame)
                return false;

            return TryGetGravityAngles(_gravitySensor.gravity.ReadValue(), out horizontal, out vertical);
        }

        // Older devices without a dedicated gravity sensor can still derive the
        // same tilt-only vector from attitude. Heading is discarded here too.
        _attitudeSensor ??= FindAttitudeSensor();
        if (_attitudeSensor == null)
            return false;

        if (!_attitudeSensor.enabled)
        {
            InputSystem.EnableDevice(_attitudeSensor);
            _sensorCalibrated = false;
            return false;
        }

        if (!_attitudeSensor.wasUpdatedThisFrame)
            return false;

        var gravity = Quaternion.Inverse(_attitudeSensor.attitude.ReadValue()) * Vector3.down;
        return TryGetGravityAngles(gravity, out horizontal, out vertical);
    }

    private static bool TryGetGravityAngles(Vector3 gravity, out float horizontal, out float vertical)
    {
        horizontal = 0f;
        vertical = 0f;
        if (gravity.sqrMagnitude < 0.01f)
            return false;

        gravity.Normalize();
        horizontal = Mathf.Asin(Mathf.Clamp(gravity.x, -1f, 1f)) * Mathf.Rad2Deg;
        vertical = Mathf.Atan2(gravity.z, gravity.y) * Mathf.Rad2Deg;
        return true;
    }

    private static GravitySensor FindGravitySensor()
    {
#if UNITY_ANDROID
        return InputSystem.GetDevice<AndroidGravitySensor>() ?? GravitySensor.current;
#else
        return GravitySensor.current;
#endif
    }

    private static AttitudeSensor FindAttitudeSensor()
    {
#if UNITY_ANDROID
        // Android can expose two attitude devices. Keeping one stable device is
        // important: AttitudeSensor.current may jump between them as activity
        // changes, which produces a sudden change of axes/orientation.
        return InputSystem.GetDevice<AndroidRotationVector>()
            ?? InputSystem.GetDevice<AndroidGameRotationVector>()
            ?? AttitudeSensor.current;
#else
        return AttitudeSensor.current;
#endif
    }

    private void LateUpdate()
    {
        if (!IsVisible || !_renderDirty || _camera == null || _texture == null || !_texture.IsCreated())
            return;

        _camera.Render();
        _renderDirty = false;
    }

    public void OnPointerDown(PointerEventData eventData)
    {
        _pressPosition = eventData.position;
        _lastPointer = eventData.position;
        _viewDrag = eventData.button == PointerEventData.InputButton.Right;
        _pressedSticker = _viewDrag ? null : RaycastSticker(eventData.position, eventData.pressEventCamera);
        if (_pressedSticker != null)
            GetProjectedCubeAxes(out _pressedProjectedRight, out _pressedProjectedUp);
    }

    public void OnDrag(PointerEventData eventData)
    {
        if (!_viewDrag) return;
        var delta = eventData.position - _lastPointer;
        _lastPointer = eventData.position;
        _yaw = Mathf.Clamp(_yaw - delta.x * 0.22f, -60f, 60f);
        _pitch = Mathf.Clamp(_pitch + GetMousePitchDelta(delta.y), -60f, 60f);
    }

    private static float GetMousePitchDelta(float pointerDeltaY)
    {
        var inversion = CubeViewPreferences.InvertVertical ? 1f : -1f;
        return pointerDeltaY * 0.22f * inversion;
    }

    private void ApplySensorTilt(float relativeHorizontal, float relativeVertical)
    {
        // Gravity contains tilt but no compass heading. Rotating together with a
        // chair therefore cannot move the cube; only tilting the screen can.
        var pitchSign = CubeViewPreferences.InvertVertical ? -1f : 1f;
        var targetYaw = Mathf.Clamp(-relativeHorizontal * 2.4f, -60f, 60f);
        var targetPitch = Mathf.Clamp(relativeVertical * 2.4f * pitchSign, -60f, 60f);
        var blend = 1f - Mathf.Exp(-6f * Time.deltaTime);

        _yaw = Mathf.LerpAngle(_yaw, targetYaw, blend);
        _pitch = Mathf.LerpAngle(_pitch, targetPitch, blend);
    }

    public void OnPointerUp(PointerEventData eventData)
    {
        if (_viewDrag)
        {
            _viewDrag = false;
            _pressedSticker = null;
            return;
        }

        var pressedSticker = _pressedSticker;
        _pressedSticker = null;
        if (_viewModel.IsAnimating || pressedSticker == null) return;
        var delta = eventData.position - _pressPosition;
        if (delta.sqrMagnitude < 900f) return;
        var cubiePosition = pressedSticker.Cubie.localPosition / 1.02f;
        var row = Mathf.Clamp(1 - Mathf.RoundToInt(cubiePosition.y), 0, 2);
        var col = Mathf.Clamp(Mathf.RoundToInt(cubiePosition.x) + 1, 0, 2);
        var normalizedDelta = delta.normalized;
        var horizontalAmount = Vector2.Dot(normalizedDelta, _pressedProjectedRight);
        var verticalAmount = Vector2.Dot(normalizedDelta, _pressedProjectedUp);
        bool success;
        if (Mathf.Abs(horizontalAmount) > Mathf.Abs(verticalAmount))
            success = _owner.RequestRowRotation(row, horizontalAmount > 0f ? RotationDirection.Clockwise : RotationDirection.CounterClockwise, MoveSource.User);
        else
            success = _owner.RequestColumnRotation(col, verticalAmount < 0f ? RotationDirection.Clockwise : RotationDirection.CounterClockwise, MoveSource.User);

        if (!success)
            return;

        var screenDirection = Mathf.Abs(delta.x) >= Mathf.Abs(delta.y)
            ? (delta.x >= 0f ? SwipeDirection.Right : SwipeDirection.Left)
            : (delta.y >= 0f ? SwipeDirection.Up : SwipeDirection.Down);
        _owner.Notify3DUserSwipe(screenDirection);
    }

    private CubeStickerHit RaycastSticker(Vector2 screenPosition, Camera eventCamera)
    {
        if (_camera == null || _surface == null ||
            !RectTransformUtility.ScreenPointToLocalPointInRectangle(_surface, screenPosition, eventCamera, out var local))
            return null;

        var rect = _surface.rect;
        var viewport = new Vector2(
            Mathf.InverseLerp(rect.xMin, rect.xMax, local.x),
            Mathf.InverseLerp(rect.yMin, rect.yMax, local.y));
        Physics.SyncTransforms();
        var ray = _camera.ViewportPointToRay(new Vector3(viewport.x, viewport.y, 0f));
        return Physics.Raycast(ray, out var hit, 50f, 1 << 29, QueryTriggerInteraction.Ignore)
            ? hit.collider.GetComponent<CubeStickerHit>()
            : null;
    }

    private void GetProjectedCubeAxes(out Vector2 right, out Vector2 up)
    {
        var center = _camera.WorldToViewportPoint(_cubeRoot.position);
        var rightPoint = _camera.WorldToViewportPoint(_cubeRoot.TransformPoint(Vector3.right));
        var upPoint = _camera.WorldToViewportPoint(_cubeRoot.TransformPoint(Vector3.up));
        right = new Vector2(rightPoint.x - center.x, rightPoint.y - center.y).normalized;
        up = new Vector2(upPoint.x - center.x, upPoint.y - center.y).normalized;
    }

    private void BuildSurface()
    {
        var go = new GameObject("Cube3DView", typeof(RectTransform), typeof(CanvasRenderer), typeof(RawImage));
        _surface = go.GetComponent<RectTransform>();
        _surface.SetParent(transform, false);
        _surface.anchorMin = new Vector2(0.5f, 0.5f);
        _surface.anchorMax = new Vector2(0.5f, 0.5f);
        _surface.sizeDelta = new Vector2(760f, 760f);
        _surface.anchoredPosition = new Vector2(0f, -105f);
        _image = go.GetComponent<RawImage>();
        _image.color = Color.white;
        go.AddComponent<RubikCube3DInputForwarder>().Target = this;

        var textureSize = Application.isMobilePlatform ? 384 : 640;
        _texture = new RenderTexture(textureSize, textureSize, 16, RenderTextureFormat.ARGB32)
        {
            antiAliasing = Application.isMobilePlatform ? 1 : 2,
            name = "RubikCube3D_RT",
            useMipMap = false,
            autoGenerateMips = false
        };
        _texture.Create();
        if (!_texture.IsCreated())
            Debug.LogError("Rubik cube RenderTexture could not be created on this device.");
        _image.texture = _texture;
        var cameraGo = new GameObject("Cube3DCamera");
        cameraGo.transform.SetParent(transform, false);
        _camera = cameraGo.AddComponent<Camera>();
        // The cube camera is rendered explicitly only when its texture changed.
        // This also avoids stale first-frame RenderTextures on some Android GPUs.
        _camera.enabled = false;
        _camera.transform.localPosition = new Vector3(0f, 0f, 10.5f);
        _camera.transform.localRotation = Quaternion.Euler(0f, 180f, 0f);
        _camera.clearFlags = CameraClearFlags.SolidColor;
        _camera.backgroundColor = new Color(0f, 0f, 0f, 0f);
        _camera.targetTexture = _texture;
        _camera.fieldOfView = 34f;
        _camera.nearClipPlane = 0.001f;
        _camera.farClipPlane = 20f;
        _camera.cullingMask = 1 << 29;
        var light = new GameObject("CubeKeyLight").AddComponent<Light>();
        light.transform.SetParent(cameraGo.transform, false);
        light.transform.localPosition = new Vector3(-3f, 4f, -3f);
        light.transform.localRotation = Quaternion.Euler(28f, -35f, 0f);
        light.type = LightType.Directional;
        light.intensity = 0.85f;
        light.cullingMask = 1 << 29;
    }

    private void BuildCube()
    {
        EnsureSharedMaterials();
        _canonicalCubiePositions.Clear();
        _cubeRoot = new GameObject("GeneratedRubikCube").transform;
        _cubeRoot.SetParent(transform, false);
        _cubeRoot.gameObject.layer = 29;
        for (var z = 0; z < 3; z++) for (var y = 0; y < 3; y++) for (var x = 0; x < 3; x++)
        {
            var cubie = GameObject.CreatePrimitive(PrimitiveType.Cube);
            cubie.name = $"Cubie_{x}_{y}_{z}";
            cubie.layer = 29;
            cubie.transform.SetParent(_cubeRoot, false);
            cubie.transform.localPosition = new Vector3(x - 1, y - 1, z - 1) * 1.02f;
            cubie.transform.localScale = Vector3.one * 0.94f;
            cubie.GetComponent<Renderer>().sharedMaterial = _bodyMaterial;
            Destroy(cubie.GetComponent<Collider>());
            _cubies.Add(cubie.transform);
            _canonicalCubiePositions[cubie.transform] = cubie.transform.localPosition;
        }
        CreateFace(CubeSide.Front, Vector3.forward, new Vector3(0f, 0f, 1.51f), Quaternion.identity);
        CreateFace(CubeSide.Back, Vector3.back, new Vector3(0f, 0f, -1.51f), Quaternion.Euler(0f, 180f, 0f));
        CreateFace(CubeSide.Right, Vector3.right, new Vector3(1.51f, 0f, 0f), Quaternion.Euler(0f, 90f, 0f));
        CreateFace(CubeSide.Left, Vector3.left, new Vector3(-1.51f, 0f, 0f), Quaternion.Euler(0f, -90f, 0f));
        CreateFace(CubeSide.Top, Vector3.up, new Vector3(0f, 1.51f, 0f), Quaternion.Euler(-90f, 0f, 0f));
        CreateFace(CubeSide.Bottom, Vector3.down, new Vector3(0f, -1.51f, 0f), Quaternion.Euler(90f, 0f, 0f));
        RefreshColors();
    }

    private void CreateFace(CubeSide side, Vector3 normal, Vector3 center, Quaternion rotation)
    {
        var renderers = new Renderer[9];
        for (var row = 0; row < 3; row++) for (var col = 0; col < 3; col++)
        {
            var sticker = GameObject.CreatePrimitive(PrimitiveType.Cube);
            sticker.name = $"{side}_{row}_{col}";
            sticker.layer = 29;
            sticker.transform.SetParent(_cubeRoot, false);
            var right = rotation * Vector3.right;
            var up = rotation * Vector3.up;
            sticker.transform.localPosition = center + right * (col - 1) * 1.02f + up * (1 - row) * 1.02f + normal * 0.005f;
            sticker.transform.localRotation = rotation;
            sticker.transform.localScale = new Vector3(0.82f, 0.82f, 0.035f);
            Transform nearest = null;
            var nearestDistance = float.MaxValue;
            foreach (var cubie in _cubies)
            {
                var distance = Vector3.SqrMagnitude(cubie.localPosition - (sticker.transform.localPosition - normal * 0.49f));
                if (distance < nearestDistance) { nearestDistance = distance; nearest = cubie; }
            }
            if (nearest != null) sticker.transform.SetParent(nearest, true);
            var hit = sticker.AddComponent<CubeStickerHit>();
            hit.Cubie = nearest;
            hit.Side = side;
            var renderer = sticker.GetComponent<Renderer>();
            renderer.sharedMaterial = _colorMaterials[CubeColor.White];
            renderers[row * 3 + col] = renderer;
        }
        _stickers[side] = renderers;
    }

    private void ApplyFace(CubeSide side, CubeFaceModel face)
    {
        if (!_stickers.TryGetValue(side, out var renderers)) return;
        for (var row = 0; row < 3; row++) for (var col = 0; col < 3; col++)
            renderers[row * 3 + col].sharedMaterial = _colorMaterials[face.GetCell(row, col)];
    }

    private void EnsureSharedMaterials()
    {
        if (_bodyMaterial == null)
            _bodyMaterial = MakeMaterial(new Color(0.025f, 0.03f, 0.045f), 0.55f);
        if (_colorMaterials.Count > 0) return;
        foreach (CubeColor cubeColor in System.Enum.GetValues(typeof(CubeColor)))
            _colorMaterials[cubeColor] = MakeMaterial(cubeColor.ToUnityColor(), 0.28f);
    }

    private Material MakeMaterial(Color color, float smoothness)
    {
        var template = Resources.Load<Material>("Materials/RubikCubeTemplate");
        if (template == null)
            throw new System.InvalidOperationException("RubikCubeTemplate material is missing from Resources/Materials.");

        var material = new Material(template);
        if (material.HasProperty("_Color")) material.SetColor("_Color", color);
        else material.color = color;
        if (material.HasProperty("_Smoothness")) material.SetFloat("_Smoothness", smoothness);
        return material;
    }

    private IEnumerator RotateLayer(CubeAxis axis, int index, RotationDirection direction, float duration)
    {
        var pivot = new GameObject("LayerPivot").transform;
        pivot.SetParent(_cubeRoot, false);
        var selected = new List<Transform>(9);
        foreach (var cubie in _cubies)
        {
            var coordinate = axis == CubeAxis.X ? cubie.localPosition.x : axis == CubeAxis.Y ? cubie.localPosition.y : cubie.localPosition.z;
            var target = axis == CubeAxis.Y ? 1 - index : index - 1;
            if (Mathf.Abs(coordinate - target * 1.02f) < 0.2f) { selected.Add(cubie); cubie.SetParent(pivot, true); }
        }
        var rotationAxis = axis == CubeAxis.X ? Vector3.right : axis == CubeAxis.Y ? Vector3.up : Vector3.forward;
        // X/Y layer directions describe where the visible front stickers travel.
        // Around those world axes Unity's positive rotation follows that movement;
        // a front-face (Z) turn uses the usual screen-space clockwise sign instead.
        var clockwiseAngle = axis == CubeAxis.Z ? -90f : 90f;
        var angle = direction == RotationDirection.Clockwise ? clockwiseAngle : -clockwiseAngle;
        var elapsed = 0f;
        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            pivot.localRotation = Quaternion.AngleAxis(angle * Mathf.SmoothStep(0f, 1f, elapsed / duration), rotationAxis);
            _renderDirty = true;
            yield return null;
        }
        foreach (var cubie in selected) cubie.SetParent(_cubeRoot, true);
        foreach (var cubie in _cubies)
        {
            cubie.localPosition = _canonicalCubiePositions[cubie];
            cubie.localRotation = Quaternion.identity;
            cubie.localScale = Vector3.one * 0.94f;
        }
        Destroy(pivot.gameObject);
        _renderDirty = true;
        _owner.CompleteVisualRotation();
    }

    private void OnDestroy()
    {
        if (_texture != null) { _texture.Release(); Destroy(_texture); }
        if (_bodyMaterial != null) Destroy(_bodyMaterial);
        foreach (var material in _colorMaterials.Values)
            if (material != null) Destroy(material);
        _colorMaterials.Clear();
        _canonicalCubiePositions.Clear();
    }
}

public sealed class CubeStickerHit : MonoBehaviour
{
    public Transform Cubie { get; set; }
    public CubeSide Side { get; set; }
}

public sealed class RubikCube3DInputForwarder : MonoBehaviour, IPointerDownHandler, IDragHandler, IPointerUpHandler
{
    public RubikCube3DPresenter Target { get; set; }
    public void OnPointerDown(PointerEventData eventData) => Target?.OnPointerDown(eventData);
    public void OnDrag(PointerEventData eventData) => Target?.OnDrag(eventData);
    public void OnPointerUp(PointerEventData eventData) => Target?.OnPointerUp(eventData);
}
