using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEditor.IMGUI.Controls;
using UnityEditor.SceneManagement;
using UnityEditorInternal;
using UnityEngine.AI;
using UnityEngine;

namespace UnityEditor.AI
{
    [CanEditMultipleObjects]
    [CustomEditor(typeof(NavMeshSurface))]
    class NavMeshSurfaceEditor : Editor
    {
        SerializedProperty _mAgentTypeId;
        SerializedProperty _mBuildHeightMesh;
        SerializedProperty _mCenter;
        SerializedProperty _mCollectObjects;
        SerializedProperty _mDefaultArea;
        SerializedProperty _mLayerMask;
        SerializedProperty _mOverrideTileSize;
        SerializedProperty _mOverrideVoxelSize;
        SerializedProperty _mSize;
        SerializedProperty _mTileSize;
        SerializedProperty _mUseGeometry;
        SerializedProperty _mVoxelSize;

        class Styles
        {
            public readonly GUIContent MLayerMask = new GUIContent("Include Layers");

            public readonly GUIContent MShowInputGeom = new GUIContent("Show Input Geom");
            public readonly GUIContent MShowVoxels = new GUIContent("Show Voxels");
            public readonly GUIContent MShowRegions = new GUIContent("Show Regions");
            public readonly GUIContent MShowRawContours = new GUIContent("Show Raw Contours");
            public readonly GUIContent MShowContours = new GUIContent("Show Contours");
            public readonly GUIContent MShowPolyMesh = new GUIContent("Show Poly Mesh");
            public readonly GUIContent MShowPolyMeshDetail = new GUIContent("Show Poly Mesh Detail");
        }

        struct AsyncBakeOperation
        {
            public NavMeshSurface Surface;
            public NavMeshData BakeData;
            public AsyncOperation BakeOperation;
        }

        static List<AsyncBakeOperation> _sBakeOperations = new List<AsyncBakeOperation>();

        static Styles _sStyles;

        static bool _sShowDebugOptions;

        static Color _sHandleColor = new Color(127f, 214f, 244f, 100f) / 255;
        static Color _sHandleColorSelected = new Color(127f, 214f, 244f, 210f) / 255;
        static Color _sHandleColorDisabled = new Color(127f * 0.75f, 214f * 0.75f, 244f * 0.75f, 100f) / 255;

        static int _sHandleControlIdHint = typeof(NavMeshSurfaceEditor).Name.GetHashCode();
        BoxBoundsHandle _mBoundsHandle = new BoxBoundsHandle(_sHandleControlIdHint);

        bool editingCollider
        {
            get { return EditMode.editMode == EditMode.SceneViewEditMode.Collider && EditMode.IsOwner(this); }
        }

        void OnEnable()
        {
            _mAgentTypeId = serializedObject.FindProperty("m_AgentTypeID");
            _mBuildHeightMesh = serializedObject.FindProperty("m_BuildHeightMesh");
            _mCenter = serializedObject.FindProperty("m_Center");
            _mCollectObjects = serializedObject.FindProperty("m_CollectObjects");
            _mDefaultArea = serializedObject.FindProperty("m_DefaultArea");
            _mLayerMask = serializedObject.FindProperty("m_LayerMask");
            _mOverrideTileSize = serializedObject.FindProperty("m_OverrideTileSize");
            _mOverrideVoxelSize = serializedObject.FindProperty("m_OverrideVoxelSize");
            _mSize = serializedObject.FindProperty("m_Size");
            _mTileSize = serializedObject.FindProperty("m_TileSize");
            _mUseGeometry = serializedObject.FindProperty("m_UseGeometry");
            _mVoxelSize = serializedObject.FindProperty("m_VoxelSize");

            NavMeshVisualizationSettings.showNavigation++;
        }

        void OnDisable()
        {
            NavMeshVisualizationSettings.showNavigation--;
        }

