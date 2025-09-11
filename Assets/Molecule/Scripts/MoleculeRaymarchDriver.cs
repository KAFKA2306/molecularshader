using System.Collections.Generic;
using System.Globalization;
using UnityEngine;

[System.Serializable]
public struct AtomData
{
    public Vector3 position;
    public uint element;
    
    public AtomData(Vector3 pos, uint elem)
    {
        position = pos;
        element = elem;
    }
}

[System.Serializable]
public struct BondData
{
    public uint atomA;
    public uint atomB;
    public float radius;
    
    public BondData(uint a, uint b, float r)
    {
        atomA = a;
        atomB = b;
        radius = r;
    }
}

[ExecuteAlways]
public class MoleculeRaymarchDriver : MonoBehaviour
{
    [Header("Compute Settings")]
    public ComputeShader computeShader;
    public RenderTexture renderTarget;
    public Vector2Int resolution = new Vector2Int(1024, 1024);
    
    [Header("Camera Settings")]
    public Transform cameraTransform;
    public float fieldOfView = 60f;
    public float nearPlane = 0.1f;
    public float farPlane = 100f;
    
    [Header("Rendering Settings")]
    public float atomScale = 1.0f;
    public float bondScale = 0.2f;
    
    [Header("Lighting")]
    public Vector3 lightDirection = new Vector3(-1, -1, -1);
    public Color lightColor = Color.white;
    public float ambientIntensity = 0.3f;
    
    [Header("Bond Detection")]
    public float bondDistanceThreshold = 1.8f;
    
    // Element data (atomic number -> color/radius)
    private static readonly Dictionary<string, uint> ElementMap = new Dictionary<string, uint>
    {
        { "H", 0 }, { "C", 1 }, { "N", 2 }, { "O", 3 }, { "F", 4 }, { "P", 5 }, { "S", 6 }, { "Cl", 7 }
    };
    
    private static readonly Vector4[] ElementColors = new Vector4[]
    {
        new Vector4(1.0f, 1.0f, 1.0f, 1.0f),  // H - White
        new Vector4(0.2f, 0.2f, 0.2f, 1.0f),  // C - Dark Gray
        new Vector4(0.0f, 0.0f, 1.0f, 1.0f),  // N - Blue
        new Vector4(1.0f, 0.0f, 0.0f, 1.0f),  // O - Red
        new Vector4(0.0f, 1.0f, 1.0f, 1.0f),  // F - Cyan
        new Vector4(1.0f, 0.5f, 0.0f, 1.0f),  // P - Orange
        new Vector4(1.0f, 1.0f, 0.0f, 1.0f),  // S - Yellow
        new Vector4(0.0f, 1.0f, 0.0f, 1.0f)   // Cl - Green
    };
    
    private static readonly float[] ElementRadii = new float[]
    {
        0.31f,  // H
        0.70f,  // C
        0.65f,  // N
        0.60f,  // O
        0.50f,  // F
        1.00f,  // P
        1.00f,  // S
        0.99f   // Cl
    };

    private ComputeBuffer atomBuffer;
    private ComputeBuffer bondBuffer;
    private ComputeBuffer elementColorsBuffer;
    private ComputeBuffer elementRadiiBuffer;

    private List<AtomData> atoms = new List<AtomData>();
    private List<BondData> bonds = new List<BondData>();

    private string xyzText;
    private bool isDirty = false;
    
    void Start()
    {
        if (cameraTransform == null)
            cameraTransform = Camera.main?.transform;
            
        SetupElementBuffers();
        SetupRenderTarget();
    }

    void Update()
    {
        if (isDirty)
        {
            ParseXYZAndBuildBuffers();
            isDirty = false;
        }

        if (computeShader != null && renderTarget != null)
        {
            RenderMolecule();
        }
    }

    public void SetXYZText(string xyz)
    {
        xyzText = xyz;
        isDirty = true;
    }

    void SetupElementBuffers()
    {
        elementColorsBuffer?.Release();
        elementRadiiBuffer?.Release();
        
        elementColorsBuffer = new ComputeBuffer(ElementColors.Length, sizeof(float) * 4);
        elementRadiiBuffer = new ComputeBuffer(ElementRadii.Length, sizeof(float));
        
        elementColorsBuffer.SetData(ElementColors);
        elementRadiiBuffer.SetData(ElementRadii);
    }

    void SetupRenderTarget()
    {
        if (renderTarget != null && 
            renderTarget.width == resolution.x && 
            renderTarget.height == resolution.y)
            return;

        if (renderTarget != null)
            renderTarget.Release();

        renderTarget = new RenderTexture(resolution.x, resolution.y, 0, RenderTextureFormat.ARGB32)
        {
            enableRandomWrite = true
        };
        renderTarget.Create();
    }

