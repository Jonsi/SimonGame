using SimonDuel.Game;
using SimonDuel.Rules;
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
        const string FeelPath = "Assets/SimonDuel/Settings/Feel.asset";
        const string DuelRulesPath = "Assets/SimonDuel/Settings/DuelRules.asset";
        const string TimingPath = "Assets/SimonDuel/Settings/Timing.asset";
        const string StandInArt = "Assets/SimonDuel/Art/StandIn/";
        const string StandInAudio = "Assets/SimonDuel/Audio/StandIn/";

        static readonly Color Navy950 = new Color32(0x05, 0x0B, 0x1D, 0xFF);
        static readonly Color TextSecondary = new Color32(0xB7, 0xCA, 0xE0, 0xFF);
        static readonly Color TextMuted = new Color32(0x81, 0x99, 0xB5, 0xFF);
        static readonly Color Player1 = new Color32(0xFF, 0x94, 0x18, 0xFF);
        static readonly Color Player2 = new Color32(0x20, 0xD7, 0xED, 0xFF);
        static readonly Color Panel = new Color32(0x0B, 0x22, 0x42, 0xFF);
        static readonly Color Raised = new Color32(0x12, 0x34, 0x5C, 0xFF);
        static readonly Color Cavity = new Color32(0x07, 0x14, 0x2B, 0xFF);
        static readonly Color Line = new Color32(0x2A, 0x52, 0x7D, 0xFF);
        static readonly Color Twist = new Color32(0xA6, 0x64, 0xFF, 0xFF);
        static readonly Color TwistText = new Color32(0xCD, 0xB0, 0xFF, 0xFF);

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
            scaler.referenceResolution = new Vector2(1920, 1080);
            scaler.screenMatchMode = CanvasScaler.ScreenMatchMode.Expand; // the whole layout always fits; extra window is background

            var font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");

            var feel = LoadSettings<Feel>(FeelPath);
            var rules = LoadSettings<DuelRules>(DuelRulesPath);
            var timing = LoadSettings<Timing>(TimingPath);

            var board = BuildBoard(canvasGo.transform, feel, out var timerRing);
            var banner = BuildBanner(canvasGo.transform, font, out var bannerText);
            var panels = new[]
            {
                BuildPlayerPanel(canvasGo.transform, font, -699, "1", "PLAYER 1", rules.rules.ralliesToWin),
                BuildPlayerPanel(canvasGo.transform, font, 699, "2", "PLAYER 2", rules.rules.ralliesToWin),
            };
            var length = MakeText(canvasGo.transform, "Length", "LENGTH 2", font, 30, TextMuted);
            Place(length.rectTransform, 0, -498, 400, 50);

            var version = MakeText(canvasGo.transform, "Version", $"{Version} ({Build})", font, 24, TextMuted);
            version.alignment = TextAnchor.LowerRight;
            Stretch(version.rectTransform, new Vector2(0, 0), new Vector2(1, 0.06f), new Vector2(-32, 0));

            var paused = BuildCover(canvasGo.transform, "Paused", 1f);
            var pausedText = MakeText(paused.transform, "Text", "Paused: click to carry on", font, 54, TextSecondary);
            Stretch(pausedText.rectTransform, Vector2.zero, Vector2.one);
            paused.SetActive(false);

            var result = BuildCover(canvasGo.transform, "MatchResult", 0.96f);
            var resultTitle = MakeText(result.transform, "Title", "PLAYER 1 WINS", font, 110, Color.white);
            Place(resultTitle.rectTransform, 0, 210, 1400, 150);
            var resultScore = MakeText(result.transform, "Score", "2 - 1", font, 90, TextSecondary);
            Place(resultScore.rectTransform, 0, 70, 600, 120);
            var rematch = MakeButton(result.transform, "Rematch", "REMATCH", font, 44, Player1, Cavity);
            Place((RectTransform)rematch.transform, 0, -90, 460, 110);
            var home = MakeButton(result.transform, "Home", "Home", font, 30, Raised, TextSecondary);
            Place((RectTransform)home.transform, 0, -220, 220, 72);
            result.SetActive(false);

            var cover = BuildCover(canvasGo.transform, "TapToStart", 0.9f);
            var title = MakeText(cover.transform, "Title", "<color=#FF9418>SIMON</color> <color=#20D7ED>DUEL</color>", font, 170, Color.white);
            title.raycastTarget = false;
            Place(title.rectTransform, 0, 90, 1600, 240);
            var tap = MakeText(cover.transform, "Label", "Tap to start", font, 54, TextSecondary);
            tap.raycastTarget = false;
            Place(tap.rectTransform, 0, -110, 800, 90);

            var boot = canvasGo.AddComponent<Boot>();
            var so = new SerializedObject(boot);
            so.FindProperty("tapToStart").objectReferenceValue = cover;
            so.ApplyModifiedPropertiesWithoutUndo();

            var duelScreen = canvasGo.AddComponent<DuelScreen>();
            Wire(duelScreen, ("feel", feel), ("banner", bannerText), ("bannerLine", banner), ("length", length), ("timerRing", timerRing),
                ("pausedCover", paused), ("resultCover", result), ("resultTitle", resultTitle), ("resultScore", resultScore),
                ("rematch", rematch), ("home", home));
            WireList(duelScreen, "players", panels);

            var runner = canvasGo.AddComponent<DuelRunner>();
            Wire(runner, ("rules", rules), ("timing", timing), ("board", board), ("screen", duelScreen), ("boot", boot));
            Wire(board, ("runner", runner));

            System.IO.Directory.CreateDirectory("Assets/SimonDuel/Scenes");
            EditorSceneManager.SaveScene(scene, AppScenePath);
            EnsureBuildList();

            if (AssetDatabase.LoadAssetAtPath<SceneAsset>("Assets/Scenes/SampleScene.unity") != null)
                AssetDatabase.DeleteAsset("Assets/Scenes/SampleScene.unity");
            if (AssetDatabase.IsValidFolder("Assets/Scenes") && AssetDatabase.FindAssets("", new[] { "Assets/Scenes" }).Length == 0)
                AssetDatabase.DeleteAsset("Assets/Scenes");

            Debug.Log($"[Setup] scene={AppScenePath} is the only scene in the build");
        }

        static T LoadSettings<T>(string path) where T : ScriptableObject
        {
            var settings = AssetDatabase.LoadAssetAtPath<T>(path);
            if (settings == null)
            {
                settings = ScriptableObject.CreateInstance<T>();
                AssetDatabase.CreateAsset(settings, path);
            }
            return settings;
        }

        static void Wire(Object target, params (string field, Object value)[] links)
        {
            var so = new SerializedObject(target);
            foreach (var (field, value) in links)
                so.FindProperty(field).objectReferenceValue = value;
            so.ApplyModifiedPropertiesWithoutUndo();
        }

        static void WireList(Object target, string field, Object[] values)
        {
            var so = new SerializedObject(target);
            var list = so.FindProperty(field);
            list.arraySize = values.Length;
            for (int i = 0; i < values.Length; i++)
                list.GetArrayElementAtIndex(i).objectReferenceValue = values[i];
            so.ApplyModifiedPropertiesWithoutUndo();
        }

        static GameObject BuildCover(Transform canvas, string name, float opacity)
        {
            var cover = new GameObject(name, typeof(RectTransform), typeof(Image));
            cover.transform.SetParent(canvas, false);
            cover.GetComponent<Image>().color = new Color(Navy950.r, Navy950.g, Navy950.b, opacity);
            Stretch((RectTransform)cover.transform, Vector2.zero, Vector2.one);
            return cover;
        }

        static Button MakeButton(Transform parent, string name, string label, Font font, int size, Color color, Color textColor)
        {
            var image = MakeSliced(parent, name, "pill.png", 41, color);
            image.raycastTarget = true;
            var button = image.gameObject.AddComponent<Button>();
            var text = MakeText(image.transform, "Text", label, font, size, textColor);
            text.raycastTarget = false;
            Stretch(text.rectTransform, Vector2.zero, Vector2.one);
            return button;
        }

        static PadBoard BuildBoard(Transform canvas, Feel feel, out Image timerRing)
        {
            var go = new GameObject("Board", typeof(RectTransform));
            go.transform.SetParent(canvas, false);
            var rect = (RectTransform)go.transform;
            rect.anchorMin = rect.anchorMax = new Vector2(0.5f, 0.5f);
            rect.sizeDelta = Vector2.one * feel.ringDiameter;
            var board = go.AddComponent<PadBoard>();

            var glow = MakeImage(go.transform, "Glow", LoadSprite("glow.png"));
            glow.color = new Color(Line.r, Line.g, Line.b, 0.45f);
            glow.rectTransform.offsetMin = Vector2.one * -220;
            glow.rectTransform.offsetMax = Vector2.one * 220;
            MakeImage(go.transform, "Disc", LoadSprite("disc.png")).color = Cavity;
            var ringSprite = LoadSprite("ring.png");
            MakeImage(go.transform, "Ring", ringSprite).color = Line;
            var ring = MakeImage(go.transform, "RingLit", ringSprite);
            ring.color = Player1;
            ring.type = Image.Type.Filled;
            ring.fillMethod = Image.FillMethod.Radial360;
            ring.fillOrigin = (int)Image.Origin360.Top;
            ring.fillClockwise = true;
            timerRing = ring;

            var shape = LoadSprite("pad.png");
            var pads = new Pad[Pads.All.Length];
            for (int i = 0; i < pads.Length; i++)
                pads[i] = BuildPad(go.transform, Pads.All[i], shape, feel);

            Wire(board, ("feel", feel));
            WireList(board, "pads", pads);
            return board;
        }

        static Pad BuildPad(Transform board, PadId id, Sprite shapeSprite, Feel feel)
        {
            string name = id.ToString().ToLowerInvariant();
            var go = new GameObject(id.ToString(), typeof(RectTransform), typeof(AudioSource));
            go.transform.SetParent(board, false);

            var depth = MakeImage(go.transform, "Depth", shapeSprite);
            var face = new GameObject("Face", typeof(RectTransform));
            face.transform.SetParent(go.transform, false);
            Stretch((RectTransform)face.transform, Vector2.zero, Vector2.one);
            var shape = MakeImage(face.transform, "Shape", shapeSprite);
            var gloss = MakeImage(face.transform, "Gloss", LoadSprite("pad_gloss.png"));
            gloss.rectTransform.anchorMin = gloss.rectTransform.anchorMax = gloss.rectTransform.pivot = new Vector2(0.5f, 1f);
            gloss.rectTransform.sizeDelta = new Vector2(feel.padSize.x * 0.82f, feel.padSize.y * 0.3f);
            var rim = MakeImage(face.transform, "Rim", LoadSprite("pad_rim.png"));
            var symbol = MakeImage(face.transform, "Symbol", LoadSprite($"symbol_{name}.png"));
            symbol.rectTransform.anchorMin = symbol.rectTransform.anchorMax = new Vector2(0.5f, 0.5f);

            var tone = go.GetComponent<AudioSource>();
            tone.playOnAwake = false;
            tone.spatialBlend = 0f;
            tone.clip = LoadTone($"tone_{name}.wav");

            var pad = go.AddComponent<Pad>();
            var so = new SerializedObject(pad);
            so.FindProperty("id").enumValueIndex = (int)id;
            so.FindProperty("face").objectReferenceValue = face.transform;
            so.FindProperty("shape").objectReferenceValue = shape;
            so.FindProperty("depth").objectReferenceValue = depth;
            so.FindProperty("gloss").objectReferenceValue = gloss;
            so.FindProperty("rim").objectReferenceValue = rim;
            so.FindProperty("symbol").objectReferenceValue = symbol;
            so.FindProperty("tone").objectReferenceValue = tone;
            so.ApplyModifiedPropertiesWithoutUndo();
            return pad;
        }

        static Image BuildBanner(Transform canvas, Font font, out Text text)
        {
            var line = MakeSliced(canvas, "Banner", "pill.png", 41, Player1);
            Place(line.rectTransform, 0, 456, 570, 84);
            var fill = MakeSliced(line.transform, "Fill", "pill.png", 41, Raised);
            fill.rectTransform.offsetMin = Vector2.one * 3;
            fill.rectTransform.offsetMax = Vector2.one * -3;
            text = MakeText(line.transform, "Text", "", font, 36, Color.white);
            Stretch(text.rectTransform, Vector2.zero, Vector2.one);
            return line;
        }

        static PlayerPanel BuildPlayerPanel(Transform canvas, Font font, float x, string number, string label, int ralliesToWin)
        {
            var color = number == "1" ? Player1 : Player2;
            var panel = MakeSliced(canvas, label, "panel.png", 40, Panel);
            Place(panel.rectTransform, x, -15, 390, 690);
            var line = MakeSliced(panel.transform, "Line", "panel_line.png", 40, Line);
            Stretch(line.rectTransform, Vector2.zero, Vector2.one);

            var badge = MakeImage(panel.transform, "Badge", LoadSprite("disc.png"));
            badge.color = color;
            Place(badge.rectTransform, 0, 225, 120, 120);
            var inner = MakeImage(badge.transform, "Inner", LoadSprite("disc.png"));
            inner.color = Cavity;
            Place(inner.rectTransform, 0, 0, 108, 108);
            Stretch(MakeText(badge.transform, "Number", number, font, 54, color).rectTransform, Vector2.zero, Vector2.one);

            Place(MakeText(panel.transform, "Name", label, font, 34, color).rectTransform, 0, 120, 360, 50);
            var pips = new Image[ralliesToWin];
            for (int i = 0; i < pips.Length; i++)
            {
                pips[i] = MakeImage(panel.transform, "Pip", LoadSprite("disc.png"));
                pips[i].color = Line;
                Place(pips[i].rectTransform, (i - (pips.Length - 1) / 2f) * 34, 72, 22, 22);
            }

            var turn = MakeSliced(panel.transform, "YourTurn", "pill.png", 41, color);
            Place(turn.rectTransform, 0, 0, 320, 60);
            Stretch(MakeText(turn.transform, "Text", "YOUR TURN", font, 28, Cavity).rectTransform, Vector2.zero, Vector2.one);
            turn.gameObject.SetActive(false);

            // Twists arrive in step 5; dimmed until then.
            var twists = new GameObject("Twists", typeof(RectTransform), typeof(CanvasGroup));
            twists.transform.SetParent(panel.transform, false);
            Stretch((RectTransform)twists.transform, Vector2.zero, Vector2.one);
            twists.GetComponent<CanvasGroup>().alpha = 0.35f;

            for (int i = 0; i < 2; i++)
            {
                var card = MakeSliced(twists.transform, "Twist", "panel.png", 40, Cavity);
                Place(card.rectTransform, i == 0 ? -87 : 87, -220, 160, 180);
                Stretch(MakeSliced(card.transform, "Line", "panel_line.png", 40, Twist).rectTransform, Vector2.zero, Vector2.one);
                Stretch(MakeText(card.transform, "Text", i == 0 ? "REVERSE" : "SHIFT", font, 24, TwistText).rectTransform, Vector2.zero, Vector2.one);
            }

            var player = panel.gameObject.AddComponent<PlayerPanel>();
            Wire(player, ("outline", line), ("yourTurn", turn.gameObject));
            WireList(player, "pips", pips);
            return player;
        }

        static Image MakeSliced(Transform parent, string name, string file, float border, Color color)
        {
            var image = MakeImage(parent, name, LoadSprite(file, border));
            image.type = Image.Type.Sliced;
            image.color = color;
            return image;
        }

        // Sets size and position from the parent's center.
        static void Place(RectTransform rt, float x, float y, float width, float height)
        {
            rt.anchorMin = rt.anchorMax = new Vector2(0.5f, 0.5f);
            rt.anchoredPosition = new Vector2(x, y);
            rt.sizeDelta = new Vector2(width, height);
        }

        static Image MakeImage(Transform parent, string name, Sprite sprite)
        {
            var go = new GameObject(name, typeof(RectTransform), typeof(Image));
            go.transform.SetParent(parent, false);
            var image = go.GetComponent<Image>();
            image.sprite = sprite;
            image.raycastTarget = false;
            Stretch(image.rectTransform, Vector2.zero, Vector2.one);
            return image;
        }

        static Sprite LoadSprite(string file, float border = 0f)
        {
            string path = StandInArt + file;
            var importer = (TextureImporter)AssetImporter.GetAtPath(path);
            if (importer.textureType != TextureImporterType.Sprite || importer.spriteBorder != Vector4.one * border)
            {
                importer.textureType = TextureImporterType.Sprite;
                importer.spriteImportMode = SpriteImportMode.Single;
                importer.mipmapEnabled = false;
                importer.alphaIsTransparency = true;
                importer.spriteBorder = Vector4.one * border;
                importer.SaveAndReimport();
            }
            return AssetDatabase.LoadAssetAtPath<Sprite>(path);
        }

        static AudioClip LoadTone(string file)
        {
            string path = StandInAudio + file;
            var importer = (AudioImporter)AssetImporter.GetAtPath(path);
            var settings = importer.defaultSampleSettings;
            settings.loadType = AudioClipLoadType.DecompressOnLoad;
            settings.compressionFormat = AudioCompressionFormat.PCM;
            settings.preloadAudioData = true;
            importer.defaultSampleSettings = settings;
            importer.SaveAndReimport();
            return AssetDatabase.LoadAssetAtPath<AudioClip>(path);
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