        static string GetAndEnsureTargetPath(NavMeshSurface surface)
        {
            // Create directory for the asset if it does not exist yet.
            var activeScenePath = surface.gameObject.scene.path;

            var targetPath = "Assets";
            if (!string.IsNullOrEmpty(activeScenePath))
                targetPath = Path.Combine(Path.GetDirectoryName(activeScenePath), Path.GetFileNameWithoutExtension(activeScenePath));
            if (!Directory.Exists(targetPath))
                Directory.CreateDirectory(targetPath);
            return targetPath;
        }

        static void CreateNavMeshAsset(NavMeshSurface surface)
        {
            var targetPath = GetAndEnsureTargetPath(surface);

            var combinedAssetPath = Path.Combine(targetPath, $"NavMesh-{surface.name}.asset");
            combinedAssetPath = AssetDatabase.GenerateUniqueAssetPath(combinedAssetPath);
            AssetDatabase.CreateAsset(surface.navMeshData, combinedAssetPath);
        }

        static NavMeshData GetNavMeshAssetToDelete(NavMeshSurface navSurface)
        {
            var prefabType = PrefabUtility.GetPrefabType(navSurface);
            if (prefabType == PrefabType.PrefabInstance || prefabType == PrefabType.DisconnectedPrefabInstance)
            {
                // Don't allow deleting the asset belonging to the prefab parent
                var parentSurface = PrefabUtility.GetPrefabParent(navSurface) as NavMeshSurface;
                if (parentSurface && navSurface.navMeshData == parentSurface.navMeshData)
                    return null;
            }
            return navSurface.navMeshData;
        }

        void ClearSurface(NavMeshSurface navSurface)
        {
            var assetToDelete = GetNavMeshAssetToDelete(navSurface);
            navSurface.RemoveData();
            navSurface.navMeshData = null;
            EditorUtility.SetDirty(navSurface);

            if (assetToDelete)
            {
                AssetDatabase.DeleteAsset(AssetDatabase.GetAssetPath(assetToDelete));
                EditorSceneManager.MarkSceneDirty(navSurface.gameObject.scene);
            }
        }

