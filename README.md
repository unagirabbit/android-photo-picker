# PhotoPicker写真選択ツールの呼び出し

Android13以降は画像選択に写真選択ツールの使用が推奨されているので使ってみる。
選択された写真はアプリ固有のキャッシュ領域へコピーする。

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
      chooserIntent.addFlags(Intent.FLAG_GRANT_READ_URI_PERMISSION);
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
