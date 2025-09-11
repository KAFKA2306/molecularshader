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
            string basePath = Application.dataPath.Replace("\\", "/");
            if (!basePath.StartsWith("/"))
            {
                // Ensure absolute path URI form: file:///...
                basePath = "/" + basePath;
            }
            downloader.urls = new string[]
            {
                "file://" + basePath + "/Molecule/TestData/ethanol.xyz",
                "file://" + basePath + "/Molecule/TestData/water.xyz", 
                "file://" + basePath + "/Molecule/TestData/benzene.xyz"
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
    
    public void OnMoleculeSelected(int index)
    {
        if (downloader != null && index >= 0 && index < downloader.keys.Length)
        {
            string selectedKey = downloader.keys[index];
            Debug.Log($"Selected molecule: {selectedKey}");
            downloader.selectedKey = selectedKey;
        }
    }
    
    public void OnLoadButtonClicked()
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
