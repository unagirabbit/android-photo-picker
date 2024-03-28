using UnityEngine;
using UnityEngine.UI;
using System;
using UnityEngine.Networking;

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
    private Button chooseBtn;
    [SerializeField]
    private RawImage rawImage;

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
#endif
        runCameraBtn.GetComponent<Button>().onClick.AddListener(() =>
        {
            ToggleCamera();
        });
        saveImageBtn.GetComponent<Button>().onClick.AddListener(() =>
        {
            SaveTextureToFile($"{DateTime.Now:yyyyMMddHHmmss}.jpg");
        });
        chooseBtn.GetComponent<Button>().onClick.AddListener(() =>
        {
            OpenChooser();
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
        var apiLevel = new AndroidJavaClass("android.os.Build$VERSION").GetStatic<int>("SDK_INT");
        if (apiLevel <= 29)
        {
            if (!Permission.HasUserAuthorizedPermission(WRITE_STORAGE_PERMISSION))
            {
                var callbacks = new PermissionCallbacks();
                callbacks.PermissionGranted += _ => SaveTextureToFile($"{DateTime.Now:yyyyMMddHHmmss}.jpg");
                Permission.RequestUserPermission(WRITE_STORAGE_PERMISSION, callbacks);
                return;
            }
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
        SaveImageToGallery(texture.EncodeToJPG(), fileName);
    }

    private void SaveImageToGallery(byte[] imageBytes, string fileName)
    {
#if UNITY_ANDROID
        using var unityPlayer = new AndroidJavaClass( "com.unity3d.player.UnityPlayer" );
        using var currentActivity = unityPlayer.GetStatic<AndroidJavaObject>( "currentActivity" );
        var ret = currentActivity.CallStatic<bool>("saveImageToMediaStore", Convert.ToBase64String(imageBytes), fileName);
        Debug.Log($"SaveImageToGallery ret = {ret}");
#endif
    }

    private void OpenChooser()
    {
#if UNITY_ANDROID
        using var unityPlayer = new AndroidJavaClass( "com.unity3d.player.UnityPlayer" );
        using var currentActivity = unityPlayer.GetStatic<AndroidJavaObject>( "currentActivity" );
        currentActivity.CallStatic("openChooser");
#endif
    }
}
