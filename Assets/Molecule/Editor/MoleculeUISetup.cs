using UnityEngine;
using UnityEngine.UI;
using UnityEditor;
using TMPro;

public class MoleculeUISetup : EditorWindow
{
    [MenuItem("Tools/Molecule/Setup UI Canvas")]
    public static void SetupCanvas()
    {
        CreateMoleculeCanvas();
    }

    [MenuItem("Tools/Molecule/Setup Complete Scene")]
    public static void SetupCompleteScene()
    {
        // Create Canvas first
        GameObject canvasGO = CreateMoleculeCanvas();
        
        // Create Molecule System GameObject
        GameObject moleculeSystemGO = new GameObject("MoleculeSystem");
        
        // Add required components
        MoleculeDownloader downloader = moleculeSystemGO.AddComponent<MoleculeDownloader>();
        MoleculeRaymarchDriver driver = moleculeSystemGO.AddComponent<MoleculeRaymarchDriver>();
        MoleculeUI ui = moleculeSystemGO.AddComponent<MoleculeUI>();
        
        // Connect components
        downloader.driver = driver;
        ui.downloader = downloader;
        ui.driver = driver;
        
        // Connect UI elements
        ui.moleculeDropdown = canvasGO.transform.Find("MoleculeDropdown").GetComponent<TMP_Dropdown>();
        ui.displayImage = canvasGO.transform.Find("MoleculeDisplay").GetComponent<RawImage>();
        ui.loadButton = canvasGO.transform.Find("LoadButton").GetComponent<Button>();
        
        // Setup camera reference (try to find main camera)
        Camera mainCamera = Camera.main;
        if (mainCamera != null)
        {
            driver.cameraTransform = mainCamera.transform;
        }
        
        // Select the created system
        Selection.activeGameObject = moleculeSystemGO;
        
        Debug.Log("Molecule UI setup complete! Configure the compute shader and camera settings.");
    }

    static GameObject CreateMoleculeCanvas()
    {
        // Create Canvas
        GameObject canvasGO = new GameObject("MoleculeCanvas");
        Canvas canvas = canvasGO.AddComponent<Canvas>();
        canvas.renderMode = RenderMode.ScreenSpaceOverlay;
        canvas.sortingOrder = 100;
        
        CanvasScaler scaler = canvasGO.AddComponent<CanvasScaler>();
        scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
        scaler.referenceResolution = new Vector2(1920, 1080);
        
        canvasGO.AddComponent<GraphicRaycaster>();
        
        // Create Panel for organization
        GameObject panelGO = new GameObject("Panel");
        panelGO.transform.SetParent(canvasGO.transform, false);
        Image panel = panelGO.AddComponent<Image>();
        panel.color = new Color(0, 0, 0, 0.5f);
        
        RectTransform panelRect = panelGO.GetComponent<RectTransform>();
        panelRect.anchorMin = new Vector2(0, 0);
        panelRect.anchorMax = new Vector2(1, 1);
        panelRect.offsetMin = Vector2.zero;
        panelRect.offsetMax = Vector2.zero;
        
        // Create Dropdown
        GameObject dropdownGO = new GameObject("MoleculeDropdown");
        dropdownGO.transform.SetParent(panelGO.transform, false);
        
        TMP_Dropdown dropdown = dropdownGO.AddComponent<TMP_Dropdown>();
        dropdown.options.Clear();
        dropdown.options.Add(new TMP_Dropdown.OptionData("ethanol"));
        dropdown.options.Add(new TMP_Dropdown.OptionData("water"));
        dropdown.options.Add(new TMP_Dropdown.OptionData("benzene"));
        dropdown.value = 0;
        
        // Setup dropdown rect
        RectTransform dropdownRect = dropdownGO.GetComponent<RectTransform>();
        dropdownRect.anchorMin = new Vector2(0.1f, 0.8f);
        dropdownRect.anchorMax = new Vector2(0.4f, 0.9f);
        dropdownRect.offsetMin = Vector2.zero;
        dropdownRect.offsetMax = Vector2.zero;
        
        // Add dropdown components
        Image dropdownImage = dropdownGO.AddComponent<Image>();
        dropdownImage.color = Color.white;
        
        // Create dropdown template (simplified)
        SetupDropdownTemplate(dropdown);
        
        // Create RawImage for display
        GameObject displayGO = new GameObject("MoleculeDisplay");
        displayGO.transform.SetParent(panelGO.transform, false);
        
        RawImage rawImage = displayGO.AddComponent<RawImage>();
        rawImage.color = Color.white;
        
        RectTransform displayRect = displayGO.GetComponent<RectTransform>();
        displayRect.anchorMin = new Vector2(0.5f, 0.1f);
        displayRect.anchorMax = new Vector2(0.9f, 0.7f);
        displayRect.offsetMin = Vector2.zero;
        displayRect.offsetMax = Vector2.zero;
        
        // Create Load Button
        GameObject buttonGO = new GameObject("LoadButton");
        buttonGO.transform.SetParent(panelGO.transform, false);
        
        Button button = buttonGO.AddComponent<Button>();
        Image buttonImage = buttonGO.AddComponent<Image>();
        buttonImage.color = new Color(0.2f, 0.8f, 0.2f, 1f);
        
        RectTransform buttonRect = buttonGO.GetComponent<RectTransform>();
        buttonRect.anchorMin = new Vector2(0.1f, 0.6f);
        buttonRect.anchorMax = new Vector2(0.4f, 0.7f);
        buttonRect.offsetMin = Vector2.zero;
        buttonRect.offsetMax = Vector2.zero;
        
        // Add button text
        GameObject buttonTextGO = new GameObject("Text (TMP)");
        buttonTextGO.transform.SetParent(buttonGO.transform, false);
        
        TextMeshProUGUI buttonText = buttonTextGO.AddComponent<TextMeshProUGUI>();
        buttonText.text = "Load Molecule";
        buttonText.fontSize = 18;
        buttonText.color = Color.white;
        buttonText.alignment = TextAlignmentOptions.Center;
        
        RectTransform buttonTextRect = buttonTextGO.GetComponent<RectTransform>();
        buttonTextRect.anchorMin = Vector2.zero;
        buttonTextRect.anchorMax = Vector2.one;
        buttonTextRect.offsetMin = Vector2.zero;
        buttonTextRect.offsetMax = Vector2.zero;
        
        // Create EventSystem if it doesn't exist
        if (FindObjectOfType<UnityEngine.EventSystems.EventSystem>() == null)
        {
            GameObject eventSystemGO = new GameObject("EventSystem");
            eventSystemGO.AddComponent<UnityEngine.EventSystems.EventSystem>();
            eventSystemGO.AddComponent<UnityEngine.EventSystems.StandaloneInputModule>();
        }
        
        Debug.Log("Molecule Canvas created successfully!");
        return canvasGO;
    }
    
