using BrilliantSkies.Core.Logger;
using BrilliantSkies.Modding.Containers;
using UnityEngine;

namespace AdvShields.Behaviours
{
    public class HitEffectBehaviour : MonoBehaviour
    {
        private float _duration;

        private float _magnitude;

        private Color _hitColor;

        private float _progress;

        private float timeRemaining;

        private Vector4 _worldHit;

        private Transform shieldTransform;

        //private MaterialPropertyBlock _propertyBlock;
        //private Renderer _renderer;
        //private Material _material;

        public void Initialize(Vector4 worldHit, Color hitColor, float magnitude, float duration, Transform shieldTrans)
        {
            //AdvLogger.LogInfo("Creating the HitEffect", LogOptions.OnlyInDeveloperLog);
            //this.gameObject.transform.position = worldHit;
            shieldTransform = shieldTrans;
            _duration = duration;
            _magnitude = magnitude;
            _hitColor = hitColor;
            _progress = 0;
            timeRemaining = duration;
            _worldHit = worldHit;
            _worldHit.w = 0;
            //_worldHit = Quaternion.Inverse(transform.rotation) * worldHit;
            Material material = GetComponent<MeshRenderer>().material;
            /*
            AdvLogger.LogInfo($"Shield scale is {shieldTrans.localScale}");
            AdvLogger.LogInfo($"Hit effect material is {material.name}");
            */
            /*
            Transform effectTransform = gameObject.transform;
            effectTransform.localScale = shieldTrans.localScale;
            effectTransform.position = shieldTrans.position;
            */
            /*
            AdvLogger.LogInfo($"Effect position is {this.transform.position}, does that look right? Shield's position is {shieldTrans.position}", LogOptions.OnlyInDeveloperLog);
            AdvLogger.LogInfo($"Effect scale is {this.transform.lossyScale}, does that look right? Shield's scale is {shieldTrans.lossyScale}", LogOptions.OnlyInDeveloperLog);
            AdvLogger.LogInfo($"Showing worldHit as {worldHit}. Magniture is {magnitude}");
            */


            //_renderer = GetComponent<MeshRenderer>();

            //_propertyBlock = new MaterialPropertyBlock();
            //_renderer.GetPropertyBlock(_propertyBlock);

            //_propertyBlock.SetColor("_Color", hitColor);
            //_propertyBlock.SetVector("_WorldHit", worldHit);
            //_propertyBlock.SetFloat("_Magnitude", magnitude);
            //_propertyBlock.SetFloat("_Progress", _progress);

            //_renderer.SetPropertyBlock(_propertyBlock);

            enabled = true;
            gameObject.SetActive(true);
            //transform.localScale = Vector3.one;
            Vector3 localHit = shieldTrans.InverseTransformPoint(worldHit);
            material.SetFloat("_RippleStartTime", Time.time);
            //material.SetVector("_RippleOrigin", new Vector4(localHit.x, localHit.y, localHit.z, 0));
            //below line is temp
            material.SetVector("_RippleOrigin", new Vector4(0.5f, 0.5f, 0, 0));
            Vector3 lossyScale = shieldTransform.lossyScale;
            //below line is temp
            Vector3 lossyscale = new Vector4(1, 1, 1, 1);
            material.SetVector("_RippleScale", lossyScale);
            material.SetFloat("_RippleSize", magnitude * 10);
            material.SetFloat("_RippleStrength", magnitude * 10);
            material.SetFloat("_RippleSpeed", 1.2f);
            //AdvLogger.LogInfo($"worldHit is {worldHit}. localHit is {localHit}");
            material.SetColor("_GridColor", _hitColor);

            var renderer = GetComponent<MeshRenderer>();
            var matInstance = renderer.material;        // instance (creates one if none)
            var matShared = renderer.sharedMaterial;  // asset

            // Log what we set
            AdvLogger.LogInfo($"[Init] Set _RippleOrigin -> localHit = {localHit} on material instance name={matInstance.name}", LogOptions._AlertDevInGame);

            // Read back the value from the same Material object you wrote to:
            Vector4 gotFromInstance = matInstance.GetVector("_RippleOrigin");
            float gotStartFromInstance = matInstance.GetFloat("_RippleStartTime");
            AdvLogger.LogInfo($"[Init] matInstance reports _RippleOrigin={gotFromInstance}, _RippleStartTime={gotStartFromInstance}", LogOptions._AlertDevInGame);

            // Also log what renderer.sharedMaterial currently has (the asset)
            if (matShared != null)
            {
                Vector4 gotShared = matShared.GetVector("_RippleOrigin");
                AdvLogger.LogInfo($"[Init] sharedMaterial reports _RippleOrigin={gotShared}", LogOptions._AlertDevInGame);
            }

            // Finally read what the renderer *is currently using* at render time:
            AdvLogger.LogInfo($"Renderer.material == matInstance? {ReferenceEquals(renderer.material, matInstance)}", LogOptions._AlertDevInGame);

            //var check = this.isActiveAndEnabled;

            //Debug.Log($"Hit animation started at {worldHit}");
        }

        private void Update()
        {
            Material _material = GetComponent<MeshRenderer>().material;
            timeRemaining -= Time.deltaTime * Time.timeScale;
            if (timeRemaining < 0)
            {
                AdvLogger.LogInfo("Effect destroyed", LogOptions.OnlyInDeveloperLog);

                enabled = false;
                gameObject.SetActive(false);
                Destroy(this);
                Destroy(gameObject);

                return;
            }
            if (_material == null) return;
            /*
            if (_progress >= 1)
            {
                AdvLogger.LogInfo("Effect destroyed", LogOptions.OnlyInDeveloperLog);

                enabled = false;
                gameObject.SetActive(false);
                Destroy(this);
                Destroy(gameObject);

                return;
            }
            */
            
            //_progress = Mathf.Clamp01(_progress + Time.deltaTime /*/ _duration*/);

            /*
            _material.SetFloat("_RippleSize", _magnitude);
            _material.SetFloat("_RippleStrength", _magnitude);
            _material.SetFloat("_RippleSpeed", _magnitude);
            */
            //_material.SetColor("_GridColor", _hitColor);
            /*
            _material.SetFloat("_Magnitude", _magnitude);
            _material.SetColor("_Color", _hitColor);
            _material.SetFloat("_Progress", _progress);
            _material.SetVector("_WorldHit", transform.rotation * _worldHit);
            */
            /*
            gameObject.transform.localScale = shieldTransform.localScale;
            gameObject.transform.position = shieldTransform.position;
            */

            //_renderer.GetPropertyBlock(_propertyBlock);
            //_propertyBlock.SetFloat("_Progress", _progress);
            //_renderer.SetPropertyBlock(_propertyBlock);
        }
    }
}
