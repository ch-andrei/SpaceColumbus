using UnityEditor.IMGUI.Controls;
using UnityEditorInternal;
using UnityEngine.AI;
using UnityEngine;

namespace UnityEditor.AI
{
    [CanEditMultipleObjects]
    [CustomEditor(typeof(NavMeshModifierVolume))]
    class NavMeshModifierVolumeEditor : Editor
    {
        SerializedProperty _mAffectedAgents;
        SerializedProperty _mArea;
        SerializedProperty _mCenter;
        SerializedProperty _mSize;

        static Color _sHandleColor = new Color(187f, 138f, 240f, 210f) / 255;
        static Color _sHandleColorDisabled = new Color(187f * 0.75f, 138f * 0.75f, 240f * 0.75f, 100f) / 255;

        static int _sHandleControlIdHint = typeof(NavMeshModifierVolumeEditor).Name.GetHashCode();
        BoxBoundsHandle _mBoundsHandle = new BoxBoundsHandle(_sHandleControlIdHint);

        bool editingCollider
        {
            get { return EditMode.editMode == EditMode.SceneViewEditMode.Collider && EditMode.IsOwner(this); }
        }

        void OnEnable()
        {
            _mAffectedAgents = serializedObject.FindProperty("m_AffectedAgents");
            _mArea = serializedObject.FindProperty("m_Area");
            _mCenter = serializedObject.FindProperty("m_Center");
            _mSize = serializedObject.FindProperty("m_Size");

            NavMeshVisualizationSettings.showNavigation++;
        }

        void OnDisable()
        {
            NavMeshVisualizationSettings.showNavigation--;
        }

        public override void OnInspectorGUI()
        {
            serializedObject.Update();

            InspectorEditButtonGui();

            EditorGUILayout.PropertyField(_mSize);
            EditorGUILayout.PropertyField(_mCenter);

            NavMeshComponentsGuiUtility.AreaPopup("Area Type", _mArea);
            NavMeshComponentsGuiUtility.AgentMaskPopup("Affected Agents", _mAffectedAgents);
            EditorGUILayout.Space();

            serializedObject.ApplyModifiedProperties();
        }

        [DrawGizmo(GizmoType.Selected | GizmoType.Active)]
        static void RenderBoxGizmo(NavMeshModifierVolume navModifier, GizmoType gizmoType)
        {
            var color = navModifier.enabled ? _sHandleColor : _sHandleColorDisabled;
            var colorTrans = new Color(color.r * 0.75f, color.g * 0.75f, color.b * 0.75f, color.a * 0.15f);

            var oldColor = Gizmos.color;
            var oldMatrix = Gizmos.matrix;

            Gizmos.matrix = navModifier.transform.localToWorldMatrix;

            Gizmos.color = colorTrans;
            Gizmos.DrawCube(navModifier.center, navModifier.size);

            Gizmos.color = color;
            Gizmos.DrawWireCube(navModifier.center, navModifier.size);

            Gizmos.matrix = oldMatrix;
            Gizmos.color = oldColor;

            Gizmos.DrawIcon(navModifier.transform.position, "NavMeshModifierVolume Icon", true);
        }

        [DrawGizmo(GizmoType.NotInSelectionHierarchy | GizmoType.Pickable)]
        static void RenderBoxGizmoNotSelected(NavMeshModifierVolume navModifier, GizmoType gizmoType)
        {
            if (NavMeshVisualizationSettings.showNavigation > 0)
            {
                var color = navModifier.enabled ? _sHandleColor : _sHandleColorDisabled;
                var oldColor = Gizmos.color;
                var oldMatrix = Gizmos.matrix;

                Gizmos.matrix = navModifier.transform.localToWorldMatrix;

                Gizmos.color = color;
                Gizmos.DrawWireCube(navModifier.center, navModifier.size);

                Gizmos.matrix = oldMatrix;
                Gizmos.color = oldColor;
            }

            Gizmos.DrawIcon(navModifier.transform.position, "NavMeshModifierVolume Icon", true);
        }

        void InspectorEditButtonGui()
        {
            var navModifier = (NavMeshModifierVolume)target;
            var bounds = new Bounds(navModifier.transform.position, navModifier.size);

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

            var vol = (NavMeshModifierVolume)target;
            var color = vol.enabled ? _sHandleColor : _sHandleColorDisabled;
            using (new Handles.DrawingScope(color, vol.transform.localToWorldMatrix))
            {
                _mBoundsHandle.center = vol.center;
                _mBoundsHandle.size = vol.size;

                EditorGUI.BeginChangeCheck();
                _mBoundsHandle.DrawHandle();
                if (EditorGUI.EndChangeCheck())
                {
                    Undo.RecordObject(vol, "Modified NavMesh Modifier Volume");
                    Vector3 center = _mBoundsHandle.center;
                    Vector3 size = _mBoundsHandle.size;
                    vol.center = center;
                    vol.size = size;
                    EditorUtility.SetDirty(target);
                }
            }
        }

        [MenuItem("GameObject/AI/NavMesh Modifier Volume", false, 2001)]
        public static void CreateNavMeshModifierVolume(MenuCommand menuCommand)
        {
            var parent = menuCommand.context as GameObject;
            var go = NavMeshComponentsGuiUtility.CreateAndSelectGameObject("NavMesh Modifier Volume", parent);
            go.AddComponent<NavMeshModifierVolume>();
            var view = SceneView.lastActiveSceneView;
            if (view != null)
                view.MoveToView(go.transform);
        }
    }
}
