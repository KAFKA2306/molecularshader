using System.Collections.Generic;
using System.IO;
using UnityEditor;
using UnityEngine;
using UnityEngine.UI;

namespace Molecule.Editor
{
    public static class MoleculeMenu
    {
        private const string ComputePath = "Assets/Molecule/Resources/Shaders/MoleculeRaymarch.compute";
        private static readonly string[] SampleMolecules =
        {
            "Assets/Molecule/Resources/TestMolecules/ethanol.xyz",
            "Assets/Molecule/Resources/TestMolecules/water.xyz",
            "Assets/Molecule/Resources/TestMolecules/benzene.xyz",
        };
        private const string DefaultScene = "Assets/Scenes/VRCDefaultWorldScene.unity";

        [MenuItem("Tools/Molecule/Validate Project Health", priority = 0)]
        public static void ValidateProjectHealth()
        {
            var messages = new List<string>();
            var ok = true;

            // Compute shader exists
            if (File.Exists(ComputePath)) messages.Add("✔ Compute shader found: " + ComputePath);
            else { messages.Add("✘ Missing compute shader at: " + ComputePath); ok = false; }

            // Samples
            foreach (var p in SampleMolecules)
            {
                if (File.Exists(p)) messages.Add("✔ Sample found: " + Path.GetFileName(p));
                else { messages.Add("✘ Missing sample: " + p); ok = false; }
            }

            // Main scene exists
            if (File.Exists(DefaultScene)) messages.Add("✔ Main scene present: " + DefaultScene);
            else { messages.Add("✘ Missing main scene: " + DefaultScene); ok = false; }

            // Script types compile
            if (typeof(MoleculeRaymarchDriver) != null) messages.Add("✔ MoleculeRaymarchDriver type loaded");
            if (typeof(MoleculeRenderPreview) != null) messages.Add("✔ MoleculeRenderPreview type loaded");

            var summary = string.Join("\n", messages);
            if (ok) Debug.Log("Molecule Validate Project Health:\n" + summary);
            else Debug.LogWarning("Molecule Validate Project Health (issues found):\n" + summary);

            EditorUtility.DisplayDialog(
                ok ? "Molecule: Project Healthy" : "Molecule: Issues Detected",
                summary,
                "OK");
        }

        [MenuItem("Tools/Molecule/Create Demo Setup", priority = 1)]
        public static void CreateDemoSetup()
        {
            // Ensure a camera exists
            var cam = Camera.main;
            if (cam == null)
            {
                var camGo = new GameObject("Main Camera");
                cam = camGo.AddComponent<Camera>();
                cam.tag = "MainCamera";
                cam.transform.position = new Vector3(0, 0, -3);
                cam.transform.rotation = Quaternion.identity;
            }

            // Create UI canvas for preview
            var canvasGo = new GameObject("MoleculeCanvas");
            var canvas = canvasGo.AddComponent<Canvas>();
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;
            canvasGo.AddComponent<CanvasScaler>();
            canvasGo.AddComponent<GraphicRaycaster>();

            var rawGo = new GameObject("MoleculePreview");
            rawGo.transform.SetParent(canvas.transform, false);
            var ri = rawGo.AddComponent<RawImage>();
            var rt = (RectTransform)ri.transform;
            rt.anchorMin = new Vector2(0.5f, 0.5f);
            rt.anchorMax = new Vector2(0.5f, 0.5f);
            rt.sizeDelta = new Vector2(512, 512);
            rt.anchoredPosition = Vector2.zero;

            // Create driver
            var root = new GameObject("MoleculeDemo");
            var driver = root.AddComponent<MoleculeRaymarchDriver>();
            driver.cameraTransform = cam.transform;
            if (driver.computeShader == null)
            {
                var cs = Resources.Load<ComputeShader>("Shaders/MoleculeRaymarch");
                if (cs != null) driver.computeShader = cs;
            }

            // Ethanol loader for quick start
            var mock = root.AddComponent<EthanolMock>();
            mock.driver = driver;
            mock.loadOnStart = true;

            // Preview binder
            var binder = root.AddComponent<MoleculeRenderPreview>();
            binder.driver = driver;
            binder.rawImageTarget = ri;

            Selection.activeGameObject = root;
            EditorGUIUtility.PingObject(root);
            Debug.Log("Molecule demo setup created. Press Play to see ethanol preview.");
        }

        [MenuItem("Assets/Create/Molecule/Spatial Config", priority = 2000)]
        public static void CreateSpatialConfig()
        {
            var asset = ScriptableObject.CreateInstance<MoleculeSpatialConfig>();
            var path = EditorUtility.SaveFilePanelInProject(
                "Create Molecule Spatial Config",
                "MoleculeSpatialConfig",
                "asset",
                "Choose a location for the Spatial Config asset.");
            if (!string.IsNullOrEmpty(path))
            {
                AssetDatabase.CreateAsset(asset, path);
                AssetDatabase.SaveAssets();
                Selection.activeObject = asset;
            }
        }
    }
}

