using UnityEngine;
using UnityEditor;
using UniGLTF.M17N;
using System.Collections.Generic;

namespace UniGLTF.MeshUtility
{
    public class MeshUtilityDialog : EditorWindow
    {
        const string TITLE = "Mesh Utility Window";

        enum MeshProcessDialogTabs
        {
            MeshSeparator,
            MeshIntegrator,
            BoneMeshEraser,
        }
        MeshProcessDialogTabs _tab;

        private GameObject _exportTarget;

        [SerializeField]
        public bool _separateByBlendShape = true;

        [SerializeField]
        public SkinnedMeshRenderer _skinnedMeshRenderer = null;

        [SerializeField]
        public List<BoneMeshEraser.EraseBone> _eraseBones;

        private BoneMeshEraserEditor _boneMeshEraserEditor;
        private Vector2 _scrollPos = new Vector2(0, 0);

        public static void OpenWindow()
        {
            var window =
                (MeshUtilityDialog)EditorWindow.GetWindow(typeof(MeshUtilityDialog));
            window.titleContent = new GUIContent(TITLE);
            window.Show();
        }

        private void OnEnable()
        {
            if (!_boneMeshEraserEditor)
            {
                _boneMeshEraserEditor = (BoneMeshEraserEditor)Editor.CreateEditor(this);
            }
        }

        private void OnGUI()
        {
            _scrollPos = EditorGUILayout.BeginScrollView(_scrollPos);
            EditorGUIUtility.labelWidth = 200;
            LanguageGetter.OnGuiSelectLang();
            _exportTarget = (GameObject)EditorGUILayout.ObjectField(MeshUtilityMessages.TARGET_OBJECT.Msg(), _exportTarget, typeof(GameObject), true);
            _tab = TabBar.OnGUI(_tab, "LargeButton", GUI.ToolbarButtonSize.Fixed);

            var processed = false;
            switch (_tab)
            {
                case MeshProcessDialogTabs.MeshSeparator:
                    {
                        EditorGUILayout.HelpBox(MeshUtilityMessages.MESH_SEPARATOR.Msg(), MessageType.Info);
                        if (TabMeshSeparator.TryExecutable(_exportTarget, out string msg))
                        {
                            processed = TabMeshSeparator.OnGUI(_exportTarget);
                        }
                        else
                        {
                            EditorGUILayout.HelpBox(msg, MessageType.Error);
                        }
                        break;
                    }

                case MeshProcessDialogTabs.MeshIntegrator:
                    {
                        EditorGUILayout.HelpBox(MeshUtilityMessages.MESH_INTEGRATOR.Msg(), MessageType.Info);
                        _separateByBlendShape = EditorGUILayout.Toggle(MeshUtilityMessages.MESH_SEPARATOR_BY_BLENDSHAPE.Msg(), _separateByBlendShape);
                        if (TabMeshIntegrator.TryExecutable(_exportTarget, out string msg))
                        {
                            if (GUILayout.Button("Process", GUILayout.MinWidth(100)))
                            {
                                processed = TabMeshIntegrator.Execute(_exportTarget, _separateByBlendShape);
                            }
                        }
                        else
                        {
                            EditorGUILayout.HelpBox(msg, MessageType.Error);
                        }
                        break;
                    }

                case MeshProcessDialogTabs.BoneMeshEraser:
                    {
                        EditorGUILayout.HelpBox(MeshUtilityMessages.BONE_MESH_ERASER.Msg(), MessageType.Info);
                        if (_boneMeshEraserEditor)
                        {
                            _boneMeshEraserEditor.OnInspectorGUI();
                        }
                        if (TabBoneMeshRemover.TryExecutable(_exportTarget, _skinnedMeshRenderer, out string msg))
                        {
                            processed = TabBoneMeshRemover.OnGUI(_exportTarget, _skinnedMeshRenderer, _eraseBones);
                        }
                        else
                        {
                            EditorGUILayout.HelpBox(msg, MessageType.Error);
                        }
                        break;
                    }
            }
            EditorGUILayout.EndScrollView();

            if (processed)
            {
                Close();
                GUIUtility.ExitGUI();
            }
        }
    }
}