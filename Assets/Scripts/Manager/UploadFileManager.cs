using System.IO;
using Cysharp.Threading.Tasks;
using UnityEditor;
using UnityEngine;
using UnityEngine.Networking;

namespace Manager
{
    public class UploadFileManager : BaseManager
    {
        private string _finalPath;

        public async UniTask<(bool, Texture, string)> SaveFile()
        {
            (bool, Texture, string) savedFile = (false, null, string.Empty);
            var allowedFileTypes = new[] { "Image files", "png,jpg,jpeg" };
            string path = EditorUtility.OpenFilePanelWithFilters("Select file", "", allowedFileTypes);

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
                EditorUtility.FocusProjectWindow();
            }

            return texture;
        }
    }
}