using UdonSharp;
using UnityEngine;
using UnityEngine.UI;

// Minimal 2D UI renderer: projects atoms into XY and draws small dots under a RectTransform.
public class MoleculeUdonRenderer : UdonSharpBehaviour
{
    [Header("UI Target")]
    public RectTransform atomContainer;
    public Color atomColor = Color.white;
    public float dotSize = 6f;
    public int maxDots = 256;

    private Image[] pool;
    private int poolSize;

    void Start()
    {
        EnsurePool();
    }

    public void ApplyXYZ(string xyz)
    {
        if (string.IsNullOrEmpty(xyz)) return;
        EnsurePool();

        string[] lines = xyz.Replace("\r\n", "\n").Replace('\r', '\n').Split('\n');
        int n = 0;
        if (lines.Length < 3 || !int.TryParse(lines[0].Trim(), out n) || n <= 0)
            return;

        // Parse atom lines: "Sym x y z"
        int count = Mathf.Min(n, maxDots);
        float minX = 1e9f, minY = 1e9f, minZ = 1e9f;
        float maxX = -1e9f, maxY = -1e9f, maxZ = -1e9f;
        float[] xs = new float[count];
        float[] ys = new float[count];
        float[] zs = new float[count];

        for (int i = 0; i < count; i++)
        {
            int li = 2 + i;
            if (li >= lines.Length) { count = i; break; }
            var parts = SplitWhitespace(lines[li]);
            if (parts.Length < 4) { count = i; break; }
            float x, y, z;
            if (!float.TryParse(parts[1], System.Globalization.NumberStyles.Float, System.Globalization.CultureInfo.InvariantCulture, out x) ||
                !float.TryParse(parts[2], System.Globalization.NumberStyles.Float, System.Globalization.CultureInfo.InvariantCulture, out y) ||
                !float.TryParse(parts[3], System.Globalization.NumberStyles.Float, System.Globalization.CultureInfo.InvariantCulture, out z))
            { count = i; break; }
            xs[i] = x; ys[i] = y; zs[i] = z;
            if (x < minX) minX = x; if (y < minY) minY = y; if (z < minZ) minZ = z;
            if (x > maxX) maxX = x; if (y > maxY) maxY = y; if (z > maxZ) maxZ = z;
        }

        // Project to XY, normalize into [0,1]
        float spanX = Mathf.Max(1e-5f, maxX - minX);
        float spanY = Mathf.Max(1e-5f, maxY - minY);
        for (int i = 0; i < poolSize; i++) pool[i].enabled = false;

        for (int i = 0; i < count; i++)
        {
            var img = pool[i];
            img.enabled = true;
            img.color = atomColor;
            var rt = (RectTransform)img.transform;
            float u = (xs[i] - minX) / spanX;
            float v = (ys[i] - minY) / spanY;
            Vector2 anchor = new Vector2(u, v);
            rt.anchorMin = anchor; rt.anchorMax = anchor; rt.anchoredPosition = Vector2.zero;
            rt.sizeDelta = new Vector2(dotSize, dotSize);
        }
    }

    void EnsurePool()
    {
        if (atomContainer == null)
        {
            Debug.LogWarning("MoleculeUdonRenderer: atomContainer is not set");
            return;
        }
        if (pool != null && pool.Length == maxDots) return;
        // Clear existing children if pool changed
        for (int i = atomContainer.childCount - 1; i >= 0; i--)
            GameObject.Destroy(atomContainer.GetChild(i).gameObject);

        pool = new Image[Mathf.Max(1, maxDots)];
        poolSize = pool.Length;
        for (int i = 0; i < poolSize; i++)
        {
            var go = new GameObject("AtomDot_" + i);
            go.transform.SetParent(atomContainer, false);
            var img = go.AddComponent<Image>();
            img.enabled = false;
            pool[i] = img;
            var rt = (RectTransform)go.transform;
            rt.sizeDelta = new Vector2(dotSize, dotSize);
        }
    }

    string[] SplitWhitespace(string s)
    {
        if (s == null) return new string[0];
        return s.Split(new char[] { ' ', '\t' }, System.StringSplitOptions.RemoveEmptyEntries);
    }
}