    static void SetupDropdownTemplate(TMP_Dropdown dropdown)
    {
        // Create Template
        GameObject templateGO = new GameObject("Template");
        templateGO.transform.SetParent(dropdown.transform, false);
        
        RectTransform templateRect = templateGO.AddComponent<RectTransform>();
        templateRect.anchorMin = new Vector2(0, 0);
        templateRect.anchorMax = new Vector2(1, 0);
        templateRect.pivot = new Vector2(0.5f, 1);
        templateRect.anchoredPosition = new Vector2(0, 2);
        templateRect.sizeDelta = new Vector2(0, 150);
        
        Image templateImage = templateGO.AddComponent<Image>();
        templateImage.color = Color.white;
        
        // Create Viewport
        GameObject viewportGO = new GameObject("Viewport");
        viewportGO.transform.SetParent(templateGO.transform, false);
        
        RectTransform viewportRect = viewportGO.AddComponent<RectTransform>();
        viewportRect.anchorMin = Vector2.zero;
        viewportRect.anchorMax = Vector2.one;
        viewportRect.sizeDelta = Vector2.zero;
        viewportRect.anchoredPosition = Vector2.zero;
        
        viewportGO.AddComponent<Image>();
        viewportGO.AddComponent<Mask>().showMaskGraphic = false;
        
        // Create Content
        GameObject contentGO = new GameObject("Content");
        contentGO.transform.SetParent(viewportGO.transform, false);
        
        RectTransform contentRect = contentGO.AddComponent<RectTransform>();
        contentRect.anchorMin = Vector2.zero;
        contentRect.anchorMax = Vector2.one;
        contentRect.sizeDelta = Vector2.zero;
        contentRect.anchoredPosition = Vector2.zero;
        
        // Create Item
        GameObject itemGO = new GameObject("Item");
        itemGO.transform.SetParent(contentGO.transform, false);
        
        RectTransform itemRect = itemGO.AddComponent<RectTransform>();
        itemRect.anchorMin = Vector2.zero;
        itemRect.anchorMax = new Vector2(1, 1);
        itemRect.sizeDelta = Vector2.zero;
        itemRect.anchoredPosition = Vector2.zero;
        
        Toggle itemToggle = itemGO.AddComponent<Toggle>();
        itemGO.AddComponent<Image>();
        
        // Create Item Label
        GameObject labelGO = new GameObject("Item Label");
        labelGO.transform.SetParent(itemGO.transform, false);
        
        TextMeshProUGUI label = labelGO.AddComponent<TextMeshProUGUI>();
        label.text = "Option A";
        label.fontSize = 14;
        label.color = Color.black;
        
        RectTransform labelRect = labelGO.GetComponent<RectTransform>();
        labelRect.anchorMin = Vector2.zero;
        labelRect.anchorMax = Vector2.one;
        labelRect.offsetMin = new Vector2(10, 1);
        labelRect.offsetMax = new Vector2(-10, -2);
        
        // Configure dropdown
        dropdown.template = templateRect;
        dropdown.captionText = label;
        dropdown.itemText = label;
        
        templateGO.SetActive(false);
    }
}