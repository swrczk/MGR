using System.Threading;
using Cysharp.Threading.Tasks;
using UnityEngine;
using UnityEngine.UI;

namespace UI
{
    public class GifAnimator : MonoBehaviour
    {
        [SerializeField]
        private RawImage rawImage;

        [SerializeField]
        private Texture2D[] frames;

        [SerializeField]
        private float framesPerSecond;

        private int _currentFrame;

        void Start()
        {
            _currentFrame = 0;
            if (frames.Length > 0)
            {
                rawImage.texture = frames[0];
            }
        }

        public async UniTask Run(CancellationToken token)
        {
            var delay = (int)(1000.0f / framesPerSecond);
            if (frames.Length == 0) return;
            while (!token.IsCancellationRequested)
            {
                await UniTask.SwitchToThreadPool();
                _currentFrame = (_currentFrame + 1) % frames.Length;
                Debug.LogError("Frame: " + _currentFrame + " Delay: " + delay + "ms");
                await UniTask.Delay(delay, cancellationToken: token);
                
                await UniTask.SwitchToMainThread(); 
                rawImage.texture = frames[_currentFrame];
            } 
        }
    }
}