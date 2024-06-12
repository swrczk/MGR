using System.IO;
using Cysharp.Threading.Tasks;
using SFB;
using UnityEngine;
using UnityEngine.Networking;

#if UNITY_EDITOR
using UnityEditor;
#endif

namespace Manager
{
    public class UploadFileManager : BaseManager
    {
        private string _finalPath;

        public async UniTask<(bool, Texture, string)> SaveFile()
        {
            (bool, Texture, string) savedFile = (false, null, string.Empty);
            string path = null;

#if UNITY_EDITOR
            var allowedFileTypes = new[] { "Image files", "png,jpg,jpeg" };
            path = EditorUtility.OpenFilePanelWithFilters("Select file", "", allowedFileTypes);
#else
            var extensions = new[] {
                new ExtensionFilter("Image Files", "png", "jpg", "jpeg"),
                new ExtensionFilter("All Files", "*"),
            };
            var paths = StandaloneFileBrowser.OpenFilePanel("Open File", "", extensions, false);

            if (paths.Length > 0)
            {
                path = paths[0];
            }
#endif

            if (string.IsNullOrEmpty(path))
            {
                Debug.Log("Operation cancelled");
            }
            else
            {
                Debug.Log("Picked file: " + path);
                _finalPath = path;
                savedFile.Item2 = await LoadImage();
                savedFile.Item1 = true;
                savedFile.Item3 = Path.GetFileNameWithoutExtension(path);
            }

            return savedFile;
        }

        private async UniTask<Texture> LoadImage()
        {
            var www = UnityWebRequestTexture.GetTexture(_finalPath);
            await www.SendWebRequest();
            Texture texture = null;
            if (www.result != UnityWebRequest.Result.Success)
            {
                Debug.Log(www.error);
            }
            else
            {
                texture = ((DownloadHandlerTexture)www.downloadHandler).texture;

#if UNITY_EDITOR
                EditorUtility.FocusProjectWindow();
#endif
            }

            return texture;
        }
    }
}