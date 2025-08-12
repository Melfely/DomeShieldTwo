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

        private Vector4 _worldHit;

        private Transform shieldTransform;

        //private MaterialPropertyBlock _propertyBlock;
        //private Renderer _renderer;
        //private Material _material;

        public void Initialize(Vector4 worldHit, Color hitColor, float magnitude, float duration, Transform shieldTrans)
        {
            AdvLogger.LogInfo("Creating the HitEffect", LogOptions.OnlyInDeveloperLog);
            //this.gameObject.transform.position = worldHit;
            shieldTransform = shieldTrans;
            _duration = duration;
            _magnitude = magnitude;
            _hitColor = hitColor;
            _progress = 0;
            _worldHit = Quaternion.Inverse(transform.rotation) * worldHit;
            Material material = GetComponent<MeshRenderer>().material;
            AdvLogger.LogInfo($"Shield scale is {shieldTrans.localScale}");
            AdvLogger.LogInfo($"Hit effect material is {material.name}");
            Transform effectTransform = gameObject.transform;
            effectTransform.localScale = shieldTrans.localScale;
            effectTransform.position = shieldTrans.position;

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

            material.SetFloat("_RippleTime", Time.time);
            //material.SetVector("_RippleOrigin", new Vector4(0.5f, 0.5f, 0f, 0f));
            material.SetVector("_RippleOrigin", worldHit);
            AdvLogger.LogInfo($"worldHit is {worldHit}");
            material.SetColor("_GridColor", _hitColor);

            //var check = this.isActiveAndEnabled;

            //Debug.Log($"Hit animation started at {worldHit}");
        }

        private void Update()
        {
            Material _material = GetComponent<MeshRenderer>().material;
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
            _progress = Mathf.Clamp01(_progress + Time.deltaTime /*/ _duration*/);

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
            gameObject.transform.localScale = shieldTransform.localScale;
            gameObject.transform.position = shieldTransform.position;

            //_renderer.GetPropertyBlock(_propertyBlock);
            //_propertyBlock.SetFloat("_Progress", _progress);
            //_renderer.SetPropertyBlock(_propertyBlock);
        }
    }
}
