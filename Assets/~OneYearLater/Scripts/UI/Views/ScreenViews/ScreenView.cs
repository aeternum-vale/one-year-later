using System;
using System.Threading;
using Cysharp.Threading.Tasks;
using NaughtyAttributes;
using OneYearLater.Management;
using UnityEngine;

namespace OneYearLater.UI.Views.ScreenViews
{
    [RequireComponent(typeof(CanvasGroupFader))]
    public class ScreenView : MonoBehaviour
    {
        public EScreenViewKey Key => _key;
        [SerializeField] private EScreenViewKey _key;
        [SerializeField] private bool _disableCanvasOnFade = true;

        [SerializeField] [ReadOnly] private CanvasGroupFader _canvasGroupFader;
        [SerializeField] [ReadOnly] private Canvas _canvas;


        private void FindComponents()
        {
            if (_canvasGroupFader == null)
                _canvasGroupFader = GetComponent<CanvasGroupFader>();
            if (_canvas == null)
                _canvas = gameObject.GetComponentInParent<Canvas>(true);
        }

        private void OnEnable()
        {
            FindComponents();
        }

        private void Reset()
        {
            FindComponents();
        }

        private void Awake()
        {
            FindComponents();
            _canvasGroupFader.OwnFadeDuration = Constants.ScreenViewFadeDuration;
        }

        public async UniTask FadeAsync(CancellationToken token)
        {
            await _canvasGroupFader.FadeAsync(token);
            if (_disableCanvasOnFade)
                _canvas.gameObject.SetActive(false);
        }

        public UniTask UnfadeAsync(CancellationToken token)
        {
            _canvas.gameObject.SetActive(true);
            return _canvasGroupFader.UnfadeAsync(token);
        }

#if UNITY_EDITOR
        private void OnValidate()
        {
            FindComponents();
        }
#endif
    }
}