        public override void OnInspectorGUI()
        {
            if (_sStyles == null)
                _sStyles = new Styles();

            serializedObject.Update();

            var bs = NavMesh.GetSettingsByID(_mAgentTypeId.intValue);

            if (bs.agentTypeID != -1)
            {
                // Draw image
                const float diagramHeight = 80.0f;
                Rect agentDiagramRect = EditorGUILayout.GetControlRect(false, diagramHeight);
                NavMeshEditorHelpers.DrawAgentDiagram(agentDiagramRect, bs.agentRadius, bs.agentHeight, bs.agentClimb, bs.agentSlope);
            }
            NavMeshComponentsGuiUtility.AgentTypePopup("Agent Type", _mAgentTypeId);

            EditorGUILayout.Space();

            EditorGUILayout.PropertyField(_mCollectObjects);
            if ((CollectObjects)_mCollectObjects.enumValueIndex == CollectObjects.Volume)
            {
                EditorGUI.indentLevel++;
                InspectorEditButtonGui();
                EditorGUILayout.PropertyField(_mSize);
                EditorGUILayout.PropertyField(_mCenter);
            }
            else
            {
                if (editingCollider)
                    EditMode.QuitEditMode();
            }

            EditorGUILayout.PropertyField(_mLayerMask, _sStyles.MLayerMask);
            EditorGUILayout.PropertyField(_mUseGeometry);

            EditorGUILayout.Space();

            EditorGUILayout.Space();

            _mOverrideVoxelSize.isExpanded = EditorGUILayout.Foldout(_mOverrideVoxelSize.isExpanded, "Advanced");
            if (_mOverrideVoxelSize.isExpanded)
            {
                EditorGUI.indentLevel++;

                NavMeshComponentsGuiUtility.AreaPopup("Default Area", _mDefaultArea);

                // Override voxel size.
                EditorGUILayout.PropertyField(_mOverrideVoxelSize);

                using (new EditorGUI.DisabledScope(!_mOverrideVoxelSize.boolValue || _mOverrideVoxelSize.hasMultipleDifferentValues))
                {
                    EditorGUI.indentLevel++;

                    EditorGUILayout.PropertyField(_mVoxelSize);

                    if (!_mOverrideVoxelSize.hasMultipleDifferentValues)
                    {
                        if (!_mAgentTypeId.hasMultipleDifferentValues)
                        {
                            float voxelsPerRadius = _mVoxelSize.floatValue > 0.0f ? (bs.agentRadius / _mVoxelSize.floatValue) : 0.0f;
                            EditorGUILayout.LabelField(" ", $"{voxelsPerRadius:0.00} voxels per agent radius", EditorStyles.miniLabel);
                        }
                        if (_mOverrideVoxelSize.boolValue)
                            EditorGUILayout.HelpBox("Voxel size controls how accurately the navigation mesh is generated from the level geometry. A good voxel size is 2-4 voxels per agent radius. Making voxel size smaller will increase build time.", MessageType.None);
                    }
                    EditorGUI.indentLevel--;
                }

                // Override tile size
                EditorGUILayout.PropertyField(_mOverrideTileSize);

                using (new EditorGUI.DisabledScope(!_mOverrideTileSize.boolValue || _mOverrideTileSize.hasMultipleDifferentValues))
                {
                    EditorGUI.indentLevel++;

                    EditorGUILayout.PropertyField(_mTileSize);

                    if (!_mTileSize.hasMultipleDifferentValues && !_mVoxelSize.hasMultipleDifferentValues)
                    {
                        float tileWorldSize = _mTileSize.intValue * _mVoxelSize.floatValue;
                        EditorGUILayout.LabelField(" ", $"{tileWorldSize:0.00} world units", EditorStyles.miniLabel);
                    }

                    if (!_mOverrideTileSize.hasMultipleDifferentValues)
                    {
                        if (_mOverrideTileSize.boolValue)
                            EditorGUILayout.HelpBox("Tile size controls the how local the changes to the world are (rebuild or carve). Small tile size allows more local changes, while potentially generating more data in overal.", MessageType.None);
                    }
                    EditorGUI.indentLevel--;
                }


                // Height mesh
                using (new EditorGUI.DisabledScope(true))
                {
                    EditorGUILayout.PropertyField(_mBuildHeightMesh);
                }

                EditorGUILayout.Space();
                EditorGUI.indentLevel--;
            }

            EditorGUILayout.Space();

            serializedObject.ApplyModifiedProperties();

            var hadError = false;
            var multipleTargets = targets.Length > 1;
            foreach (NavMeshSurface navSurface in targets)
            {
                var settings = navSurface.GetBuildSettings();
                // Calculating bounds is potentially expensive when unbounded - so here we just use the center/size.
                // It means the validation is not checking vertical voxel limit correctly when the surface is set to something else than "in volume".
                var bounds = new Bounds(Vector3.zero, Vector3.zero);
                if (navSurface.collectObjects == CollectObjects.Volume)
                {
                    bounds = new Bounds(navSurface.center, navSurface.size);
                }

                var errors = settings.ValidationReport(bounds);
                if (errors.Length > 0)
                {
                    if (multipleTargets)
                        EditorGUILayout.LabelField(navSurface.name);
                    foreach (var err in errors)
                    {
                        EditorGUILayout.HelpBox(err, MessageType.Warning);
                    }
                    GUILayout.BeginHorizontal();
                    GUILayout.Space(EditorGUIUtility.labelWidth);
                    if (GUILayout.Button("Open Agent Settings...", EditorStyles.miniButton))
                        NavMeshEditorHelpers.OpenAgentSettings(navSurface.AgentTypeId);
                    GUILayout.EndHorizontal();
                    hadError = true;
                }
            }

            if (hadError)
                EditorGUILayout.Space();

            using (new EditorGUI.DisabledScope(Application.isPlaying || _mAgentTypeId.intValue == -1))
            {
                GUILayout.BeginHorizontal();
                GUILayout.Space(EditorGUIUtility.labelWidth);
                if (GUILayout.Button("Clear"))
                {
                    foreach (NavMeshSurface s in targets)
                        ClearSurface(s);
                    SceneView.RepaintAll();
                }

                if (GUILayout.Button("Bake"))
                {
                    // Remove first to avoid double registration of the callback
                    EditorApplication.update -= UpdateAsyncBuildOperations;
                    EditorApplication.update += UpdateAsyncBuildOperations;

                    foreach (NavMeshSurface surf in targets)
                    {
                        var oper = new AsyncBakeOperation();

                        oper.BakeData = InitializeBakeData(surf);
                        oper.BakeOperation = surf.UpdateNavMesh(oper.BakeData);
                        oper.Surface = surf;

                        _sBakeOperations.Add(oper);
                    }
                }

                GUILayout.EndHorizontal();
            }

            // Show progress for the selected targets
            for (int i = _sBakeOperations.Count - 1; i >= 0; --i)
            {
                if (!targets.Contains(_sBakeOperations[i].Surface))
                    continue;

                var oper = _sBakeOperations[i].BakeOperation;
                if (oper == null)
                    continue;

                var p = oper.progress;
                if (oper.isDone)
                {
                    SceneView.RepaintAll();
                    continue;
                }

                GUILayout.BeginHorizontal();

                if (GUILayout.Button("Cancel", EditorStyles.miniButton))
                {
                    var bakeData = _sBakeOperations[i].BakeData;
                    UnityEngine.AI.NavMeshBuilder.Cancel(bakeData);
                    _sBakeOperations.RemoveAt(i);
                }

                EditorGUI.ProgressBar(EditorGUILayout.GetControlRect(), p, $"Baking: {(int) (100 * p)}%");
                if (p <= 1)
                    Repaint();

                GUILayout.EndHorizontal();
            }
        }

