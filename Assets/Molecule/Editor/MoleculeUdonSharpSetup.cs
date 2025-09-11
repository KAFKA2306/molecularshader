using UnityEngine;
using UnityEditor;
#if UDONSHARP
using UdonSharp;
#endif
#if UNITY_EDITOR
#if UDONSHARP_EDITOR
using UdonSharpEditor;
#endif
using System.Linq;
#endif

// Editor tools to validate UdonSharp setup and wire scene components
public static class MoleculeUdonSharpSetup
{
    [MenuItem("Tools/Molecule/VRChat/Validate UdonSharp Setup")] 
    public static void ValidateUdonSharpSetup()
    {
#if !UNITY_EDITOR
        Debug.LogWarning("ValidateUdonSharpSetup can only run in the Unity Editor.");
        return;
#else
        // 1) Find or create the MoleculeVRChatSystem object
        var system = GameObject.Find("MoleculeVRChatSystem");
        if (system == null)
        {
            system = new GameObject("MoleculeVRChatSystem");
            Undo.RegisterCreatedObjectUndo(system, "Create MoleculeVRChatSystem");
        }

        // 2) Ensure core components exist
        var driver = system.GetComponent<MoleculeRaymarchDriver>() ?? Undo.AddComponent<MoleculeRaymarchDriver>(system);
        var ui = system.GetComponent<MoleculeUI>() ?? Undo.AddComponent<MoleculeUI>(system);

#if UDONSHARP
        // 3) Ensure UdonSharp MoleculeDownloader exists
        var downloader = system.GetComponent<MoleculeDownloader>();
        if (downloader == null)
        {
            downloader = Undo.AddComponent<MoleculeDownloader>(system);
        }
#endif

        // 4) Wire UI references
        ui.driver = driver;
#if UDONSHARP
        ui.downloader = downloader;
#endif

        // 5) Assign compute shader if found in project
        var guids = AssetDatabase.FindAssets("t:ComputeShader MoleculeRaymarch");
        if (guids != null && guids.Length > 0)
        {
            var csPath = AssetDatabase.GUIDToAssetPath(guids[0]);
            var cs = AssetDatabase.LoadAssetAtPath<ComputeShader>(csPath);
            if (cs != null) driver.computeShader = cs;
        }

        // 6) Link to camera
        if (Camera.main != null)
        {
            driver.cameraTransform = Camera.main.transform;
        }

#if UDONSHARP_EDITOR
        // 7) Compile all UdonSharp programs so program assets are created
        try
        {
            UdonSharpEditorUtility.CompileAllCsPrograms();
            Debug.Log("UdonSharp: Compiled all C# programs and generated program assets.");
        }
        catch (System.Exception ex)
        {
            Debug.LogError($"UdonSharp compile error: {ex.Message}\nCheck for any missing program asset or script errors.");
        }
#else
        Debug.Log("UdonSharpEditor not available in current compile symbols. Open in Unity Editor with UdonSharp installed to compile.");
#endif

        Selection.activeObject = system;
        Debug.Log("Molecule UdonSharp validation complete. If errors persist, open UdonSharp Compiler window and recompile.");
#endif
    }
}

