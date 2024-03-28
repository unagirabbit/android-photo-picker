package com.unagirabbit.photopicker;

import android.content.ActivityNotFoundException;
import android.content.ContentResolver;
import android.content.ContentValues;
import android.content.Intent;
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

    private static final int CHOOSER_PICK_IMAGES_MAX = 5;

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
    }

    /**
     * 画像を他アプリから見える領域に保存する
     *
     * @param imageBytes 画像バイト配列
     * @param fileName   保存ファイル名
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
        String outputUri = "";
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
                    outputUri = imageUri.toString();
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
        UnityPlayer.UnitySendMessage("SampleScene", "SaveImageToGalleryCallback", outputUri);
        return success;
    }

    /**
     * MediaStoreへ登録するValuesを作成
     *
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
        }
        return values;
    }

    /**
     * 共有の外部領域URIを取得する
     *
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

    private static void openChooser()
    {
        Intent chooserIntent;
        if (Build.VERSION.SDK_INT < Build.VERSION_CODES.TIRAMISU) {
            chooserIntent = new Intent(Intent.ACTION_OPEN_DOCUMENT);
            chooserIntent.addCategory(Intent.CATEGORY_OPENABLE);
            chooserIntent.addFlags(Intent.FLAG_GRANT_READ_URI_PERMISSION);
            chooserIntent.putExtra(Intent.EXTRA_ALLOW_MULTIPLE, true);
            chooserIntent.setType("*/*");
            chooserIntent.putExtra(Intent.EXTRA_MIME_TYPES, new String[]{"image/*", "video/*"});
        } else {
            // API33以上はPhotoPickerを使用する
            chooserIntent = new Intent(MediaStore.ACTION_PICK_IMAGES);
            chooserIntent.putExtra(MediaStore.EXTRA_PICK_IMAGES_MAX, CHOOSER_PICK_IMAGES_MAX);
            chooserIntent.setType("*/*");
        }
        try {
            UnityPlayer.currentActivity.startActivityForResult(chooserIntent, 0);
        } catch (ActivityNotFoundException e) {
            Log.e("", Objects.requireNonNull(e.getMessage()));
        }
    }
}
