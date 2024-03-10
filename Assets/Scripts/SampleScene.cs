using System.IO;
using UnityEngine;
using UnityEngine.UI;
#if UNITY_ANDROID
using UnityEngine.Android;
#endif

public class SampleScene : MonoBehaviour
{
    [SerializeField]
    private Button saveImageBtn;
    [SerializeField]
    private Button runCameraBtn;
    [SerializeField]
    private RawImage rawImage;
    private string appFilesDirectory;
    private WebCamTexture webCamTexture;
#if UNITY_ANDROID
    private const string CAMERA_PERMISSION = "android.permission.CAMERA";
#endif

    void Start()
    {
#if UNITY_ANDROID
        if (!Permission.HasUserAuthorizedPermission(CAMERA_PERMISSION))
        {
            Permission.RequestUserPermission(CAMERA_PERMISSION);
        }
#endif
        runCameraBtn.GetComponent<Button>().onClick.AddListener(() =>
        {
            ToggleCamera();
        });
        saveImageBtn.GetComponent<Button>().onClick.AddListener(() =>
        {
            appFilesDirectory = GetAppFilesDirectory();
            SaveRenderTextureToFile(appFilesDirectory);
        });
    }

    private void ToggleCamera()
    {
        if (null == webCamTexture)
        {
            var rect = rawImage.GetComponent<RectTransform>().rect;
            webCamTexture = new WebCamTexture((int)rect.width, (int)rect.height);
            rawImage.texture = webCamTexture;
        }
        if (webCamTexture.isPlaying)
        {
            webCamTexture.Stop();
        }
        else
        {
            webCamTexture.Play();
        }
    }

    private string GetAppFilesDirectory()
    {
#if UNITY_ANDROID && !UNITY_EDITOR
        using var unityPlayer = new AndroidJavaClass("com.unity3d.player.UnityPlayer");
        using var currentActivity = unityPlayer.GetStatic<AndroidJavaObject>("currentActivity");
        using var context = currentActivity.Call<AndroidJavaObject>("getApplicationContext");
        using var filesDir = context.Call<AndroidJavaObject>("getFilesDir");
        return filesDir.Call<string>("getAbsolutePath");
#else
        return Application.persistentDataPath;
#endif
    }

    private void SaveRenderTextureToFile(string filePath)
    {
        var texture = new Texture2D(webCamTexture.width, webCamTexture.height);
        texture.SetPixels(webCamTexture.GetPixels());
        texture.Apply();
        byte[] pngData = texture.EncodeToPNG();
        try
        {
            File.WriteAllBytes(filePath, pngData);
            Debug.Log($"save to = {filePath}");
        }
        catch (IOException ex)
        {
            Debug.LogError(ex.Message);
        }
    }
}
