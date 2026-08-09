# Unity Project Instructions

This repository is a Unity project. Treat the running Unity Editor and the connected `unityMCP` server as the source of truth for editor state.

## Project context

- Unity version: 6000.3.x.
- Rendering: Universal Render Pipeline (URP).
- Input: Unity Input System.
- Main gameplay code is under `Assets/Script/`.
- Main scenes are `Assets/Scenes/Menu.unity` and `Assets/Scenes/Game.unity`.
- The worktree may contain user changes. Never discard, overwrite, or reformat unrelated changes.

## Tool selection

- Choose the specialized tool that owns the artifact or state; do not substitute a generic shell/script workflow when a suitable skill or Unity MCP tool exists.
- For new raster visual assets or raster edits - 2D art, textures, sprites, icons, backgrounds, UI art, illustrations, cutouts, and similar bitmap content - automatically use the ImageGen skill. Do not wait for the user to explicitly mention ImageGen.
- Do not use ImageGen when the correct deliverable is an existing editable SVG/vector asset, a code-native graphic, or a deterministic Unity-generated primitive.
- For generated or imported 3D content, use the dedicated Unity MCP model generation/import workflow when it is available. Use Unity primitives or procedural geometry only for simple deterministic shapes. Verify scale, orientation, pivot, materials, rig, colliders, and prefab setup in Unity.
- For audio, music, video, vector art, fonts, and other non-raster content, use a dedicated installed skill or tool when one exists. Do not misuse ImageGen or fabricate a binary asset with a generic script. If no suitable generator is available, explain the limitation and use an existing or user-provided asset.
- Prefer reusing or editing suitable existing project assets over generating near-duplicates.
- For project-bound generated images, move or copy the selected final output into an appropriate subfolder under `Assets/`; never leave a referenced asset only in Codex's generated-image storage or a temporary folder.
- Do not overwrite an existing visual asset unless replacement was explicitly requested. Otherwise use a clear new or versioned filename.
- Import generated images through Unity MCP and configure the correct Unity import type and settings for their use (for example Sprite, texture, transparency, filtering, compression, pixels per unit, or wrap mode).
- Inspect generated output before using it, then inspect it again in its actual Unity context. Regenerate or make a targeted edit when the style, readability, transparency, seams, dimensions, or in-game usage is incorrect.
- Use URP-compatible shaders and materials. Inspect the actual shader and its property names before assigning values; do not guess property names or silently replace a project's shader choice.
- For UI work, preserve the existing UI system and conventions. Verify Canvas mode, Canvas Scaler, anchors, pivots, safe areas, sorting, navigation, and readability at the relevant Game View resolutions.
- For animation work, inspect the Animator Controller, clips, parameters, transitions, avatar/rig, and root-motion settings before editing. Validate transitions in Play Mode.
- Use Unity MCP for Unity-owned state and assets, including scenes, GameObjects, components, prefabs, ScriptableObjects, materials, shaders, animation, UI, tests, Console, Play Mode, and builds.
- Use normal repository editing tools for source code, Markdown, JSON, and other plain-text project files; validate their Unity impact through Unity MCP.

## Unity MCP workflow

- Use the connected Unity MCP whenever a task reads or affects scenes, GameObjects, components, prefabs, ScriptableObjects, materials, shaders, textures, animation, UI, project settings, play mode, builds, the Unity Console, or Unity tests.
- At the start of Unity-related work, confirm that a Unity session is available and target the correct project instance. If the server reports `no_unity_session`, ask the user to press **Connect** in `Window > MCP for Unity`; do not silently target another open project.
- Read `mcpforunity://custom-tools` when project-specific Unity tools may be relevant.
- Inspect the relevant scene hierarchy, components, assets, scripts, and Console state before modifying anything.
- Prefer focused Unity MCP tools over `execute_code`. Use `execute_code` only when no focused tool can perform the required operation safely.
- Use `batch_execute` for repetitive independent Unity operations when appropriate.
- Do not edit `.unity`, `.prefab`, `.asset`, `.mat`, or other Unity-serialized YAML files by hand unless the user explicitly requests it and MCP cannot perform the change safely.
- Do not create, modify, move, or delete Unity objects merely to inspect the project.

## Implementation rules

