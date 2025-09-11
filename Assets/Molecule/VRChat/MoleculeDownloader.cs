using UdonSharp;
using UnityEngine;
using VRC.SDKBase;
using VRC.Udon;
using VRC.SDK3.StringLoading;
using VRC.Udon.Common.Interfaces;

// Minimal network fetcher: downloads XYZ or MOL text and hands it to a renderer.
public class MoleculeDownloader : UdonSharpBehaviour
{
    [Header("Selection")]
    [TextArea(1, 2)] public string selectedKey = "ethanol";

    [Header("Last Result")] public string latestXYZ;

    [Header("Optional Renderer")]
    public MoleculeUdonRenderer renderer;

    [Header("Key→URL Mapping")]
    public string[] keys = new[] { "ethanol", "water", "benzene" };
    public string[] urls = new[] {
        "https://example.com/molecules/ethanol.xyz",
        "https://example.com/molecules/water.xyz",
        "https://example.com/molecules/benzene.xyz"
    };

    [Header("CCDC MOL Endpoint")]
    [Tooltip("Use {id} as a placeholder for CCDC ID")] public string ccdcUrlTemplate = "https://www.ccdc.cam.ac.uk/structures/Data/Mol?id={id}&databaseId=0";

    public void LoadSelected()
    {
        LoadByKey(selectedKey);
    }

    public void LoadByKey(string key)
    {
        selectedKey = key;
        var url = ResolveUrl(key);
        if (string.IsNullOrEmpty(url))
        {
            Debug.LogError("MoleculeDownloader: URL not found for key " + key);
            return;
        }
        LoadByUrl(url);
    }

    public void LoadByUrl(string url)
    {
        Debug.Log("MoleculeDownloader: fetching " + url);
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
        if (string.IsNullOrEmpty(ccdcId)) return;
        LoadByUrl(ccdcUrlTemplate.Replace("{id}", ccdcId));
    }

    public override void OnStringLoadSuccess(IVRCStringDownload result)
    {
        var text = result.Result;
        var xyz = TryNormalizeToXYZ(text);
        if (string.IsNullOrEmpty(xyz))
        {
            Debug.LogError("MoleculeDownloader: downloaded data is not XYZ or MOL(V2000)");
            return;
        }
        latestXYZ = xyz;
        if ((object)renderer != null) renderer.ApplyXYZ(latestXYZ);
    }

    public override void OnStringLoadError(IVRCStringDownload result)
    {
        Debug.LogError("MoleculeDownloader: download failed: " + result.Error);
    }

    string ResolveUrl(string key)
    {
        for (int i = 0; i < keys.Length; i++)
            if (keys[i] == key) return i < urls.Length ? urls[i] : null;
        return null;
    }

    string TryNormalizeToXYZ(string input)
    {
        if (string.IsNullOrEmpty(input)) return null;
        var norm = input.Replace("\r\n", "\n").Replace('\r', '\n');
        var lines = norm.Split('\n');
        if (lines.Length >= 3)
        {
            int n;
            if (int.TryParse(lines[0].Trim(), out n) && n > 0)
                return norm; // Looks like XYZ
        }
        // Try MOL(V2000)→XYZ
        return ConvertMolV2000ToXYZ(norm);
    }

    string ConvertMolV2000ToXYZ(string mol)
    {
        var lines = mol.Split('\n');
        if (lines.Length < 6) return null;

        int countsIndex = -1;
        for (int i = 0; i < 6 && i < lines.Length; i++)
        {
            var l = lines[i].Trim();
            if (l.Length == 0) continue;
            var tok = SplitWhitespace(l);
            int a, b;
            if (tok.Length >= 2 && int.TryParse(tok[0], out a) && int.TryParse(tok[1], out b))
            { countsIndex = i; break; }
        }
        if (countsIndex < 0) return null;

        var cparts = SplitWhitespace(lines[countsIndex].Trim());
        int natoms = 0; if (!int.TryParse(cparts[0], out natoms) || natoms <= 0) return null;

        int atomStart = countsIndex + 1;
        if (atomStart + natoms > lines.Length) return null;

        var header = "converted from MOL (V2000)";
        var xyz = natoms.ToString() + "\n" + header + "\n";
        for (int i = 0; i < natoms; i++)
        {
            var parts = SplitWhitespace(lines[atomStart + i]);
            if (parts.Length < 4) return null;
            float fx, fy, fz;
            if (!(float.TryParse(parts[0], System.Globalization.NumberStyles.Float, System.Globalization.CultureInfo.InvariantCulture, out fx) &&
                  float.TryParse(parts[1], System.Globalization.NumberStyles.Float, System.Globalization.CultureInfo.InvariantCulture, out fy) &&
                  float.TryParse(parts[2], System.Globalization.NumberStyles.Float, System.Globalization.CultureInfo.InvariantCulture, out fz)))
                return null;
            var sym = parts[3];
            if (sym.Length > 1) sym = char.ToUpperInvariant(sym[0]) + sym.Substring(1).ToLowerInvariant();
            else sym = sym.ToUpperInvariant();
            xyz += sym + " " + fx.ToString(System.Globalization.CultureInfo.InvariantCulture) + " " + fy.ToString(System.Globalization.CultureInfo.InvariantCulture) + " " + fz.ToString(System.Globalization.CultureInfo.InvariantCulture) + "\n";
        }
        return xyz;
    }

    string[] SplitWhitespace(string line)
    {
        return line == null ? new string[0] : line.Split(new char[] { ' ', '\t' }, System.StringSplitOptions.RemoveEmptyEntries);
    }

#if UNITY_EDITOR
    void LoadLocalFile(string fileUrl)
    {
        try
        {
            string path = new System.Uri(fileUrl).LocalPath;
            if (!System.IO.File.Exists(path))
            {
                Debug.LogError("Local file not found: " + path);
                return;
            }
            var xyz = System.IO.File.ReadAllText(path);
            latestXYZ = xyz;
            if ((object)renderer != null) renderer.ApplyXYZ(latestXYZ);
        }
        catch (System.Exception ex)
        {
            Debug.LogError("Failed to load local file: " + ex.Message);
        }
    }
#endif
}

