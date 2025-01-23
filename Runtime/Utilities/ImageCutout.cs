using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.UI;

namespace Racer.EzTransitions.Utilities
{
    internal class ImageCutout : Image
    {
        private static readonly int StencilComp = Shader.PropertyToID("_StencilComp");
        private Material _cachedMaterial;

        public override Material materialForRendering
        {
            get
            {
                if (_cachedMaterial != null) return _cachedMaterial;

                _cachedMaterial = new Material(base.materialForRendering);
                _cachedMaterial.SetInt(StencilComp, (int)CompareFunction.NotEqual);

                return _cachedMaterial;
            }
        }

        protected override void OnDestroy()
        {
            if (_cachedMaterial != null)
            {
                if (Application.isPlaying)
                    Destroy(_cachedMaterial);
                else
                    DestroyImmediate(_cachedMaterial);
            }

            base.OnDestroy();
        }
    }
}