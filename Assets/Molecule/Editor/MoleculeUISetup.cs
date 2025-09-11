using UnityEngine;
using UnityEngine.UI;
using UnityEditor;
#if TMP_PRESENT
using TMPro;
#endif

public class MoleculeUISetup : EditorWindow
{
#if TMP_PRESENT
    [MenuItem("Tools/Molecule/Setup UI Canvas")]
    public static void SetupCanvas()
    {
        CreateMoleculeCanvas();
    }

    [MenuItem("Tools/Molecule/Setup Test Scene")]
    public static void SetupTestScene()
    {
        // Create Canvas first
        GameObject canvasGO = CreateMoleculeCanvas();
        
        // Create Molecule System GameObject
        GameObject moleculeSystemGO = new GameObject("MoleculeTestSystem");
        
        // Add components (no UdonSharp dependencies)
        MoleculeRaymarchDriver driver = moleculeSystemGO.AddComponent<MoleculeRaymarchDriver>();
        if (driver == null)
        {
            Debug.LogError("Failed to add MoleculeRaymarchDriver to MoleculeTestSystem");
            return;
        }

        MoleculeUI ui = moleculeSystemGO.AddComponent<MoleculeUI>();
        if (ui == null)
        {
            Debug.LogError("Failed to add MoleculeUI to MoleculeTestSystem (script compile issue?)");
            return;
        }

        // Connect components
        ui.driver = driver;

        // Connect UI elements
        Transform panelTransform = canvasGO != null ? canvasGO.transform.Find("Panel") : null;
        if (panelTransform != null)
        {
            ui.moleculeDropdown = panelTransform.Find("MoleculeDropdown")?.GetComponent<TMP_Dropdown>();
            ui.displayImage = panelTransform.Find("MoleculeDisplay")?.GetComponent<RawImage>();
            ui.loadButton = panelTransform.Find("LoadButton")?.GetComponent<Button>();
        }
        else
        {
            Debug.LogWarning("Panel not found under created MoleculeCanvas; UI references not wired");
        }

        // Setup camera reference (try to find main camera)
        Camera mainCamera = Camera.main;
        if (mainCamera != null)
        {
            driver.cameraTransform = mainCamera.transform;
        }
        else
        {
            Debug.LogWarning("Main Camera not found; assign cameraTransform on MoleculeRaymarchDriver manually.");
        }

        // Try auto-assign compute shader by name
        var guids = AssetDatabase.FindAssets("t:ComputeShader MoleculeRaymarch");
        if (guids != null && guids.Length > 0)
        {
            var path = AssetDatabase.GUIDToAssetPath(guids[0]);
            var cs = AssetDatabase.LoadAssetAtPath<ComputeShader>(path);
            if (cs != null)
            {
                driver.computeShader = cs;
            }
        }

        // Select the created system
        Selection.activeGameObject = moleculeSystemGO;

        Debug.Log("Molecule Test Scene setup complete! This version works without UdonSharp for testing.");
        Debug.Log("Configure the compute shader in the MoleculeRaymarchDriver component.");
    }

    [MenuItem("Tools/Molecule/Setup VRChat Scene")]
    public static void SetupVRChatScene()
    {
        // Create Canvas first
        GameObject canvasGO = CreateMoleculeCanvas();
        
        // Create Molecule System GameObject
        GameObject moleculeSystemGO = new GameObject("MoleculeVRChatSystem");
        
        // Add non-UdonSharp components first
        MoleculeRaymarchDriver driver = moleculeSystemGO.AddComponent<MoleculeRaymarchDriver>();
        if (driver == null)
        {
            Debug.LogError("Failed to add MoleculeRaymarchDriver to MoleculeVRChatSystem");
            return;
        }

        MoleculeUI ui = moleculeSystemGO.AddComponent<MoleculeUI>();
        if (ui == null)
        {
            Debug.LogError("Failed to add MoleculeUI to MoleculeVRChatSystem (script compile issue?)");
            return;
        }
        
        // Connect components (downloader will be added manually)
        ui.driver = driver;
        
        // Connect UI elements
        Transform panelTransform = canvasGO != null ? canvasGO.transform.Find("Panel") : null;
        if (panelTransform != null)
        {
            ui.moleculeDropdown = panelTransform.Find("MoleculeDropdown")?.GetComponent<TMP_Dropdown>();
            ui.displayImage = panelTransform.Find("MoleculeDisplay")?.GetComponent<RawImage>();
            ui.loadButton = panelTransform.Find("LoadButton")?.GetComponent<Button>();
        }
        else
        {
            Debug.LogWarning("Panel not found under created MoleculeCanvas; UI references not wired");
        }
        
        // Setup camera reference
        Camera mainCamera = Camera.main;
        if (mainCamera != null)
        {
            driver.cameraTransform = mainCamera.transform;
        }
        else
        {
            Debug.LogWarning("Main Camera not found; assign cameraTransform on MoleculeRaymarchDriver manually.");
        }
        
        // Try auto-assign compute shader by name
        var guids = AssetDatabase.FindAssets("t:ComputeShader MoleculeRaymarch");
        if (guids != null && guids.Length > 0)
        {
            var path = AssetDatabase.GUIDToAssetPath(guids[0]);
            var cs = AssetDatabase.LoadAssetAtPath<ComputeShader>(path);
            if (cs != null)
            {
                driver.computeShader = cs;
            }
        }
        
        // Select the created system
        Selection.activeGameObject = moleculeSystemGO;
        
        Debug.Log("VRChat Scene base setup complete!");
        Debug.Log("MANUAL STEPS REQUIRED:");
        Debug.Log("1. Add 'MoleculeDownloader' UdonSharp component to the MoleculeVRChatSystem GameObject");
        Debug.Log("2. Connect the downloader to the UI component");
        Debug.Log("3. Assign the compute shader");
        Debug.Log("4. Configure molecule URLs");
    }

