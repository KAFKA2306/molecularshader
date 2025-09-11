using UnityEngine;

// Simple helper to load ethanol mock XYZ data and feed it to the driver.
// Priority: Resources (TestMolecules/ethanol) -> Editor asset path fallback -> do nothing.
public class EthanolMock : MonoBehaviour
{
    public MoleculeRaymarchDriver driver;

    [Tooltip("Load ethanol mock on Start")] public bool loadOnStart = true;

    void Awake()
    {
        if (driver == null)
            driver = GetComponent<MoleculeRaymarchDriver>();
    }

    void Start()
    {
        if (!loadOnStart) return;
        LoadEthanol();
    }

    [ContextMenu("Load Ethanol Mock")]
    public void LoadEthanol()
    {
        if (driver == null)
        {
            Debug.LogWarning("EthanolMock: MoleculeRaymarchDriver not found on GameObject.");
            return;
        }

        // Try Resources first so it works in PlayMode tests without file IO
        var ta = Resources.Load<TextAsset>("TestMolecules/ethanol");
        if (ta != null && !string.IsNullOrEmpty(ta.text))
        {
            driver.SetXYZText(ta.text);
            Debug.Log("EthanolMock: Loaded ethanol from Resources.");
            return;
        }

#if UNITY_EDITOR
        // Fallback in Editor: load directly from the TestData path
        string path = Application.dataPath + "/Molecule/TestData/ethanol.xyz";
        if (System.IO.File.Exists(path))
        {
            string xyz = System.IO.File.ReadAllText(path);
            if (!string.IsNullOrEmpty(xyz))
            {
                driver.SetXYZText(xyz);
                Debug.Log("EthanolMock: Loaded ethanol from Assets/Molecule/TestData.");
                return;
            }
        }
#endif

        Debug.LogWarning("EthanolMock: Could not find ethanol XYZ in Resources or TestData.");
    }
}