        static NavMeshData InitializeBakeData(NavMeshSurface surface)
        {
            var emptySources = new List<NavMeshBuildSource>();
            var emptyBounds = new Bounds();
            return UnityEngine.AI.NavMeshBuilder.BuildNavMeshData(surface.GetBuildSettings(), emptySources, emptyBounds
                , surface.transform.position, surface.transform.rotation);
        }

        static void UpdateAsyncBuildOperations()
        {
            foreach (var oper in _sBakeOperations)
            {
                if (oper.Surface == null || oper.BakeOperation == null)
                    continue;

                if (oper.BakeOperation.isDone)
                {
                    var surface = oper.Surface;
                    var delete = GetNavMeshAssetToDelete(surface);
                    if (delete != null)
                        AssetDatabase.DeleteAsset(AssetDatabase.GetAssetPath(delete));

                    surface.RemoveData();
                    surface.navMeshData = oper.BakeData;
                    if (surface.isActiveAndEnabled)
                        surface.AddData();
                    CreateNavMeshAsset(surface);
                    EditorSceneManager.MarkSceneDirty(surface.gameObject.scene);
                }
            }
            _sBakeOperations.RemoveAll(o => o.BakeOperation == null || o.BakeOperation.isDone);
            if (_sBakeOperations.Count == 0)
                EditorApplication.update -= UpdateAsyncBuildOperations;
        }

        [DrawGizmo(GizmoType.Selected | GizmoType.Active | GizmoType.Pickable)]
        static void RenderBoxGizmoSelected(NavMeshSurface navSurface, GizmoType gizmoType)
        {
            RenderBoxGizmo(navSurface, gizmoType, true);
        }

