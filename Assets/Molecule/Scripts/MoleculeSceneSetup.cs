using UnityEngine;
using UnityEngine.UI;
using TMPro;

[System.Serializable]
public class MoleculeSystemComponents
{
    [Header("Core Components")]
    public MoleculeDownloader downloader;
    public MoleculeRaymarchDriver driver;
    public MoleculeUI ui;
    
    [Header("UI Elements")]
    public Canvas canvas;
    public TMP_Dropdown dropdown;
    public RawImage displayImage;
    public Button loadButton;
    
    [Header("Settings")]
    public Camera targetCamera;
    public ComputeShader moleculeShader;
}

public class MoleculeSceneSetup : MonoBehaviour
{
    [Header("Auto Setup")]
    public bool setupOnStart = true;
    public bool useLocalTestData = true;
    
    [Header("Components")]
    public MoleculeSystemComponents components;
    
    void Start()
    {
        if (setupOnStart)
        {
            SetupMoleculeSystem();
        }
    }
    
    [ContextMenu("Setup Molecule System")]
    public void SetupMoleculeSystem()
    {
        ValidateComponents();
        ConnectComponents();
        ConfigureSettings();
        
        Debug.Log("Molecule system setup complete!");
    }
    
    void ValidateComponents()
    {
        // Find or create main components
        if (components.downloader == null)
            components.downloader = GetComponent<MoleculeDownloader>();
            
        if (components.driver == null)
            components.driver = GetComponent<MoleculeRaymarchDriver>();
            
        if (components.ui == null)
            components.ui = GetComponent<MoleculeUI>();
            
        // Find UI elements if not assigned
        if (components.canvas == null)
            components.canvas = FindObjectOfType<Canvas>();
            
        if (components.dropdown == null)
            components.dropdown = FindObjectOfType<TMP_Dropdown>();
            
        if (components.displayImage == null)
            components.displayImage = FindObjectOfType<RawImage>();
            
        if (components.loadButton == null)
            components.loadButton = FindObjectOfType<Button>();
            
        // Find camera if not assigned
        if (components.targetCamera == null)
            components.targetCamera = Camera.main;
    }
    
    void ConnectComponents()
    {
        // Connect downloader and driver
        if (components.downloader != null && components.driver != null)
        {
            components.downloader.driver = components.driver;
        }
        
        // Connect UI system
        if (components.ui != null)
        {
            components.ui.downloader = components.downloader;
            components.ui.driver = components.driver;
            components.ui.moleculeDropdown = components.dropdown;
            components.ui.displayImage = components.displayImage;
            components.ui.loadButton = components.loadButton;
            components.ui.useLocalTestData = useLocalTestData;
        }
        
        // Connect camera to driver
        if (components.driver != null && components.targetCamera != null)
        {
            components.driver.cameraTransform = components.targetCamera.transform;
        }
        
        // Assign compute shader if available
        if (components.driver != null && components.moleculeShader != null)
        {
            components.driver.computeShader = components.moleculeShader;
        }
    }
    
    void ConfigureSettings()
    {
        // Configure driver settings
        if (components.driver != null)
        {
            components.driver.resolution = new Vector2Int(512, 512);
            components.driver.atomScale = 1.0f;
            components.driver.bondScale = 0.2f;
            components.driver.fieldOfView = 60f;
            components.driver.lightDirection = new Vector3(-1, -1, -1).normalized;
            components.driver.lightColor = Color.white;
            components.driver.ambientIntensity = 0.3f;
        }
        
        // Configure UI settings
        if (components.ui != null)
        {
            components.ui.useLocalTestData = useLocalTestData;
        }
        
        // Setup dropdown options
        if (components.dropdown != null)
        {
            components.dropdown.options.Clear();
            components.dropdown.options.Add(new TMP_Dropdown.OptionData("ethanol"));
            components.dropdown.options.Add(new TMP_Dropdown.OptionData("water"));
            components.dropdown.options.Add(new TMP_Dropdown.OptionData("benzene"));
            components.dropdown.value = 0;
        }
        
        // Configure load button
        if (components.loadButton != null && components.ui != null)
        {
            components.loadButton.onClick.RemoveAllListeners();
            components.loadButton.onClick.AddListener(components.ui.OnLoadButtonClicked);
        }
        
        // Configure dropdown callback
        if (components.dropdown != null && components.ui != null)
        {
            components.dropdown.onValueChanged.RemoveAllListeners();
            components.dropdown.onValueChanged.AddListener(components.ui.OnMoleculeSelected);
        }
    }
    
    [ContextMenu("Load Test Ethanol")]
    public void LoadTestEthanol()
    {
        if (components.ui != null)
        {
            components.ui.LoadLocalTestData("ethanol");
        }
    }
    
    [ContextMenu("Load Test Water")]
    public void LoadTestWater()
    {
        if (components.ui != null)
        {
            components.ui.LoadLocalTestData("water");
        }
    }
    
    [ContextMenu("Load Test Benzene")]
    public void LoadTestBenzene()
    {
        if (components.ui != null)
        {
            components.ui.LoadLocalTestData("benzene");
        }
    }
    
    void OnValidate()
    {
        if (Application.isPlaying && components.ui != null)
        {
            components.ui.useLocalTestData = useLocalTestData;
        }
    }
}