using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class MoleculeTestDriver : MonoBehaviour
{
    [Header("Test Components")]
    public MoleculeRaymarchDriver driver;
    public TMP_Dropdown moleculeDropdown;
    public RawImage displayImage;
    public Button loadButton;
    
    [Header("Test Data")]
    public string[] availableMolecules = { "ethanol", "water", "benzene" };
    
    void Start()
    {
        SetupTestUI();
        LoadDefaultMolecule();
    }
    
    void SetupTestUI()
    {
        // Setup dropdown
        if (moleculeDropdown != null)
        {
            moleculeDropdown.options.Clear();
            foreach (string molecule in availableMolecules)
            {
                moleculeDropdown.options.Add(new TMP_Dropdown.OptionData(molecule));
            }
            moleculeDropdown.value = 0;
            moleculeDropdown.onValueChanged.AddListener(OnMoleculeSelected);
        }
        
        // Setup load button
        if (loadButton != null)
        {
            loadButton.onClick.AddListener(OnLoadButtonClicked);
        }
        
        // Connect display image
        if (displayImage != null && driver != null)
        {
            UpdateDisplayImage();
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
        if (index >= 0 && index < availableMolecules.Length)
        {
            string selectedMolecule = availableMolecules[index];
            Debug.Log($"Selected molecule: {selectedMolecule}");
        }
    }
    
    public void OnLoadButtonClicked()
    {
        int selectedIndex = moleculeDropdown != null ? moleculeDropdown.value : 0;
        if (selectedIndex >= 0 && selectedIndex < availableMolecules.Length)
        {
            LoadMolecule(availableMolecules[selectedIndex]);
        }
    }
    
    public void LoadMolecule(string moleculeName)
    {
        if (driver != null)
        {
            string path = Application.dataPath + $"/Molecule/TestData/{moleculeName}.xyz";
            if (System.IO.File.Exists(path))
            {
                string xyzData = System.IO.File.ReadAllText(path);
                driver.SetXYZText(xyzData);
                Debug.Log($"Loaded test data for {moleculeName}: {xyzData.Length} characters");
            }
            else
            {
                Debug.LogError($"Test data file not found: {path}");
            }
        }
    }
    
    void LoadDefaultMolecule()
    {
        if (availableMolecules.Length > 0)
        {
            LoadMolecule(availableMolecules[0]);
        }
    }
    
    [ContextMenu("Load Ethanol")]
    public void LoadEthanol() => LoadMolecule("ethanol");
    
    [ContextMenu("Load Water")]
    public void LoadWater() => LoadMolecule("water");
    
    [ContextMenu("Load Benzene")]
    public void LoadBenzene() => LoadMolecule("benzene");
}