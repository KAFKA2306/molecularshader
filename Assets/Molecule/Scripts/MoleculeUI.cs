using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class MoleculeUI : MonoBehaviour
{
    [Header("UI Components")]
    public TMP_Dropdown moleculeDropdown;
    public RawImage displayImage;
    public Button loadButton;
    
    [Header("Molecule System")]
    public MoleculeDownloader downloader;
    public MoleculeRaymarchDriver driver;
    
    [Header("Test Mode")]
    public bool useLocalTestData = true;
    
    void Start()
    {
        SetupUI();
        SetupTestMode();
    }
    
    void SetupUI()
    {
        if (moleculeDropdown != null)
        {
            moleculeDropdown.onValueChanged.AddListener(OnMoleculeSelected);
        }
        
        if (loadButton != null)
        {
            loadButton.onClick.AddListener(OnLoadButtonClicked);
        }
        
        // Connect the display image to the render target
        if (displayImage != null && driver != null)
        {
            UpdateDisplayImage();
        }
    }
    
    void SetupTestMode()
    {
        if (useLocalTestData && downloader != null)
        {
            // Override URLs for local testing
            downloader.urls = new string[]
            {
                "file://" + Application.dataPath + "/Molecule/TestData/ethanol.xyz",
                "file://" + Application.dataPath + "/Molecule/TestData/water.xyz", 
                "file://" + Application.dataPath + "/Molecule/TestData/benzene.xyz"
            };
        }
    }
    
    void Update()
    {
        // Update display image if render target changes
        if (displayImage != null && driver != null && displayImage.texture != driver.renderTarget)
        {
            UpdateDisplayImage();
        }
    }
    
    void UpdateDisplayImage()
    {
        if (displayImage != null && driver != null && driver.renderTarget != null)
        {
            displayImage.texture = driver.renderTarget;
        }
    }
    
    void OnMoleculeSelected(int index)
    {
        if (downloader != null && index >= 0 && index < downloader.keys.Length)
        {
            string selectedKey = downloader.keys[index];
            Debug.Log($"Selected molecule: {selectedKey}");
            downloader.selectedKey = selectedKey;
        }
    }
    
    void OnLoadButtonClicked()
    {
        if (downloader != null)
        {
            downloader.LoadSelected();
        }
    }
    
    public void LoadMoleculeByName(string moleculeName)
    {
        if (downloader != null)
        {
            downloader.LoadByKey(moleculeName);
        }
    }
    
    // Test method for local XYZ data
    public void LoadLocalTestData(string moleculeName)
    {
        if (driver != null)
        {
            string path = Application.dataPath + $"/Molecule/TestData/{moleculeName}.xyz";
            if (System.IO.File.Exists(path))
            {
                string xyzData = System.IO.File.ReadAllText(path);
                driver.SetXYZText(xyzData);
                Debug.Log($"Loaded local test data for {moleculeName}");
            }
            else
            {
                Debug.LogError($"Test data file not found: {path}");
            }
        }
    }
}