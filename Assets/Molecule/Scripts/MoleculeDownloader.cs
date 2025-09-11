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

    [Header("Dynamic Sources")]
    [Tooltip("CCDC Mol endpoint URL template. Use {id} placeholder for the CCDC ID.")]
    public string ccdcUrlTemplate = "https://www.ccdc.cam.ac.uk/structures/Data/Mol?id={id}&databaseId=0";

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

        LoadByUrl(url);
    }

    public void LoadByUrl(string url)
    {
        Debug.Log($"Loading molecule from {url}");
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

    public void LoadCcdcById(string ccdcId)
    {
        if (string.IsNullOrEmpty(ccdcId))
        {
            Debug.LogError("CCDC ID is empty");
            return;
        }
        string url = ccdcUrlTemplate.Replace("{id}", ccdcId);
        LoadByUrl(url);
    }

    public override void OnStringLoadSuccess(IVRCStringDownload result)
    {
        string text = result.Result;
        // Determine format: XYZ or MOL. Try XYZ first
        string xyz = TryNormalizeToXYZ(text);
        if (string.IsNullOrEmpty(xyz))
        {
            Debug.LogError("Downloaded data is not recognized as XYZ or MOL(V2000) format");
            return;
        }

        latestXYZ = xyz;
        Debug.Log($"Loaded molecule text; normalized to XYZ: {xyz.Length} chars");

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

    string TryNormalizeToXYZ(string input)
    {
        if (string.IsNullOrEmpty(input)) return null;
        // Quick XYZ check: first line integer atom count
        string[] lines = input.Replace("\r\n", "\n").Replace('\r', '\n').Split('\n');
        if (lines.Length >= 3)
        {
            int atomCountCandidate = -1;
            bool isInt = int.TryParse(lines[0].Trim(), out atomCountCandidate);
            if (isInt && atomCountCandidate > 0)
            {
                // Looks like XYZ already
                return input;
            }
        }

        // Try MOL (V2000) conversion
        string xyz = ConvertMolV2000ToXYZ(input);
        if (!string.IsNullOrEmpty(xyz)) return xyz;
        return null;
    }

    string ConvertMolV2000ToXYZ(string mol)
    {
        if (string.IsNullOrEmpty(mol)) return null;
        string[] lines = mol.Replace("\r\n", "\n").Replace('\r', '\n').Split('\n');
        if (lines.Length < 6) return null;

        // V2000 heuristic: counts line is at index 3 (0-based) usually
        // But some MOLs may have fewer header lines. Search first line containing at least 2 integers
        int countsIndex = -1;
        for (int i = 0; i < 6 && i < lines.Length; i++)
        {
            // Find a line that has at least 2 ints: atom and bond counts
            string l = lines[i].Trim();
            if (l.Length == 0) continue;
            // Tokenize
            int a, b;
            string[] tok = SplitWhitespace(l);
            if (tok != null && tok.Length >= 2 && TryParseInt(tok[0], out a) && TryParseInt(tok[1], out b))
            {
                countsIndex = i;
                break;
            }
        }
        if (countsIndex < 0) return null;

        string[] cparts = SplitWhitespace(lines[countsIndex].Trim());
        if (cparts == null || cparts.Length < 2) return null;
        int natoms = 0;
        int nbonds = 0;
        if (!TryParseInt(cparts[0], out natoms) || natoms <= 0) return null;
        // bonds optional for us
        TryParseInt(cparts[1], out nbonds);

        int atomStart = countsIndex + 1;
        if (atomStart + natoms > lines.Length) return null;

        // Build XYZ
        string header = "converted from MOL (V2000)";
        string xyz = natoms.ToString() + "\n" + header + "\n";
        for (int i = 0; i < natoms; i++)
        {
            string aline = lines[atomStart + i];
            string[] parts = SplitWhitespace(aline);
            if (parts == null || parts.Length < 4) return null;
            // MOL atom line: x y z symbol ...
            string sx = parts[0];
            string sy = parts[1];
            string sz = parts[2];
            string sym = parts[3];
            // Normalize element symbol case
            if (sym.Length > 1) sym = char.ToUpperInvariant(sym[0]) + sym.Substring(1).ToLowerInvariant();
            else sym = sym.ToUpperInvariant();

            // Validate floats (use invariant); if parse fails, still pass through raw
            float fx, fy, fz;
            bool px = float.TryParse(sx, System.Globalization.NumberStyles.Float, System.Globalization.CultureInfo.InvariantCulture, out fx);
            bool py = float.TryParse(sy, System.Globalization.NumberStyles.Float, System.Globalization.CultureInfo.InvariantCulture, out fy);
            bool pz = float.TryParse(sz, System.Globalization.NumberStyles.Float, System.Globalization.CultureInfo.InvariantCulture, out fz);
            if (!(px && py && pz)) return null;

            xyz += sym + " " + fx.ToString(System.Globalization.CultureInfo.InvariantCulture) + " " + fy.ToString(System.Globalization.CultureInfo.InvariantCulture) + " " + fz.ToString(System.Globalization.CultureInfo.InvariantCulture) + "\n";
        }
        return xyz;
    }

    string[] SplitWhitespace(string line)
    {
        if (line == null) return null;
        return line.Split(new char[] { ' ', '\t' }, System.StringSplitOptions.RemoveEmptyEntries);
    }

    bool TryParseInt(string s, out int v)
    {
        return int.TryParse(s, out v);
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
