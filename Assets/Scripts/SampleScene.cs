using System;
using System.IO;
using UnityEngine;
using UnityEngine.UI;

public class SampleScene : MonoBehaviour
{
    [SerializeField]
    private Button chooseBtn;
    [SerializeField]
    private RawImage rawImage;

    private void Start()
    {
        chooseBtn.GetComponent<Button>().onClick.AddListener(() =>
        {
            OpenChooser();
        });
    }

    private string GetCacheDirectory()
    {
        var ret = string.Empty;
#if UNITY_ANDROID && !UNITY_EDITOR
        using var unityPlayer = new AndroidJavaClass("com.unity3d.player.UnityPlayer");
        using var currentActivity = unityPlayer.GetStatic<AndroidJavaObject>("currentActivity");
        using var context = currentActivity.Call<AndroidJavaObject>("getApplicationContext");
        using var filesDir = context.Call<AndroidJavaObject>("getCacheDir");
        ret = filesDir.Call<string>("getAbsolutePath");
#endif
        return ret;
    }

    private void OpenChooser()
    {
#if UNITY_ANDROID && !UNITY_EDITOR
        using var unityPlayer = new AndroidJavaClass( "com.unity3d.player.UnityPlayer" );
        using var currentActivity = unityPlayer.GetStatic<AndroidJavaObject>( "currentActivity" );
        currentActivity.CallStatic("openChooser");
#endif
    }

    private void OnActivityResult(string fileName)
    {
        var filePath = $"{GetCacheDirectory()}/{fileName}";
        try
        {
            var fileBytes = File.ReadAllBytes(filePath);
            if (null != fileBytes)
            {
                var texture = new Texture2D(1, 1);
                texture.LoadImage(fileBytes);
                rawImage.texture = texture;
            }
        }
        catch (Exception e)
        {
            Debug.LogError(e.Message);
        }
    }
}