- Preserve the existing architecture, naming, folder structure, serialization model, and code style.
- Prefer extending an existing system over adding a duplicate manager, service, controller, singleton, event bus, or data model.
- Search for existing implementations and usages before introducing new types or public APIs.
- Keep changes minimal and scoped to the request. Do not modify unrelated files or perform opportunistic refactors.
- Preserve serialized field names. If a serialized field must be renamed, preserve existing data with the appropriate Unity migration mechanism such as `FormerlySerializedAs`.
- Use the Unity Input System already installed in this project. Do not introduce legacy `UnityEngine.Input` polling or a second input architecture.
- Use `Time.deltaTime` for frame-based motion and `Time.fixedDeltaTime`/`FixedUpdate` for physics work. Do not move dynamic Rigidbody objects by directly writing their Transform unless the existing design explicitly requires it.
- Match event, coroutine, async task, and object lifetimes. Unsubscribe and cancel work when the owning object is disabled or destroyed, and handle Unity's destroyed-object semantics safely.
- Keep runtime code free of `UnityEditor` APIs. Put editor-only tools in an `Editor` folder or editor-only assembly.
- Avoid expensive per-frame allocations and repeated scene searches in `Update`, `LateUpdate`, or `FixedUpdate`.
- Cache stable component references and prefer explicit serialized references or existing dependency mechanisms over repeated `Find`, `FindObjectOfType`, `GetComponent`, or `Resources.Load` calls in hot paths.
- Do not add packages or change project-wide settings unless required by the task.
- Never modify generated or transient content in `Library/`, `Temp/`, `Logs/`, `obj/`, generated `.csproj` files, or generated solution files.

## Validation loop

After every code, gameplay, scene, prefab, asset, or project-setting change:

1. Trigger/allow Unity to refresh and wait until compilation and domain reload finish.
2. Read the Unity Console, focusing first on errors and exceptions.
3. Fix every compile error or exception caused by the change. Do not take ownership of unrelated pre-existing warnings unless they block validation.
4. Run the smallest relevant Edit Mode or Play Mode tests when tests exist or the change is testable.
5. Enter Play Mode when runtime behavior must be verified; exit Play Mode after validation unless the user asks otherwise.
6. Inspect the resulting hierarchy, components, serialized values, and runtime state through Unity MCP.
7. For visual, camera, UI, animation, physics, collider, or transform work, inspect the relevant Scene/Game view rather than inferring correctness from serialized values alone.
8. If validation fails, fix the issue and repeat the loop.

Do not claim that a feature works until it has been validated in Unity. If full validation is impossible, state exactly what was checked and what remains unverified.

## Scene and asset safety

- Before changing a scene, identify the active scene and whether it has unsaved changes.
- Do not save unrelated dirty scenes or assets without the user's permission.
- Preserve prefab links and use prefab-aware operations for prefab instances and assets.
- Create a prefab for genuinely reusable configured objects; keep one-off scene composition in the scene. Do not create duplicate Cameras, EventSystems, audio listeners, managers, or persistent roots without inspecting existing ones.
- Move and rename Unity assets through Unity-aware operations so `.meta` files and GUID references are preserved. Never casually delete or regenerate `.meta` files.
- Put imported/generated assets in a clear existing folder when possible and configure their importer deliberately. Do not rely on incidental default import settings.
- Record intentional Unity changes through operations that support Undo when the MCP tool provides that behavior.
- Confirm exact targets before destructive operations. Never delete similarly named objects or assets based only on a partial match.
- Do not leave temporary GameObjects, debug components, generated assets, test scenes, or helper scripts in the project.

## Performance, tests, and builds

- Add focused Edit Mode tests for deterministic logic and Play Mode tests for scene/runtime behavior when the change warrants automated coverage and the project has a compatible test structure.
- For performance-sensitive work, measure with Unity's profiling tools or comparable runtime evidence before claiming an optimization. Do not infer performance from code appearance alone.
- When a change can affect player builds, scenes in build settings, stripping, platform APIs, shaders, Addressables, or asset inclusion, run the smallest relevant build validation through Unity MCP. Do not change the active platform or global quality settings unless the task requires it.
- Treat a clean Editor Console as necessary but not sufficient: also verify the requested behavior, visible result, and relevant edge cases in the running project.

## Reporting

- Lead with the completed outcome.
- Summarize the files and Unity objects changed.
- Report validation performed, including compilation, Console results, tests, and Play Mode/visual inspection when applicable.
- Mention any remaining risks, pre-existing blockers, or manual steps concisely.