    // Back-compat with earlier menu name reported by users
    [MenuItem("Tools/Molecule/Setup Complete Scene")]
    public static void SetupCompleteScene()
    {
        SetupVRChatScene();
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
        var scrollRect = templateGO.AddComponent<ScrollRect>();

        // Create Viewport
        GameObject viewportGO = new GameObject("Viewport");
        viewportGO.transform.SetParent(templateGO.transform, false);
        
        RectTransform viewportRect = viewportGO.AddComponent<RectTransform>();
        viewportRect.anchorMin = Vector2.zero;
        viewportRect.anchorMax = Vector2.one;
        viewportRect.sizeDelta = Vector2.zero;
        viewportRect.anchoredPosition = Vector2.zero;
        
        var viewportImage = viewportGO.AddComponent<Image>();
        viewportGO.AddComponent<Mask>().showMaskGraphic = false;
        scrollRect.viewport = viewportRect;

        // Create Content
        GameObject contentGO = new GameObject("Content");
        contentGO.transform.SetParent(viewportGO.transform, false);
        
        RectTransform contentRect = contentGO.AddComponent<RectTransform>();
        contentRect.anchorMin = Vector2.zero;
        contentRect.anchorMax = Vector2.one;
        contentRect.sizeDelta = Vector2.zero;
        contentRect.anchoredPosition = Vector2.zero;
        scrollRect.content = contentRect;
        
        // Create Item
        GameObject itemGO = new GameObject("Item");
        itemGO.transform.SetParent(contentGO.transform, false);
        
        RectTransform itemRect = itemGO.AddComponent<RectTransform>();
        itemRect.anchorMin = Vector2.zero;
        itemRect.anchorMax = new Vector2(1, 1);
        itemRect.sizeDelta = Vector2.zero;
        itemRect.anchoredPosition = Vector2.zero;
        
        Toggle itemToggle = itemGO.AddComponent<Toggle>();
        var itemBg = itemGO.AddComponent<Image>();
        itemToggle.targetGraphic = itemBg;
        
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
        dropdown.itemText = label;

        // Add caption text under dropdown root (separate from template)
        GameObject captionGO = new GameObject("Label");
        captionGO.transform.SetParent(dropdown.transform, false);
        var captionRect = captionGO.AddComponent<RectTransform>();
        captionRect.anchorMin = new Vector2(0, 0);
        captionRect.anchorMax = new Vector2(1, 1);
        captionRect.offsetMin = new Vector2(10, 1);
        captionRect.offsetMax = new Vector2(-10, -2);
        var captionText = captionGO.AddComponent<TextMeshProUGUI>();
        captionText.text = "Select Molecule";
        captionText.fontSize = 14;
        captionText.color = Color.black;
        dropdown.captionText = captionText;

        templateGO.SetActive(false);
    }
    
#else // TMP_PRESENT not defined

    // Fallback stubs when TextMeshPro is missing: keep menus discoverable and informative.
    [MenuItem("Tools/Molecule/Setup UI Canvas")]
    public static void SetupCanvasStub()
    {
        EditorUtility.DisplayDialog(
            "TextMeshPro Required",
            "This action requires the TextMeshPro package (com.unity.textmeshpro).\nPlease install it via Package Manager and try again.",
            "OK");
    }

    [MenuItem("Tools/Molecule/Setup Test Scene")]
    public static void SetupTestSceneStub()
    {
        SetupCanvasStub();
    }

    [MenuItem("Tools/Molecule/Setup VRChat Scene")]
    public static void SetupVRChatSceneStub()
    {
        SetupCanvasStub();
    }

    [MenuItem("Tools/Molecule/Setup Complete Scene")]
    public static void SetupCompleteSceneStub()
    {
        SetupCanvasStub();
    }
#endif // TMP_PRESENT
}
