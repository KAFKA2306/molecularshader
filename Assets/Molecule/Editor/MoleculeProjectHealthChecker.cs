using UnityEngine;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine.SceneManagement;

public class MoleculeProjectHealthChecker : EditorWindow
{
    [MenuItem("Tools/Molecule/Validate Project Health")]
    public static void ValidateProjectHealth()
    {
        Debug.Log("=== MOLECULE PROJECT HEALTH CHECK ===");
        
        bool allGood = true;
        
        // Check VRChat SDK
        if (!CheckVRChatSDK())
        {
            allGood = false;
        }
        
        // Check Resources structure
        if (!CheckResourcesStructure())
        {
            allGood = false;
        }
        
        // Check scene setup
        if (!CheckSceneSetup())
        {
            allGood = false;
        }
        
        // Check compute shader assignments
        if (!CheckComputeShaderAssignments())
        {
            allGood = false;
        }
        
        if (allGood)
        {
            Debug.Log("✅ PROJECT HEALTH: ALL CHECKS PASSED");
        }
        else
        {
            Debug.Log("❌ PROJECT HEALTH: ISSUES FOUND - see messages above");
        }
    }
    
    [MenuItem("Tools/Molecule/Add VRChat World Descriptor")]
    public static void AddVRChatWorldDescriptor()
    {
        // Find or create a VRChat World Descriptor in current scene
        var descriptor = FindObjectOfType<VRC.SDK3.Components.VRCSceneDescriptor>();
        if (descriptor != null)
        {
            Debug.Log("✅ VRChat World Descriptor already exists in scene");
            Selection.activeGameObject = descriptor.gameObject;
            return;
        }
        
        GameObject worldDescriptorGO = new GameObject("VRCWorld");
        descriptor = worldDescriptorGO.AddComponent<VRC.SDK3.Components.VRCSceneDescriptor>();
        
        // Set basic spawn point
        descriptor.spawns = new Transform[] { worldDescriptorGO.transform };
        worldDescriptorGO.transform.position = Vector3.zero;
        
        Selection.activeGameObject = worldDescriptorGO;
        EditorUtility.SetDirty(worldDescriptorGO);
        
        Debug.Log("✅ VRChat World Descriptor added to scene");
    }
    
    [MenuItem("Tools/Molecule/Auto-Assign Compute Shader to Drivers")]
    public static void AutoAssignComputeShaders()
    {
        var drivers = FindObjectsOfType<MoleculeRaymarchDriver>();
        if (drivers.Length == 0)
        {
            Debug.LogWarning("No MoleculeRaymarchDriver found in scene");
            return;
        }
        
        var guids = AssetDatabase.FindAssets("t:ComputeShader MoleculeRaymarch");
        if (guids == null || guids.Length == 0)
        {
            Debug.LogError("❌ MoleculeRaymarch compute shader not found in project");
            return;
        }
        
        var path = AssetDatabase.GUIDToAssetPath(guids[0]);
        var computeShader = AssetDatabase.LoadAssetAtPath<ComputeShader>(path);
        
        int assigned = 0;
        foreach (var driver in drivers)
        {
            if (driver.computeShader == null)
            {
                driver.computeShader = computeShader;
                EditorUtility.SetDirty(driver);
                assigned++;
            }
        }
        
        Debug.Log($"✅ Auto-assigned compute shader to {assigned} drivers");
    }
    
    static bool CheckVRChatSDK()
    {
        bool hasWorlds = System.IO.Directory.Exists("Packages/com.vrchat.worlds");
        bool hasBase = System.IO.Directory.Exists("Packages/com.vrchat.base");
        
        if (hasWorlds && hasBase)
        {
            Debug.Log("✅ VRChat SDK packages detected");
            return true;
        }
        else
        {
            Debug.LogError("❌ CRITICAL: VRChat SDK packages missing. Use VRChat Creator Companion to add them.");
            return false;
        }
    }
    
    static bool CheckResourcesStructure()
    {
        string ethanolPath = "Assets/Molecule/Resources/TestMolecules/ethanol.xyz";
        if (System.IO.File.Exists(ethanolPath))
        {
            Debug.Log("✅ Resources/TestMolecules structure exists");
            return true;
        }
        else
        {
            Debug.LogWarning("⚠️ Resources/TestMolecules/ethanol.xyz missing - creating...");
            
            // Auto-fix: create structure
            string resourcesDir = "Assets/Molecule/Resources/TestMolecules";
            if (!System.IO.Directory.Exists(resourcesDir))
            {
                System.IO.Directory.CreateDirectory(resourcesDir);
            }
            
            string sourceEthanol = "Assets/Molecule/TestData/ethanol.xyz";
            if (System.IO.File.Exists(sourceEthanol))
            {
                System.IO.File.Copy(sourceEthanol, ethanolPath, true);
                
                // Copy other test files too
                string[] testFiles = { "water.xyz", "benzene.xyz" };
                foreach (string file in testFiles)
                {
                    string src = $"Assets/Molecule/TestData/{file}";
                    string dst = $"Assets/Molecule/Resources/TestMolecules/{file}";
                    if (System.IO.File.Exists(src))
                    {
                        System.IO.File.Copy(src, dst, true);
                    }
                }
                
                AssetDatabase.Refresh();
                Debug.Log("✅ Auto-fixed: Created Resources structure and copied test data");
                return true;
            }
            else
            {
                Debug.LogError("❌ Source test data not found in TestData folder");
                return false;
            }
        }
    }
    
    static bool CheckSceneSetup()
    {
        var descriptor = FindObjectOfType<VRC.SDK3.Components.VRCSceneDescriptor>();
        if (descriptor == null)
        {
            Debug.LogWarning("⚠️ No VRChat World Descriptor in scene. Use 'Add VRChat World Descriptor' to fix.");
            return false;
        }
        else
        {
            Debug.Log("✅ VRChat World Descriptor found in scene");
            return true;
        }
    }
    
    static bool CheckComputeShaderAssignments()
    {
        var drivers = FindObjectsOfType<MoleculeRaymarchDriver>();
        if (drivers.Length == 0)
        {
            Debug.LogWarning("⚠️ No MoleculeRaymarchDriver found in current scene");
            return true; // Not an error if no drivers exist
        }
        
        int unassigned = 0;
        foreach (var driver in drivers)
        {
            if (driver.computeShader == null)
            {
                unassigned++;
            }
            if (driver.cameraTransform == null)
            {
                Debug.LogWarning($"⚠️ {driver.name}: cameraTransform not assigned");
            }
        }
        
        if (unassigned > 0)
        {
            Debug.LogWarning($"⚠️ {unassigned} MoleculeRaymarchDriver(s) missing compute shader assignment. Use 'Auto-Assign Compute Shader' to fix.");
            return false;
        }
        else
        {
            Debug.Log($"✅ All {drivers.Length} MoleculeRaymarchDriver(s) have compute shaders assigned");
            return true;
        }
    }
}