using UnityEngine;
using UnityEngine.UI;

// Binds a MoleculeRaymarchDriver's RenderTexture to a RawImage (or MeshRenderer) for quick previews.
public class MoleculeRenderPreview : MonoBehaviour
{
    public MoleculeRaymarchDriver driver;
    public RawImage rawImageTarget;
    public MeshRenderer meshRendererTarget;
    public string textureProperty = "_MainTex";

    void Awake()
    {
        if (driver == null)
            driver = GetComponent<MoleculeRaymarchDriver>();
    }

    void LateUpdate()
    {
        var tex = driver != null ? driver.renderTarget : null;
        if (rawImageTarget != null)
            rawImageTarget.texture = tex;
        if (meshRendererTarget != null && meshRendererTarget.sharedMaterial != null)
            meshRendererTarget.sharedMaterial.SetTexture(textureProperty, tex);
    }
}

