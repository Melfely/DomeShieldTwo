using AdvShields.Models;
using BrilliantSkies.Core.Serialisation.Parameters.Prototypes;
using UnityEngine;
using BrilliantSkies.Core.Help;
using BrilliantSkies.Core.Logger;
using BrilliantSkies.Modding.Containers;
using System.Collections;

namespace AdvShields.Behaviours
{
    public partial class ShieldDomeBehaviour : MonoBehaviour
    {
        private Material shieldMaterial;
        private AdvShieldVisualData visualData;
        // Shader
        //public ShaderFloatProperty Metallic { get; set; }
        //public ShaderFloatProperty Gloss { get; set; }
        //public ShaderFloatProperty Expansion { get; set; }
        //public ShaderFloatProperty NormalSwitch { get; set; }

        private ShaderFloatProperty edgeProp;
        private ShaderFloatProperty fresnelProp;
        private ShaderColorProperty baseColorProp;
        private ShaderColorProperty gridColorProp;

        private bool isActive;

        private ShaderFloatProperty progress;

        private float timeToShift = 4;

        public void Update()
        {
            /*
            edgeProp.Us = visualData.Edge.Us;
            fresnelProp.Us = visualData.Fresnel.Us;
            baseColorProp.Us = visualData.BaseColor.Us;
            gridColorProp.Us = visualData.GridColor.Us;
            */
            if (isActive && progress < 1)
            {
                progress.Us = Mathf.Clamp01(progress + Time.deltaTime / timeToShift);
            }
            else if (!isActive && progress > 0)
            {
                progress.Us = Mathf.Clamp01(progress - Time.deltaTime / timeToShift);
            }
        }

        public void Initialize(AdvShieldVisualData visualData)
        {
            AdvLogger.LogInfo("Trying to initialize the shield", LogOptions._AlertDevInGame);
            Material material = gameObject.GetComponent<MeshRenderer>().material;
            AdvLogger.LogInfo($"Material gathered: {material.name}", LogOptions._AlertDevInGame);
            AdvLogger.LogInfo($"Material shader: {material.shader.name}");
            if (!material.shader.isSupported)
            {
                AdvLogger.LogInfo($"Shader {material.shader.name} is NOT supported at runtime!");
            }
            else
            {
                AdvLogger.LogInfo($"Shader {material.shader.name} is supported.");
            }
            //material.shader = Shader.Find("Standard");
            progress = new ShaderFloatProperty(material, "_Progress", 0, 0, 1, NoLimitMode.None);
            shieldMaterial = material;
            /*
            edgeProp.Us = visualData.Edge.Us; // this will drive the material
            fresnelProp.Us = visualData.Fresnel.Us;
            baseColorProp.Us = visualData.BaseColor.Us;
            gridColorProp.Us = visualData.GridColor.Us;
            */
            AdvLogger.LogInfo("progress Set", LogOptions._AlertDevInGame);

        }

        public void UpdateSizeInfo(AdvShieldTransformData data)
        {
            transform.localScale = new Vector3(data.Width, data.Height, data.Length);
        }

        public bool SetState(bool isActive)
        {
            if (this.isActive == isActive) return false;

            this.isActive = isActive;
            return true;
        }

        private IEnumerator FadeShieldAlpha(float from, float to)
        {
            float duration = 0.5f;
            float t = 0f;
            while (t < duration)
            {
                float normalized = t / duration;
                float alpha = Mathf.Lerp(from, to, normalized);
                shieldMaterial.SetFloat("_ShieldAlpha", alpha);
                t += Time.deltaTime;
                yield return null;
            }
            shieldMaterial.SetFloat("_ShieldAlpha", to);
        }
    }
}
