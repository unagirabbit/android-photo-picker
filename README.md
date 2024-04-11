# UnityでPhotoPicker写真選択ツールの呼び出し

[Google Play の写真と動画の権限に関するポリシーの詳細](https://support.google.com/googleplay/android-developer/answer/14115180?hl=ja)  

2025年初頭から`READ_MEDIA_IMAGES`、`READ_MEDIA_VIDEO`権限はメディアを管理するようなアプリでしか使えなくなります。  
端末のフォトから画像を選択したり、アプリで表示するだけなら権限は必要ありません。  
また、Android13以降でアプリ固有領域外に保存されているメディアを選択するのに、写真選択ツール（PhotoPicker）の使用が推奨されます。  

[写真選択ツール](https://developer.android.com/training/data-storage/shared/photopicker?hl=ja)
C#コードだけでAndroidのクラスを呼び出すことも可能ですが、今回はUnityPlayerActivityを継承したクラスで実装します。
`PhotoPicker`は`Jetpack Activity`の`PickVisualMedia`、`PickMultipleVisualMedia`を使用すべきですが、`registerForActivityResult`は`Activity`で使用できないので`MediaStore.ACTION_PICK_IMAGES`を使います。

<https://qiita.com/y-mimura/items/b5b9b6f19ae283108a28>

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
