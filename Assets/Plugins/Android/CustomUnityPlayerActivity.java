package com.unagirabbit.photopicker;

import android.content.ContentResolver;
import android.content.ContentValues;
import android.graphics.Bitmap;
import android.graphics.BitmapFactory;
import android.net.Uri;
import android.os.Build;
import android.os.Bundle;
import android.os.Environment;
import android.provider.MediaStore;
import android.util.Base64;
import android.util.Log;
import com.unity3d.player.UnityPlayer;
import com.unity3d.player.UnityPlayerActivity;
import java.io.IOException;
import java.io.OutputStream;
import java.util.Objects;

public class CustomUnityPlayerActivity extends UnityPlayerActivity {

  protected void onCreate(Bundle savedInstanceState) {
    super.onCreate(savedInstanceState);
  }

  /**
   * 画像を他アプリから見える領域に保存する
   * @param imageBytes 画像バイト配列
   * @param fileName 保存ファイル名
   * @return 成功失敗
   */
  private static boolean saveImageToMediaStore(
    String imageBytes,
    String fileName
  ) {
    byte[] bytes = Base64.decode(imageBytes, Base64.DEFAULT);
    Bitmap bitmap = BitmapFactory.decodeByteArray(bytes, 0, bytes.length);
    ContentResolver resolver = UnityPlayer.currentActivity.getContentResolver();
    ContentValues contentValues = getContentValues(fileName);

    OutputStream outputStream = null;
    boolean success = false;
    try {
      Uri imageUri = resolver.insert(getExternalStorageUri(), contentValues);
      if (imageUri != null) {
        outputStream = resolver.openOutputStream(imageUri);
        if (outputStream != null) {
          bitmap.compress(Bitmap.CompressFormat.JPEG, 100, outputStream);
          outputStream.flush();
          if (Build.VERSION.SDK_INT >= Build.VERSION_CODES.Q) {
            contentValues.clear();
            contentValues.put(MediaStore.Images.Media.IS_PENDING, 0);
            resolver.update(imageUri, contentValues, null, null);
          }
          Log.d(
            "saveImageToMediaStore",
            "Image saved to " + imageUri.getPath()
          );
          success = true;
        } else {
          Log.e("saveImageToMediaStore", "Failed to open output stream");
        }
      } else {
        Log.e("saveImageToMediaStore", "Failed to get image URI");
      }
    } catch (IOException e) {
      Log.e("saveImageToMediaStore", "Failed to save image", e);
    } catch (Exception e) {
      Log.e("saveImageToMediaStore", "An error occurred", e);
    } finally {
      if (outputStream != null) {
        try {
          outputStream.close();
        } catch (IOException e) {
          Log.e("saveImageToMediaStore", "Failed to close output stream", e);
        }
      }
      if (bitmap != null) {
        bitmap.recycle();
      }
    }
    return success;
  }

  /**
   * MediaStoreへ登録するValuesを作成
   * @param fileName ファイル名
   * @return ContentValues
   */
  private static ContentValues getContentValues(String fileName) {
    ContentValues values = new ContentValues();
    values.put(MediaStore.Images.Media.DISPLAY_NAME, fileName);
    values.put(MediaStore.Images.Media.MIME_TYPE, "image/jpeg");
    if (Build.VERSION.SDK_INT >= Build.VERSION_CODES.Q) {
      values.put(
        MediaStore.Images.Media.RELATIVE_PATH,
        Environment.DIRECTORY_PICTURES
      );
      // 作成中に他アプリから操作されないようにする
      values.put(MediaStore.Images.Media.IS_PENDING, 1);
    }
    return values;
  }

  /**
   * 共有の外部領域URIを取得する
   * @return URI
   */
  private static Uri getExternalStorageUri() {
    if (Build.VERSION.SDK_INT >= Build.VERSION_CODES.Q) {
      return MediaStore.Images.Media.getContentUri(
        MediaStore.VOLUME_EXTERNAL_PRIMARY
      );
    } else {
      return MediaStore.Images.Media.EXTERNAL_CONTENT_URI;
    }
  }
}
