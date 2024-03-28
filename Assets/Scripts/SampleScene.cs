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
    private Button chooseBtn;
    [SerializeField]
    private RawImage rawImage;
    private WebCamTexture webCamTexture;
#if UNITY_ANDROID
    private static readonly int ANDROID_API_LEVEL_Q = 29;
#endif

    void Start()
    {
#if UNITY_ANDROID && !UNITY_EDITOR
        if (!Permission.HasUserAuthorizedPermission())
        {
            Permission.RequestUserPermission(Permission.Camera);
        }
#endif
        runCameraBtn.GetComponent<Button>().onClick.AddListener(() =>
        {
            ToggleCamera();
        });
        saveImageBtn.GetComponent<Button>().onClick.AddListener(() =>
        {
            SaveWebCamTextureToFile($"{DateTime.Now:yyyyMMddHHmmss}.jpg");
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

    private void SaveWebCamTextureToFile(string fileName)
    {
        if (null == webCamTexture || (webCamTexture.width <= 16 && webCamTexture.height <= 16))
        {
            Debug.LogError("webcam not available.");
            return;
        }
#if UNITY_ANDROID && !UNITY_EDITOR
        var apiLevel = new AndroidJavaClass("android.os.Build$VERSION").GetStatic<int>("SDK_INT");
        if (apiLevel <= ANDROID_API_LEVEL_Q)
        {
            if (!Permission.HasUserAuthorizedPermission(Permission.ExternalStorageRead))
            {
                var callbacks = new PermissionCallbacks();
                callbacks.PermissionGranted += _ => SaveTextureToFile($"{DateTime.Now:yyyyMMddHHmmss}.jpg");
                Permission.RequestUserPermission(Permission.ExternalStorageRead, callbacks);
                return;
            }
        }
#endif
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
#if UNITY_ANDROID && !UNITY_EDITOR
        using var unityPlayer = new AndroidJavaClass( "com.unity3d.player.UnityPlayer" );
        using var currentActivity = unityPlayer.GetStatic<AndroidJavaObject>( "currentActivity" );
        currentActivity.CallStatic<bool>("saveImageToMediaStore", Convert.ToBase64String(imageBytes), fileName);
#endif
    }

    private void OpenChooser()
    {
#if UNITY_ANDROID && !UNITY_EDITOR
        using var unityPlayer = new AndroidJavaClass( "com.unity3d.player.UnityPlayer" );
        using var currentActivity = unityPlayer.GetStatic<AndroidJavaObject>( "currentActivity" );
        currentActivity.CallStatic("openChooser");
#endif
    }
}
