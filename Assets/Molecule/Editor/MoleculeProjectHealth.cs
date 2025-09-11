using System;
using System.Linq;
using UnityEditor;
using UnityEngine;

public static class MoleculeProjectHealth
{
    [MenuItem("Tools/Molecule/Validate Project Health")] 
    public static void Validate()
    {
        Debug.Log("--- Molecule Project Health Check ---");

        // 1) VRChat SDK / UdonSharp presence
        bool hasVRCWorlds = TypeExists("VRC.SDKBase.VRC_SceneDescriptor") || TypeExists("VRC.SDK3.Components.VRCSceneDescriptor");
        bool hasStringLoader = TypeExists("VRC.SDK3.StringLoading.VRCStringDownloader");
        bool hasUdonSharp = TypeExists("UdonSharp.UdonSharpBehaviour");

        if (!hasVRCWorlds || !hasUdonSharp)
        {
            Debug.LogError("CRITICAL: VRChat SDK Worlds and/or UdonSharp not found. Install via VRChat Creator Companion (VCC).");
        }
        else
        {
            Debug.Log("OK: VRChat SDK/UdonSharp types detected.");
        }
        if (!hasStringLoader)
        {
            Debug.LogError("CRITICAL: VRC String Loading API not found (VRCStringDownloader).");
        }

        // 2) Resources structure for test molecules
        string ethanolResPath = "Assets/Molecule/Resources/TestMolecules/ethanol.xyz";
        if (!System.IO.File.Exists(ethanolResPath))
        {
            Debug.LogError($"CRITICAL: Missing {ethanolResPath}. PlayMode tests or EthanolMock will fail.");
        }
        else
        {
            Debug.Log("OK: Resources/TestMolecules/ethanol.xyz present.");
        }

        // 3) Scene references: compute shader and camera
        var drivers = GameObject.FindObjectsOfType<MoleculeRaymarchDriver>();
        if (drivers == null || drivers.Length == 0)
        {
            Debug.LogWarning("MEDIUM: No MoleculeRaymarchDriver found in the open scene. Run Tools/Molecule/Setup Test Scene.");
        }
        else
        {
            string[] shaderGuids = AssetDatabase.FindAssets("t:ComputeShader MoleculeRaymarch");
            ComputeShader foundShader = null;
            if (shaderGuids.Length > 0)
            {
                string path = AssetDatabase.GUIDToAssetPath(shaderGuids[0]);
                foundShader = AssetDatabase.LoadAssetAtPath<ComputeShader>(path);
            }

            foreach (var d in drivers)
            {
                if (d.computeShader == null)
                {
                    if (foundShader != null)
                    {
                        Undo.RecordObject(d, "Assign compute shader");
                        d.computeShader = foundShader;
                        EditorUtility.SetDirty(d);
                        Debug.Log($"FIXED: Assigned compute shader '{foundShader.name}' to {d.name}.");
                    }
                    else
                    {
                        Debug.LogWarning($"MEDIUM: Compute shader not assigned on {d.name}. Create/assign 'MoleculeRaymarch.compute'.");
                    }
                }

                if (d.cameraTransform == null)
                {
                    var cam = Camera.main ?? GameObject.FindObjectsOfType<Camera>().FirstOrDefault();
                    if (cam != null)
                    {
                        Undo.RecordObject(d, "Assign cameraTransform");
                        d.cameraTransform = cam.transform;
                        EditorUtility.SetDirty(d);
                        Debug.Log($"FIXED: Assigned cameraTransform '{cam.name}' to {d.name}.");
                    }
                    else
                    {
                        Debug.LogWarning($"MEDIUM: No camera found to assign to {d.name}.");
                    }
                }
            }
        }

        Debug.Log("--- Health Check Complete ---");
    }

    [MenuItem("Tools/Molecule/Auto-Assign Compute Shader to Drivers")] 
    public static void AutoAssignCompute()
    {
        string[] shaderGuids = AssetDatabase.FindAssets("t:ComputeShader MoleculeRaymarch");
        if (shaderGuids.Length == 0)
        {
            Debug.LogError("No 'MoleculeRaymarch' compute shader found in project.");
            return;
        }
        var shader = AssetDatabase.LoadAssetAtPath<ComputeShader>(AssetDatabase.GUIDToAssetPath(shaderGuids[0]));
        var drivers = GameObject.FindObjectsOfType<MoleculeRaymarchDriver>();
        foreach (var d in drivers)
        {
            if (d.computeShader == null)
            {
                Undo.RecordObject(d, "Assign compute shader");
                d.computeShader = shader;
                EditorUtility.SetDirty(d);
                Debug.Log($"Assigned compute shader to {d.name}");
            }
        }
    }

    static bool TypeExists(string typeName)
    {
        return AppDomain.CurrentDomain.GetAssemblies()
            .SelectMany(a => {
                try { return a.GetTypes(); } catch { return Array.Empty<Type>(); }
            })
            .Any(t => t.FullName == typeName);
    }
}

