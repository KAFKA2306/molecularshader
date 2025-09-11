using UdonSharp;
using UnityEngine;
using VRC.SDKBase;
using VRC.Udon;
using VRC.SDK3.StringLoading;
using VRC.Udon.Common.Interfaces;

public class MoleculeDownloader : UdonSharpBehaviour
{
    [TextArea(2,6)] public string selectedKey = "ethanol";
    // Store the most recent XYZ text so other systems can read it
    public string latestXYZ;

#if !COMPILER_UDONSHARP
    // Editor/Mono-only reference; not available in Udon at runtime
    public MoleculeRaymarchDriver driver;
#endif
    
    [Header("Molecule URL Mapping")]
    public string[] keys = new[] { "ethanol", "water", "benzene" };
    public string[] urls = new[] {
        "https://example.cdn/molecules/ethanol.xyz",
        "https://example.cdn/molecules/water.xyz",
        "https://example.cdn/molecules/benzene.xyz"
    };

    void Start()
    {
#if !COMPILER_UDONSHARP
        if (driver == null)
        {
            driver = GetComponent<MoleculeRaymarchDriver>();
            if (driver == null)
            {
                Debug.LogError("MoleculeRaymarchDriver not found!");
            }
        }
#endif
    }

    public void LoadByKey(string key)
    {
        selectedKey = key;
        var url = ResolveUrl(key);
        if (string.IsNullOrEmpty(url))
        {
            Debug.LogError($"No URL found for molecule: {key}");
            return;
        }

        Debug.Log($"Loading molecule: {key} from {url}");
        // Support local file loading in the Unity Editor; VRChat runtime will use VRCStringDownloader
        #if UNITY_EDITOR
        if (url.StartsWith("file://"))
        {
            LoadLocalFile(url);
            return;
        }
        #endif
        VRCStringDownloader.LoadUrl(new VRCUrl(url), (IUdonEventReceiver)this);
    }

    public override void OnStringLoadSuccess(IVRCStringDownload result)
    {
        string xyz = result.Result;
        latestXYZ = xyz;
        Debug.Log($"Successfully loaded XYZ data: {xyz.Length} characters");

#if !COMPILER_UDONSHARP
        if (driver != null)
        {
            driver.SetXYZText(xyz);
        }
        else
        {
            Debug.LogError("MoleculeRaymarchDriver is null!");
        }
#endif
    }

    public override void OnStringLoadError(IVRCStringDownload result)
    {
        Debug.LogError($"Download failed for {selectedKey}: {result.Error}");
    }

    string ResolveUrl(string key)
    {
        for (int i = 0; i < keys.Length; i++)
        {
            if (keys[i] == key)
            {
                if (i < urls.Length)
                    return urls[i];
                Debug.LogError($"URL mapping missing for key '{key}' at index {i}");
                return null;
            }
        }
        return null;
    }

    #if UNITY_EDITOR
    void LoadLocalFile(string fileUrl)
    {
        // fileUrl expected like: file:///absolute/path/to/file.xyz
        try
        {
            string path = new System.Uri(fileUrl).LocalPath;

            if (!System.IO.File.Exists(path))
            {
                Debug.LogError($"Local file not found: {path}");
                return;
            }

            string xyz = System.IO.File.ReadAllText(path);
            latestXYZ = xyz;
#if !COMPILER_UDONSHARP
            if (driver != null)
            {
                driver.SetXYZText(xyz);
            }
            else
            {
                Debug.LogError("MoleculeRaymarchDriver is null!");
            }
#endif
        }
        catch (System.Exception ex)
        {
            Debug.LogError($"Failed to load local file '{fileUrl}': {ex.Message}");
        }
    }
    #endif

    [ContextMenu("Load Selected Molecule")]
    public void LoadSelected()
    {
        LoadByKey(selectedKey);
    }
}
