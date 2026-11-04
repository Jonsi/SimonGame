using SimonDuel.Game;
using UnityEditor;
using UnityEditor.Build;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem.UI;
using UnityEngine.Rendering.Universal;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

namespace SimonDuel.EditorTools
{
    // One-time project setup, each part applied once per machine.
    // Re-run any part from the menu: CuzGames > ...
    [InitializeOnLoad]
    public static class ProjectSetup
    {
        const string Company = "CuzGames";
        const string Product = "Simon Duel";               // codename; public name comes later
        const string AppId = "com.cuzgames.duel.dev";     // test ID; never "simon"
        const string Version = "0.1.0";
        const int Build = 1;
        const string AppScenePath = "Assets/SimonDuel/Scenes/App.unity";

        static readonly Color Navy950 = new Color32(0x05, 0x0B, 0x1D, 0xFF);
        static readonly Color TextSecondary = new Color32(0xB7, 0xCA, 0xE0, 0xFF);
        static readonly Color TextMuted = new Color32(0x81, 0x99, 0xB5, 0xFF);

        static string Key(string part) => "SimonDuel.ProjectSetup." + part + "." + Application.dataPath;

        static ProjectSetup()
        {
            EditorApplication.delayCall += () =>
            {
                RunOnce("identity.v1", ApplyIdentity);
                RunOnce("web.v2", ApplyWeb);
                RunOnce("scene.v1", BuildAppScene);
                RunOnce("buildlist.v1", EnsureBuildList);
                RunOnce("quality.v1", ApplyQuality);
            };
        }

        static void RunOnce(string part, System.Action apply)
        {
            if (EditorPrefs.GetBool(Key(part), false)) return;
            apply();
            EditorPrefs.SetBool(Key(part), true);
        }

        [MenuItem("CuzGames/Apply Project Setup")]
        public static void ApplyIdentity()
        {
            PlayerSettings.companyName = Company;
            PlayerSettings.productName = Product;
            PlayerSettings.bundleVersion = Version;
            PlayerSettings.Android.bundleVersionCode = Build;
            PlayerSettings.iOS.buildNumber = Build.ToString();
            foreach (var target in new[] { NamedBuildTarget.WebGL, NamedBuildTarget.Android, NamedBuildTarget.iOS, NamedBuildTarget.Standalone })
                PlayerSettings.SetApplicationIdentifier(target, AppId);
            PlayerSettings.SplashScreen.show = false;

            if (EditorUserBuildSettings.activeBuildTarget != BuildTarget.WebGL)
                EditorUserBuildSettings.SwitchActiveBuildTarget(BuildTargetGroup.WebGL, BuildTarget.WebGL);

            AssetDatabase.SaveAssets();
            Debug.Log($"[Setup] company={Company} product={Product} id={AppId} version={Version} build={Build} target={EditorUserBuildSettings.activeBuildTarget}");
        }

        [MenuItem("CuzGames/Apply Web Settings")]
        public static void ApplyWeb()
        {
            // The page: our own template (portrait stage, navy, loading bar).
            PlayerSettings.WebGL.template = "PROJECT:SimonDuel";
            // Small download that works on any web host.
            PlayerSettings.WebGL.compressionFormat = WebGLCompressionFormat.Gzip;
            PlayerSettings.WebGL.decompressionFallback = true;
            PlayerSettings.WebGL.dataCaching = true;
            PlayerSettings.SetManagedStrippingLevel(NamedBuildTarget.WebGL, ManagedStrippingLevel.High);
            PlayerSettings.stripEngineCode = true;
            PlayerSettings.defaultInterfaceOrientation = UIOrientation.Portrait;

            // Picture: no HDR, no anti-aliasing, no post-processing (the camera turns it off in the scene).
            var urp = AssetDatabase.LoadAssetAtPath<UniversalRenderPipelineAsset>("Assets/Settings/UniversalRP.asset");
            if (urp != null)
            {
                urp.supportsHDR = false;
                urp.msaaSampleCount = 1;
                EditorUtility.SetDirty(urp);
            }

            AssetDatabase.SaveAssets();
            Debug.Log($"[Setup] web template={PlayerSettings.WebGL.template} compression={PlayerSettings.WebGL.compressionFormat} fallback={PlayerSettings.WebGL.decompressionFallback} stripping=High hdr={(urp != null ? urp.supportsHDR.ToString() : "no URP asset")}");
        }

