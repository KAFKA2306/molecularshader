#if UNITY_INCLUDE_TESTS
using System.Collections;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;

public class EthanolMockPlayModeTests
{
    [UnityTest]
    public IEnumerator LoadsAndParsesEthanolFromResources()
    {
        var go = new GameObject("MoleculeTest");
        var driver = go.AddComponent<MoleculeRaymarchDriver>();
        var mock = go.AddComponent<EthanolMock>();
        mock.driver = driver;

        // Load ethanol immediately; driver will parse on next Update
        mock.LoadEthanol();

        // Wait a frame to let Update() run and parse XYZ
        yield return null;

        Assert.Greater(driver.GetAtomCount(), 0, "Atom count should be parsed > 0");
        Assert.AreEqual(9, driver.GetAtomCount(), "Ethanol should have 9 atoms as per test data");
        Assert.Greater(driver.GetBondCount(), 0, "Bond count should be generated > 0");

        Object.Destroy(go);
    }
}
#endif
