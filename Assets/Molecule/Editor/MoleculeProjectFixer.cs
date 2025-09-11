using System.IO;
using System.Linq;
using UnityEditor;
using UnityEngine;

public static class MoleculeProjectFixer
{
    [MenuItem("Tools/Molecule/Verify Project Setup")] 
    public static void VerifySetup()
    {
        bool ok = true;

        // 1) VRChat packages present in manifest
        string manifestPath = Path.Combine(Application.dataPath, "../Packages/manifest.json");
        string manifestText = File.Exists(manifestPath) ? File.ReadAllText(manifestPath) : string.Empty;
        bool hasWorlds = manifestText.Contains("com.vrchat.worlds");
        bool hasBase = manifestText.Contains("com.vrchat.base");
        if (!hasWorlds || !hasBase)
        {
            Debug.LogError("[Molecule] manifest.json is missing VRChat packages. Add com.vrchat.worlds and com.vrchat.base.");
            ok = false;
        }

        // 2) Resources/TestMolecules/ethanol present
        string ethanolResPath = "Assets/Molecule/Resources/TestMolecules/ethanol.txt";
        if (!File.Exists(ethanolResPath))
        {
            Debug.LogError("[Molecule] Missing Resources/TestMolecules/ethanol.txt (required by EthanolMock and tests)");
            ok = false;
        }

        // 3) Compute shader asset exists
        string[] guids = AssetDatabase.FindAssets("t:ComputeShader MoleculeRaymarch");
        if (guids == null || guids.Length == 0)
        {
            Debug.LogError("[Molecule] MoleculeRaymarch.compute not found. Ensure Assets/Molecule/Shaders/MoleculeRaymarch.compute exists.");
            ok = false;
        }

        if (ok) Debug.Log("[Molecule] Verify Project Setup: OK");
    }

    [MenuItem("Tools/Molecule/Create Resources/TestMolecules")] 
    public static void CreateResourcesTestMolecules()
    {
        string src = Path.Combine(Application.dataPath, "Molecule/TestData/ethanol.xyz");
        string dstDir = Path.Combine(Application.dataPath, "Molecule/Resources/TestMolecules");
        string dst = Path.Combine(dstDir, "ethanol.txt");

        if (!Directory.Exists(dstDir)) Directory.CreateDirectory(dstDir);

        if (File.Exists(src))
        {
            File.WriteAllText(dst, File.ReadAllText(src));
            Debug.Log("[Molecule] Copied ethanol from TestData to Resources/TestMolecules/ethanol.txt");
        }
        else
        {
            // Minimal fallback content
            File.WriteAllText(dst, "9\nethanol\nC 0 0 0\nC 1.54 0 0\nO 2.09 1.2 0\nH -0.54 0.9 0\nH -0.54 -0.9 0\nH 2.05 -0.6 0.9\nH 2.05 -0.6 -0.9\nH 2.98 1.1 0\nH -0.3 0 1");
            Debug.LogWarning("[Molecule] TestData ethanol.xyz not found; created minimal ethanol.txt in Resources.");
        }
        AssetDatabase.Refresh();
    }

    [MenuItem("Tools/Molecule/Assign Driver References")] 
    public static void AssignDriverReferences()
    {
        // Find driver in scene
        var driver = Object.FindObjectOfType<MoleculeRaymarchDriver>();
        if (driver == null)
        {
            Debug.LogError("[Molecule] No MoleculeRaymarchDriver found in the scene.");
            return;
        }

        // Assign camera if missing
        if (driver.cameraTransform == null && Camera.main != null)
        {
            driver.cameraTransform = Camera.main.transform;
            EditorUtility.SetDirty(driver);
            Debug.Log("[Molecule] Assigned main camera to MoleculeRaymarchDriver.cameraTransform");
        }

        // Assign compute shader by asset search
        if (driver.computeShader == null)
        {
            string[] guids = AssetDatabase.FindAssets("t:ComputeShader MoleculeRaymarch");
            if (guids != null && guids.Length > 0)
            {
                string path = AssetDatabase.GUIDToAssetPath(guids[0]);
                var cs = AssetDatabase.LoadAssetAtPath<ComputeShader>(path);
                if (cs != null)
                {
                    driver.computeShader = cs;
                    EditorUtility.SetDirty(driver);
                    Debug.Log($"[Molecule] Assigned compute shader: {path}");
                }
            }
            else
            {
                Debug.LogWarning("[Molecule] Could not find MoleculeRaymarch.compute. Please assign manually.");
            }
        }

        Debug.Log("[Molecule] Assign Driver References complete.");
    }
}

