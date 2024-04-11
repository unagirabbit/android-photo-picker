package com.unagirabbit.photopicker;

import android.content.ActivityNotFoundException;
import android.content.ClipData;
import android.content.ContentResolver;
import android.content.Context;
import android.content.Intent;
import android.net.Uri;
import android.os.Build;
import android.os.Bundle;
import android.provider.MediaStore;
import android.util.Log;
import com.unity3d.player.UnityPlayer;
import com.unity3d.player.UnityPlayerActivity;
import java.io.File;
import java.io.FileOutputStream;
import java.io.IOException;
import java.io.InputStream;
import java.util.Objects;

public class CustomUnityPlayerActivity extends UnityPlayerActivity {

  // PhotoPickerで選択可能な最大数
  private static final int CHOOSER_PICK_IMAGES_MAX = 5;
  // Activityリクエストコード
  private static final int MEDIA_REQUEST_CODE = 83748;

  protected void onCreate(Bundle savedInstanceState) {
    super.onCreate(savedInstanceState);
  }

  @Override
  protected void onActivityResult(
    int requestCode,
    int resultCode,
    Intent resultData
  ) {
    super.onActivityResult(requestCode, resultCode, resultData);
    if (requestCode == MEDIA_REQUEST_CODE) {
      if (null != resultData.getClipData()) {
        ClipData clipData = resultData.getClipData();
        for (int i = 0; i < clipData.getItemCount(); i++) {
          mediaCopyToTemp(clipData.getItemAt(i).getUri(), i);
        }
      } else if (null != resultData.getData()) {
        Uri uri = resultData.getData();
        mediaCopyToTemp(uri, 0);
      }
      UnityPlayer.UnitySendMessage("SampleScene", "OnActivityResult", "");
    }
  }

  /**
   * メディア選択を開く
   */
  private static void openChooser() {
    Intent chooserIntent;
    if (Build.VERSION.SDK_INT < Build.VERSION_CODES.TIRAMISU) {
      // API33以上はPhotoPickerを使用
      chooserIntent = new Intent(MediaStore.ACTION_PICK_IMAGES);
      chooserIntent.putExtra(
        MediaStore.EXTRA_PICK_IMAGES_MAX,
        CHOOSER_PICK_IMAGES_MAX
      );
      chooserIntent.setType("*/*");
    } else {
      chooserIntent = new Intent(Intent.ACTION_OPEN_DOCUMENT);
      chooserIntent.addCategory(Intent.CATEGORY_OPENABLE);
      // 複数選択可能
      chooserIntent.putExtra(Intent.EXTRA_ALLOW_MULTIPLE, true);
      // 画像と動画を選択可能とする
      chooserIntent.setType("*/*");
      chooserIntent.putExtra(
        Intent.EXTRA_MIME_TYPES,
        new String[] { "image/*", "video/*" }
      );
    }
    try {
      UnityPlayer.currentActivity.startActivityForResult(
        chooserIntent,
        MEDIA_REQUEST_CODE
      );
    } catch (ActivityNotFoundException e) {
      Log.e("Unity", Objects.requireNonNull(e.getMessage()));
    }
  }

  /**
   * アプリ固有のキャッシュ領域へコピーする
   * @param uri Uri
   * @param index ファイル名が被らないように連番index
   */
  private static void mediaCopyToTemp(Uri uri, int index) {
    Context context = UnityPlayer.currentActivity.getApplicationContext();
    ContentResolver resolver = context.getContentResolver();
    String fileName = String.format("temp_%03d", index);
    File cacheFile = new File(context.getCacheDir(), fileName);
    InputStream inputStream = null;
    FileOutputStream outputStream = null;
    try {
      inputStream = resolver.openInputStream(uri);
      if (inputStream == null) return;
      outputStream = new FileOutputStream(cacheFile);
      byte[] buffer = new byte[1024];
      int length;
      while ((length = inputStream.read(buffer)) > 0) {
        outputStream.write(buffer, 0, length);
      }
    } catch (IOException e) {
      Log.e("Unity", Objects.requireNonNull(e.getMessage()));
    } finally {
      try {
        if (null != inputStream) {
          inputStream.close();
        }
        if (null != outputStream) {
          outputStream.close();
        }
      } catch (IOException e) {
        Log.e("Unity", Objects.requireNonNull(e.getMessage()));
      }
    }
  }
}
