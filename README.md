# UnityでPhotoPicker写真選択ツールの呼び出し

[Google Play の写真と動画の権限に関するポリシーの詳細](https://support.google.com/googleplay/android-developer/answer/14115180?hl=ja)  

2025年初頭から`READ_MEDIA_IMAGES`、`READ_MEDIA_VIDEO`権限はメディアを管理するようなアプリでしか使えなくなります。  
また、Android13以降アプリ外部に保存されているメディアを選択するには、写真選択ツール（PhotoPicker）の使用が推奨されます。  

UnityでAndroidのネイティブ実装を行う場合、`AndroidJavaClass`を使うかJava/Kotlinでプラグインを作成するかです。  
[C# スクリプトから Java や Kotlin のプラグインコードを呼び出す](https://docs.unity3d.com/ja/2022.3/Manual/android-plugins-java-code-from-c-sharp.html)  
今回は`UnityPlayerActivity`を継承したクラスで実装を行います。  
[カスタムアクティビティの作成](https://docs.unity3d.com/ja/2022.3/Manual/android-custom-activity.html)
  
[写真選択ツール](https://developer.android.com/training/data-storage/shared/photopicker?hl=ja)  
`PhotoPicker`は`Jetpack Activity`の`PickVisualMedia`、`PickMultipleVisualMedia`を使用すべきですが、  
`registerForActivityResult`は`Fragment`や`ComponentActivity`を継承する必要があります。  
`UnityPlayerActivity`は`Activity`で使用できないため[MediaStore.ACTION_PICK_IMAGES](https://developer.android.com/reference/android/provider/MediaStore#ACTION_PICK_IMAGES)を使用します。  

```java
  /**
   * 画像/動画を選択可能なActivityを表示
  */
  private static void openChooser() {
    Intent chooserIntent;
    if (Build.VERSION.SDK_INT >= Build.VERSION_CODES.TIRAMISU) {
      // API33以上はPhotoPickerを使用
      chooserIntent = new Intent(MediaStore.ACTION_PICK_IMAGES);
      chooserIntent.putExtra(
        MediaStore.EXTRA_PICK_IMAGES_MAX,
        5 // 選択可能な最大枚数を指定
      );
      chooserIntent.setType("*/*");
    } else {
      chooserIntent = new Intent(Intent.ACTION_OPEN_DOCUMENT);
      chooserIntent.addCategory(Intent.CATEGORY_OPENABLE);
      chooserIntent.putExtra(Intent.EXTRA_ALLOW_MULTIPLE, true);
      chooserIntent.setType("*/*");
      chooserIntent.putExtra(
        Intent.EXTRA_MIME_TYPES,
        new String[] { "image/*", "video/*" }
      );
    }
    try {
      UnityPlayer.currentActivity.startActivityForResult(
        chooserIntent,
        0
      );
    } catch (ActivityNotFoundException e) {
      Log.e("Unity", Objects.requireNonNull(e.getMessage()));
    }
  }
```
