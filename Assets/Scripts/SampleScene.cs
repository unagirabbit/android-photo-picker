using UnityEngine;
using UnityEngine.UI;
using System;
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
    private const string WRITE_STORAGE_PERMISSION = "android.permission.WRITE_EXTERNAL_STORAGE";
#endif

    void Start()
    {
#if UNITY_ANDROID
        if (!Permission.HasUserAuthorizedPermission(CAMERA_PERMISSION))
        {
            Permission.RequestUserPermission(CAMERA_PERMISSION);
        }
        var apiLevel = new AndroidJavaClass("android.os.Build$VERSION").GetStatic<int>("SDK_INT");
        if (apiLevel <= 29)
        {
            if (!Permission.HasUserAuthorizedPermission(WRITE_STORAGE_PERMISSION))
            {
                Permission.RequestUserPermission(WRITE_STORAGE_PERMISSION);
            }
        }
#endif
        runCameraBtn.GetComponent<Button>().onClick.AddListener(() =>
        {
            ToggleCamera();
        });
        saveImageBtn.GetComponent<Button>().onClick.AddListener(() =>
        {
            SaveTextureToFile($"{DateTime.Now:yyyyMMddHHmmss}.jpg");
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

    private void SaveTextureToFile(string fileName)
    {
        if (webCamTexture.width <= 16 && webCamTexture.height <= 16)
        {
            Debug.LogError("webcam not available.");
            return;
        }
        var texture = new Texture2D(webCamTexture.width, webCamTexture.height);
        try
        {
            texture.SetPixels(webCamTexture.GetPixels());
        }
        catch (ArgumentException e)
        {
            Debug.LogError(e);
            return;
        }
        texture.Apply();
        byte[] jpgData = texture.EncodeToJPG();
        SaveImageToGallery(jpgData, fileName);
    }

    public void SaveImageToGallery(byte[] imageBytes, string fileName)
    {
#if UNITY_ANDROID
        using var myClass = new AndroidJavaObject("com.unagirabbit.photopicker.CustomUnityPlayerActivity");
        var ret = myClass?.CallStatic<bool>("saveImageToMediaStore", Convert.ToBase64String(imageBytes), fileName);
        Debug.Log($"SaveImageToGallery ret = {ret}");
#endif
    }
}
