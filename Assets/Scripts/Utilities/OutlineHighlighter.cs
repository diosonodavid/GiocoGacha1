using UnityEngine;

namespace GachaGame.Utilities
{
    // Drives an outline shader's color/width via a MaterialPropertyBlock (no per-instance material)
    // to highlight the selected character/object on field.
    public class OutlineHighlighter : MonoBehaviour
    {
        [SerializeField] private Renderer targetRenderer;
        [SerializeField] private string outlineColorProperty = "_OutlineColor";
        [SerializeField] private string outlineWidthProperty = "_OutlineWidth";
        [SerializeField] private Color highlightColor = Color.yellow;
        [SerializeField] private float highlightWidth = 0.02f;

        private MaterialPropertyBlock propertyBlock;

        private void Awake() => propertyBlock = new MaterialPropertyBlock();

        public void SetHighlighted(bool highlighted)
        {
            if (targetRenderer == null) return;

            targetRenderer.GetPropertyBlock(propertyBlock);
            propertyBlock.SetColor(outlineColorProperty, highlightColor);
            propertyBlock.SetFloat(outlineWidthProperty, highlighted ? highlightWidth : 0f);
            targetRenderer.SetPropertyBlock(propertyBlock);
        }
    }
}