    void ParseXYZAndBuildBuffers()
    {
        atoms.Clear();
        bonds.Clear();
        
        if (string.IsNullOrEmpty(xyzText))
            return;

        string[] lines = xyzText.Split('\n');
        if (lines.Length < 3)
        {
            Debug.LogError("Invalid XYZ format: insufficient lines");
            return;
        }

        // Parse atom count
        if (!int.TryParse(lines[0].Trim(), out int atomCount))
        {
            Debug.LogError("Invalid XYZ format: first line should contain atom count");
            return;
        }

        // Skip comment line (line 1)
        // Parse atoms starting from line 2
        for (int i = 2; i < lines.Length && atoms.Count < atomCount; i++)
        {
            string line = lines[i].Trim();
            if (string.IsNullOrEmpty(line)) continue;

            string[] parts = line.Split(new char[] { ' ', '\t' }, System.StringSplitOptions.RemoveEmptyEntries);
            if (parts.Length >= 4)
            {
                string element = parts[0];
                
                if (float.TryParse(parts[1], NumberStyles.Float, CultureInfo.InvariantCulture, out float x) &&
                    float.TryParse(parts[2], NumberStyles.Float, CultureInfo.InvariantCulture, out float y) &&
                    float.TryParse(parts[3], NumberStyles.Float, CultureInfo.InvariantCulture, out float z))
                {
                    uint elementIndex = ElementMap.ContainsKey(element) ? ElementMap[element] : 1; // Default to Carbon
                    atoms.Add(new AtomData(new Vector3(x, y, z), elementIndex));
                }
            }
        }

        // Generate bonds based on distance
        for (int i = 0; i < atoms.Count; i++)
        {
            for (int j = i + 1; j < atoms.Count; j++)
            {
                float distance = Vector3.Distance(atoms[i].position, atoms[j].position);
                if (distance < bondDistanceThreshold)
                {
                    bonds.Add(new BondData((uint)i, (uint)j, 0.1f));
                }
            }
        }

        // Update compute buffers
        UpdateAtomBuffer();
        UpdateBondBuffer();
        
        Debug.Log($"Parsed {atoms.Count} atoms and {bonds.Count} bonds");
    }

    void UpdateAtomBuffer()
    {
        atomBuffer?.Release();
        if (atoms.Count > 0)
        {
            atomBuffer = new ComputeBuffer(atoms.Count, sizeof(float) * 3 + sizeof(uint));
            atomBuffer.SetData(atoms.ToArray());
        }
    }

    void UpdateBondBuffer()
    {
        bondBuffer?.Release();
        if (bonds.Count > 0)
        {
            bondBuffer = new ComputeBuffer(bonds.Count, sizeof(uint) * 2 + sizeof(float));
            bondBuffer.SetData(bonds.ToArray());
        }
    }

    void RenderMolecule()
    {
        if (atomBuffer == null || atoms.Count == 0) return;

        int kernelIndex = computeShader.FindKernel("CSMain");
        
        // Set render target
        computeShader.SetTexture(kernelIndex, "_Target", renderTarget);
        
        // Set buffers
        computeShader.SetBuffer(kernelIndex, "_AtomBuffer", atomBuffer);
        if (bondBuffer != null)
            computeShader.SetBuffer(kernelIndex, "_BondBuffer", bondBuffer);
        computeShader.SetBuffer(kernelIndex, "_ElementColors", elementColorsBuffer);
        computeShader.SetBuffer(kernelIndex, "_ElementRadii", elementRadiiBuffer);
        
        // Set atom and bond counts
        computeShader.SetInt("_AtomCount", atoms.Count);
        computeShader.SetInt("_BondCount", bonds.Count);
        
        // Set camera matrices
        SetCameraMatrices(kernelIndex);
        
        // Set rendering parameters
        computeShader.SetFloat("_AtomScale", atomScale);
        computeShader.SetFloat("_BondScale", bondScale);
        
        // Set lighting
        computeShader.SetVector("_LightDirection", lightDirection.normalized);
        computeShader.SetVector("_LightColor", new Vector3(lightColor.r, lightColor.g, lightColor.b));
        computeShader.SetFloat("_AmbientIntensity", ambientIntensity);
        
        // Dispatch compute shader
        int threadGroupsX = Mathf.CeilToInt(resolution.x / 8f);
        int threadGroupsY = Mathf.CeilToInt(resolution.y / 8f);
        computeShader.Dispatch(kernelIndex, threadGroupsX, threadGroupsY, 1);
    }

    void SetCameraMatrices(int kernelIndex)
    {
        if (cameraTransform == null) return;
        
        Matrix4x4 viewMatrix = Matrix4x4.LookAt(
            cameraTransform.position,
            cameraTransform.position + cameraTransform.forward,
            cameraTransform.up
        );
        
        float aspect = (float)resolution.x / resolution.y;
        Matrix4x4 projectionMatrix = Matrix4x4.Perspective(fieldOfView, aspect, nearPlane, farPlane);
        
        computeShader.SetMatrix("_ViewMatrix", viewMatrix);
        computeShader.SetMatrix("_ProjectionMatrix", projectionMatrix);
        computeShader.SetMatrix("_InverseViewMatrix", viewMatrix.inverse);
        computeShader.SetMatrix("_InverseProjectionMatrix", projectionMatrix.inverse);
        computeShader.SetVector("_CameraPosition", cameraTransform.position);
        computeShader.SetVector("_CameraForward", cameraTransform.forward);
    }

    void OnDisable()
    {
        atomBuffer?.Release();
        bondBuffer?.Release();
        elementColorsBuffer?.Release();
        elementRadiiBuffer?.Release();
        
        if (renderTarget != null)
        {
            renderTarget.Release();
            renderTarget = null;
        }
    }
}