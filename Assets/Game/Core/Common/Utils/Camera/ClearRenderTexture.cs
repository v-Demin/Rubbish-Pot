using UnityEngine;

[RequireComponent(typeof(Camera))]
public class ClearRenderTexture : MonoBehaviour
{
    public RenderTexture targetTexture;

    void OnPreRender()
    {
        if (targetTexture != null)
        {
            Graphics.SetRenderTarget(targetTexture);
            GL.Clear(true, true, Color.clear); // Очистка и глубины, и цвета в прозрачность
        }
    }
}