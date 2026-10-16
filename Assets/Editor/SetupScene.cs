using UnityEngine;
using UnityEditor;
using UnityEditor.SceneManagement;
using System.IO;

public class SetupScene
{
    [MenuItem("Tools/Setup Assignment Scene")]
    public static void Setup()
    {
        // 1. 設定圖片為 Sprite 格式
        ConfigureSpriteImporter("Assets/Sprites/Roulette.png");
        ConfigureSpriteImporter("Assets/Sprites/Needle.png");
        AssetDatabase.Refresh();

        // 2. 開啟或新建場景
        string scenePath = "Assets/Scenes/SampleScene.unity";
        var scene = EditorSceneManager.NewScene(NewSceneSetup.DefaultGameObjects, NewSceneMode.Single);

        // 3. 設定 Main Camera (2D 正交正視角，質感深色背景)
        Camera mainCam = Camera.main;
        if (mainCam != null)
        {
            mainCam.orthographic = true;
            mainCam.orthographicSize = 4.5f;
            mainCam.transform.position = new Vector3(0, 0, -10f);
            mainCam.backgroundColor = new Color(0.12f, 0.15f, 0.20f);
            mainCam.clearFlags = CameraClearFlags.SolidColor;
        }

        // 4. 載入 Assets 資源
        Sprite rouletteSprite = AssetDatabase.LoadAssetAtPath<Sprite>("Assets/Sprites/Roulette.png");
        Sprite needleSprite = AssetDatabase.LoadAssetAtPath<Sprite>("Assets/Sprites/Needle.png");
        AudioClip tickClip = AssetDatabase.LoadAssetAtPath<AudioClip>("Assets/Audio/RouletteTick.wav");
        AudioClip winClip = AssetDatabase.LoadAssetAtPath<AudioClip>("Assets/Audio/RouletteWin.wav");

        // 5. 建立輪盤物件 (Roulette)
        GameObject roulette = new GameObject("Roulette");
        roulette.transform.position = new Vector3(0f, -0.3f, 0f);
        roulette.transform.localScale = new Vector3(1.35f, 1.35f, 1f);

        SpriteRenderer rouletteRenderer = roulette.AddComponent<SpriteRenderer>();
        rouletteRenderer.sprite = rouletteSprite;
        rouletteRenderer.sortingOrder = 1;

        AudioSource audioSource = roulette.AddComponent<AudioSource>();
        audioSource.playOnAwake = false;
        audioSource.spatialBlend = 0f; // 2D 純音效

        RouletteController controller = roulette.AddComponent<RouletteController>();
        controller.audioSource = audioSource;
        controller.tickSound = tickClip;
        controller.winSound = winClip;
        controller.damping = 0.985f;
        controller.minMaxSpeed = 16f;
        controller.maxMaxSpeed = 24f;
        controller.acceleration = 30f;

        // 6. 建立指針物件 (Needle)
        GameObject needle = new GameObject("Needle");
        needle.transform.position = new Vector3(0f, 2.3f, 0f);
        needle.transform.localScale = new Vector3(0.7f, 0.7f, 1f);

        SpriteRenderer needleRenderer = needle.AddComponent<SpriteRenderer>();
        needleRenderer.sprite = needleSprite;
        needleRenderer.sortingOrder = 10; // 位於輪盤上方

        // 7. 儲存場景並加入 Build Settings
        EditorSceneManager.SaveScene(scene, scenePath);

        EditorBuildSettingsScene[] buildScenes = new EditorBuildSettingsScene[]
        {
            new EditorBuildSettingsScene(scenePath, true)
        };
        EditorBuildSettings.scenes = buildScenes;

        // 8. 自動產出靜止與旋轉中預覽畫面截圖
        Directory.CreateDirectory("Docs/Screenshots");
        CaptureCameraView(mainCam, 1280, 720, "Docs/Screenshots/roulette_gameplay.png");
        CaptureCameraView(mainCam, 720, 1280, "Docs/Screenshots/roulette_mobile_view.png");

        // 旋轉輪盤產生運轉中截圖
        roulette.transform.rotation = Quaternion.Euler(0, 0, -68.5f);
        CaptureCameraView(mainCam, 1280, 720, "Docs/Screenshots/roulette_spinning.png");
        roulette.transform.rotation = Quaternion.identity; // 復原

        Debug.Log("【成功】HW3 輪盤場景已自動配置完成並儲存，截圖已產出至 Docs/Screenshots/！");
    }

    private static void ConfigureSpriteImporter(string path)
    {
        TextureImporter importer = AssetImporter.GetAtPath(path) as TextureImporter;
        if (importer != null)
        {
            importer.textureType = TextureImporterType.Sprite;
            importer.spriteImportMode = SpriteImportMode.Single;
            importer.mipmapEnabled = false;
            importer.alphaIsTransparency = true;
            importer.SaveAndReimport();
        }
    }

    private static void CaptureCameraView(Camera cam, int width, int height, string savePath)
    {
        if (cam == null) return;
        RenderTexture rt = new RenderTexture(width, height, 24);
        cam.targetTexture = rt;
        Texture2D screenShot = new Texture2D(width, height, TextureFormat.RGB24, false);
        cam.Render();
        RenderTexture.active = rt;
        screenShot.ReadPixels(new Rect(0, 0, width, height), 0, 0);
        screenShot.Apply();
        cam.targetTexture = null;
        RenderTexture.active = null;
        Object.DestroyImmediate(rt);
        byte[] bytes = screenShot.EncodeToPNG();
        Object.DestroyImmediate(screenShot);
        File.WriteAllBytes(savePath, bytes);
    }
}