        [DrawGizmo(GizmoType.NotInSelectionHierarchy | GizmoType.Pickable)]
        static void RenderBoxGizmoNotSelected(NavMeshSurface navSurface, GizmoType gizmoType)
        {
            if (NavMeshVisualizationSettings.showNavigation > 0)
                RenderBoxGizmo(navSurface, gizmoType, false);
            else
                Gizmos.DrawIcon(navSurface.transform.position, "NavMeshSurface Icon", true);
        }

        static void RenderBoxGizmo(NavMeshSurface navSurface, GizmoType gizmoType, bool selected)
        {
            var color = selected ? _sHandleColorSelected : _sHandleColor;
            if (!navSurface.enabled)
                color = _sHandleColorDisabled;

            var oldColor = Gizmos.color;
            var oldMatrix = Gizmos.matrix;

            // Use the unscaled matrix for the NavMeshSurface
            var localToWorld = Matrix4x4.TRS(navSurface.transform.position, navSurface.transform.rotation, Vector3.one);
            Gizmos.matrix = localToWorld;

            if (navSurface.collectObjects == CollectObjects.Volume)
            {
                Gizmos.color = color;
                Gizmos.DrawWireCube(navSurface.center, navSurface.size);

                if (selected && navSurface.enabled)
                {
                    var colorTrans = new Color(color.r * 0.75f, color.g * 0.75f, color.b * 0.75f, color.a * 0.15f);
                    Gizmos.color = colorTrans;
                    Gizmos.DrawCube(navSurface.center, navSurface.size);
                }
            }
            else
            {
                if (navSurface.navMeshData != null)
                {
                    var bounds = navSurface.navMeshData.sourceBounds;
                    Gizmos.color = Color.grey;
                    Gizmos.DrawWireCube(bounds.center, bounds.size);
                }
            }

            Gizmos.matrix = oldMatrix;
            Gizmos.color = oldColor;

            Gizmos.DrawIcon(navSurface.transform.position, "NavMeshSurface Icon", true);
        }

        void InspectorEditButtonGui()
        {
            var navSurface = (NavMeshSurface)target;
            var bounds = new Bounds(navSurface.transform.position, navSurface.size);

            EditMode.DoEditModeInspectorModeButton(
                EditMode.SceneViewEditMode.Collider,
                "Edit Volume",
                EditorGUIUtility.IconContent("EditCollider"),
                bounds,
                this
                );
        }

        void OnSceneGUI()
        {
            if (!editingCollider)
                return;

            var navSurface = (NavMeshSurface)target;
            var color = navSurface.enabled ? _sHandleColor : _sHandleColorDisabled;
            var localToWorld = Matrix4x4.TRS(navSurface.transform.position, navSurface.transform.rotation, Vector3.one);
            using (new Handles.DrawingScope(color, localToWorld))
            {
                _mBoundsHandle.center = navSurface.center;
                _mBoundsHandle.size = navSurface.size;

                EditorGUI.BeginChangeCheck();
                _mBoundsHandle.DrawHandle();
                if (EditorGUI.EndChangeCheck())
                {
                    Undo.RecordObject(navSurface, "Modified NavMesh Surface");
                    Vector3 center = _mBoundsHandle.center;
                    Vector3 size = _mBoundsHandle.size;
                    navSurface.center = center;
                    navSurface.size = size;
                    EditorUtility.SetDirty(target);
                }
            }
        }

        [MenuItem("GameObject/AI/NavMesh Surface", false, 2000)]
        public static void CreateNavMeshSurface(MenuCommand menuCommand)
        {
            var parent = menuCommand.context as GameObject;
            var go = NavMeshComponentsGuiUtility.CreateAndSelectGameObject("NavMesh Surface", parent);
            go.AddComponent<NavMeshSurface>();
            var view = SceneView.lastActiveSceneView;
            if (view != null)
                view.MoveToView(go.transform);
        }
    }
}
