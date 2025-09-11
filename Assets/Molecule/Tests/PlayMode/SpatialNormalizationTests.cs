#if UNITY_INCLUDE_TESTS
using System.Collections;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;

public class SpatialNormalizationTests
{
    [UnityTest]
    public IEnumerator CentersAndFitsToTargetSize()
    {
        var go = new GameObject("SpatialTest");
        var driver = go.AddComponent<MoleculeRaymarchDriver>();

        // Assign spatial config that centers and fits to 1.0m box
        var cfg = ScriptableObject.CreateInstance<MoleculeSpatialConfig>();
        cfg.centerAtOrigin = true;
        cfg.autoFit = true;
        cfg.targetBoundsSize = 1.0f;
        cfg.atomScale = 1.0f;
        cfg.bondScale = 1.0f;
        driver.spatial = cfg;

        // Load ethanol via helper
        var mock = go.AddComponent<EthanolMock>();
        mock.driver = driver;
        mock.LoadEthanol();

        // Allow driver.Update to parse and compute bounds
        yield return null;

        Assert.Greater(driver.GetAtomCount(), 0, "Atoms should be parsed");
        var center = driver.GetBoundsCenter();
        var size = driver.GetBoundsSize();

        // Center should be near (0,0,0)
        Assert.Less(Vector3.Magnitude(center), 1e-3f, "Bounds should be centered near origin");

        // Longest side ~ targetBoundsSize within tolerance
        float longest = Mathf.Max(size.x, Mathf.Max(size.y, size.z));
        Assert.That(longest, Is.InRange(0.98f, 1.02f), "Longest bounds side should be ~1.0m");

        Object.Destroy(go);
    }
}
#endif

