#if UNITY_EDITOR
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace ProjectEclipse.EditorTools
{
    [InitializeOnLoad]
    public static class MvpSceneBootstrap
    {
        private const string MvpScenePath = "Assets/ProjectEclipse/Scenes/ProjectEclipse_MVP.unity";

        static MvpSceneBootstrap()
        {
            EditorApplication.delayCall += EnsureMvpSceneConfigured;
            EditorApplication.delayCall += CleanMissingScriptsInProjectEclipseScene;
        }

        [MenuItem("Project Eclipse/Open MVP Scene")]
        public static void OpenMvpScene()
        {
            if (EditorApplication.isPlayingOrWillChangePlaymode)
            {
                return;
            }

            if (AssetDatabase.LoadAssetAtPath<SceneAsset>(MvpScenePath) == null)
            {
                Debug.LogWarning("Project Eclipse MVP scene is missing at " + MvpScenePath + ".");
                return;
            }

            if (EditorSceneManager.SaveCurrentModifiedScenesIfUserWantsTo())
            {
                EditorSceneManager.OpenScene(MvpScenePath);
            }
        }

        [MenuItem("Project Eclipse/Configure MVP Play Scene")]
        public static void EnsureMvpSceneConfigured()
        {
            SceneAsset sceneAsset = AssetDatabase.LoadAssetAtPath<SceneAsset>(MvpScenePath);
            if (sceneAsset == null)
            {
                Debug.LogWarning("Project Eclipse MVP scene is missing at " + MvpScenePath + ".");
                return;
            }

            EnsureBuildSettingsScene();

            if (EditorSceneManager.playModeStartScene != sceneAsset)
            {
                EditorSceneManager.playModeStartScene = sceneAsset;
            }

            OpenMvpIfUnityRestoredDefaultScene();
        }

        private static void EnsureBuildSettingsScene()
        {
            EditorBuildSettingsScene[] scenes = EditorBuildSettings.scenes;
            for (int i = 0; i < scenes.Length; i++)
            {
                if (scenes[i].path == MvpScenePath)
                {
                    if (!scenes[i].enabled)
                    {
                        scenes[i].enabled = true;
                        EditorBuildSettings.scenes = scenes;
                    }

                    return;
                }
            }

            EditorBuildSettingsScene[] updatedScenes = new EditorBuildSettingsScene[scenes.Length + 1];
            for (int i = 0; i < scenes.Length; i++)
            {
                updatedScenes[i] = scenes[i];
            }

            updatedScenes[updatedScenes.Length - 1] = new EditorBuildSettingsScene(MvpScenePath, true);
            EditorBuildSettings.scenes = updatedScenes;
        }

        private static void OpenMvpIfUnityRestoredDefaultScene()
        {
            if (EditorApplication.isPlayingOrWillChangePlaymode)
            {
                return;
            }

            Scene activeScene = SceneManager.GetActiveScene();
            GameObject[] roots = activeScene.GetRootGameObjects();
            if (!LooksLikeUnityDefaultScene(activeScene) || !HasOnlyDefaultCameraAndLight(roots))
            {
                return;
            }

            EditorSceneManager.OpenScene(MvpScenePath);
        }

        private static void CleanMissingScriptsInProjectEclipseScene()
        {
            if (EditorApplication.isPlayingOrWillChangePlaymode)
            {
                return;
            }

            Scene activeScene = SceneManager.GetActiveScene();
            if (!activeScene.isLoaded)
            {
                return;
            }

            string scenePath = activeScene.path.Replace('\\', '/');
            bool isProjectEclipseScene = scenePath == MvpScenePath || scenePath.Contains("Temp/__Backupscenes/");
            if (!isProjectEclipseScene)
            {
                return;
            }

            GameObject[] roots = activeScene.GetRootGameObjects();
            int missingCount = 0;
            for (int i = 0; i < roots.Length; i++)
            {
                if (roots[i] != null)
                {
                    missingCount += GameObjectUtility.GetMonoBehavioursWithMissingScriptCount(roots[i]);
                }
            }

            if (missingCount <= 0)
            {
                return;
            }

            int removedCount = 0;
            for (int i = 0; i < roots.Length; i++)
            {
                GameObject root = roots[i];
                if (root == null)
                {
                    continue;
                }

                Undo.RegisterFullObjectHierarchyUndo(root, "Remove missing Project Eclipse scripts");
                removedCount += GameObjectUtility.RemoveMonoBehavioursWithMissingScript(root);
            }

            if (removedCount > 0)
            {
                EditorSceneManager.MarkSceneDirty(activeScene);
                Debug.LogWarning("Project Eclipse removed " + removedCount + " missing script component(s) from " + activeScene.name + ".");
            }
        }

        private static bool LooksLikeUnityDefaultScene(Scene scene)
        {
            if (string.IsNullOrEmpty(scene.path))
            {
                return scene.name == "Untitled";
            }

            string normalizedPath = scene.path.Replace('\\', '/');
            return normalizedPath.Contains("Temp/__Backupscenes/");
        }

        private static bool HasOnlyDefaultCameraAndLight(GameObject[] roots)
        {
            if (roots.Length == 0 || roots.Length > 2)
            {
                return false;
            }

            bool hasCamera = false;
            bool hasLight = false;
            for (int i = 0; i < roots.Length; i++)
            {
                if (roots[i] == null)
                {
                    continue;
                }

                if (roots[i].name == "Main Camera")
                {
                    hasCamera = true;
                    continue;
                }

                if (roots[i].name == "Directional Light")
                {
                    hasLight = true;
                    continue;
                }

                return false;
            }

            return hasCamera || hasLight;
        }
    }
}
#endif