        [MenuItem("CuzGames/Apply Single Quality Level")]
        public static void ApplyQuality()
        {
            // One quality level for every platform, so the picture never differs by device.
            var asset = AssetDatabase.LoadAllAssetsAtPath("ProjectSettings/QualitySettings.asset")[0];
            var so = new SerializedObject(asset);
            var levels = so.FindProperty("m_QualitySettings");
            while (levels.arraySize > 1) levels.DeleteArrayElementAtIndex(0); // keep the last ("Ultra"), rename it
            levels.GetArrayElementAtIndex(0).FindPropertyRelative("name").stringValue = "Game";
            so.FindProperty("m_CurrentQuality").intValue = 0;
            var perPlatform = so.FindProperty("m_PerPlatformDefaultQuality");
            for (int i = 0; i < perPlatform.arraySize; i++)
                perPlatform.GetArrayElementAtIndex(i).FindPropertyRelative("second").intValue = 0;
            so.ApplyModifiedPropertiesWithoutUndo();
            AssetDatabase.SaveAssets();
            Debug.Log($"[Setup] quality levels={QualitySettings.names.Length} name={QualitySettings.names[0]}");
        }

        [MenuItem("CuzGames/Rebuild First Scene")]
        public static void BuildAppScene()
        {
            var scene = EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);

            var cameraGo = new GameObject("Main Camera") { tag = "MainCamera" };
            cameraGo.transform.position = new Vector3(0, 0, -10);
            var cam = cameraGo.AddComponent<Camera>();
            cam.orthographic = true;
            cam.clearFlags = CameraClearFlags.SolidColor;
            cam.backgroundColor = Navy950;
            cameraGo.AddComponent<UniversalAdditionalCameraData>().renderPostProcessing = false;

            new GameObject("EventSystem", typeof(EventSystem), typeof(InputSystemUIInputModule));

            var canvasGo = new GameObject("Canvas", typeof(Canvas), typeof(CanvasScaler), typeof(GraphicRaycaster));
            canvasGo.GetComponent<Canvas>().renderMode = RenderMode.ScreenSpaceOverlay;
            var scaler = canvasGo.GetComponent<CanvasScaler>();
            scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            scaler.referenceResolution = new Vector2(1080, 1920);
            scaler.matchWidthOrHeight = 0f; // scale by width (Art Bible §5)

            var font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");

            var title = MakeText(canvasGo.transform, "Title", "<color=#FF9418>SIMON</color> <color=#20D7ED>DUEL</color>", font, 110, Color.white);
            Stretch(title.rectTransform, new Vector2(0, 0.45f), new Vector2(1, 0.65f));

            var version = MakeText(canvasGo.transform, "Version", $"{Version} ({Build})", font, 28, TextMuted);
            version.alignment = TextAnchor.LowerRight;
            Stretch(version.rectTransform, new Vector2(0, 0), new Vector2(1, 0.06f), new Vector2(-48, 0));

            var cover = new GameObject("TapToStart", typeof(RectTransform), typeof(Image));
            cover.transform.SetParent(canvasGo.transform, false);
            cover.GetComponent<Image>().color = new Color(Navy950.r, Navy950.g, Navy950.b, 0.85f);
            Stretch((RectTransform)cover.transform, Vector2.zero, Vector2.one);
            var tap = MakeText(cover.transform, "Label", "Tap to start", font, 54, TextSecondary);
            tap.raycastTarget = false;
            Stretch(tap.rectTransform, new Vector2(0, 0.3f), new Vector2(1, 0.4f));

            var boot = canvasGo.AddComponent<Boot>();
            var so = new SerializedObject(boot);
            so.FindProperty("tapToStart").objectReferenceValue = cover;
            so.ApplyModifiedPropertiesWithoutUndo();

            System.IO.Directory.CreateDirectory("Assets/SimonDuel/Scenes");
            EditorSceneManager.SaveScene(scene, AppScenePath);
            EnsureBuildList();

            if (AssetDatabase.LoadAssetAtPath<SceneAsset>("Assets/Scenes/SampleScene.unity") != null)
                AssetDatabase.DeleteAsset("Assets/Scenes/SampleScene.unity");
            if (AssetDatabase.IsValidFolder("Assets/Scenes") && AssetDatabase.FindAssets("", new[] { "Assets/Scenes" }).Length == 0)
                AssetDatabase.DeleteAsset("Assets/Scenes");

            Debug.Log($"[Setup] scene={AppScenePath} is the only scene in the build");
        }

        static void EnsureBuildList()
        {
            EditorBuildSettings.scenes = new[] { new EditorBuildSettingsScene(AppScenePath, true) };
            AssetDatabase.SaveAssets();
        }

        static Text MakeText(Transform parent, string name, string text, Font font, int size, Color color)
        {
            var go = new GameObject(name, typeof(RectTransform), typeof(Text));
            go.transform.SetParent(parent, false);
            var t = go.GetComponent<Text>();
            t.text = text;
            t.font = font;
            t.fontSize = size;
            t.fontStyle = FontStyle.Bold;
            t.color = color;
            t.alignment = TextAnchor.MiddleCenter;
            t.supportRichText = true;
            t.horizontalOverflow = HorizontalWrapMode.Overflow;
            return t;
        }

        static void Stretch(RectTransform rt, Vector2 min, Vector2 max, Vector2 offset = default)
        {
            rt.anchorMin = min;
            rt.anchorMax = max;
            rt.offsetMin = offset;
            rt.offsetMax = offset;
        }
    }
}
