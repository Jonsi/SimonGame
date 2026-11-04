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
        const string ComputerLevelsPath = "Assets/SimonDuel/Settings/ComputerLevels.asset";
        const string AudioMixPath = "Assets/SimonDuel/Settings/AudioMix.asset";
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

        static Feel buildFeel;

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
            buildFeel = LoadSettings<Feel>(FeelPath);

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

            var feel = buildFeel;
            var rules = LoadSettings<DuelRules>(DuelRulesPath);
            var timing = LoadSettings<Timing>(TimingPath);
            var levels = LoadSettings<ComputerLevels>(ComputerLevelsPath);
            var mix = LoadSettings<AudioMix>(AudioMixPath);

            MakeImage(canvasGo.transform, "Background", LoadSprite("background.png"));
            var board = BuildBoard(canvasGo.transform, feel, out var timerRing);
            var banner = BuildBanner(canvasGo.transform, font, out var bannerText);
            var panels = new[]
            {
                BuildPlayerPanel(canvasGo.transform, font, -699, "1", "PLAYER 1", rules.rules),
                BuildPlayerPanel(canvasGo.transform, font, 699, "2", "PLAYER 2", rules.rules),
            };
            var length = MakeText(canvasGo.transform, "Length", "LENGTH 2", font, 30, TextMuted);
            Place(length.rectTransform, 0, -498, 400, 50);

            var version = MakeText(canvasGo.transform, "Version", $"{Version} ({Build})", font, 24, TextMuted);
            version.alignment = TextAnchor.LowerRight;
            Stretch(version.rectTransform, new Vector2(0, 0), new Vector2(1, 0.06f), new Vector2(-32, 0));

            var pause = MakeButton(canvasGo.transform, "Pause", "", font, 20, Raised, TextSecondary);
            var pauseRect = (RectTransform)pause.transform;
            pauseRect.anchorMin = pauseRect.anchorMax = new Vector2(0f, 1f);
            pauseRect.anchoredPosition = new Vector2(70, -62);
            pauseRect.sizeDelta = new Vector2(84, 84);
            var pauseIcon = MakeImage(pause.transform, "Icon", LoadSprite("pause.png"));
            pauseIcon.color = TextSecondary;
            Place(pauseIcon.rectTransform, 0, 0, 44, 44);

            var paused = BuildCover(canvasGo.transform, "Paused", 1f);
            paused.GetComponent<Image>().sprite = LoadSprite("background.png");
            paused.GetComponent<Image>().color = Color.white;
            var pausedText = MakeText(paused.transform, "Text", "Paused", font, 90, TextSecondary);
            Place(pausedText.rectTransform, 0, 150, 800, 130);
            var resume = MakeButton(paused.transform, "Resume", "CARRY ON", font, 44, Player1, Cavity);
            Place((RectTransform)resume.transform, 0, -10, 460, 110);
            var quit = MakeButton(paused.transform, "Quit", "Quit to menu", font, 30, Raised, TextSecondary);
            Place((RectTransform)quit.transform, 0, -140, 320, 72);
            paused.SetActive(false);

            var result = BuildCover(canvasGo.transform, "MatchResult", 1f);
            result.GetComponent<Image>().sprite = LoadSprite("background.png");
            result.GetComponent<Image>().color = Color.white;
            result.AddComponent<CanvasGroup>();
            var resultTitle = MakeText(result.transform, "Title", "PLAYER 1 WINS", font, 110, Color.white);
            Place(resultTitle.rectTransform, 0, 210, 1400, 150);
            var resultScore = MakeText(result.transform, "Score", "2 - 1", font, 90, TextSecondary);
            Place(resultScore.rectTransform, 0, 70, 600, 120);
            var rematch = MakeButton(result.transform, "Rematch", "REMATCH", font, 44, Player1, Cavity);
            Place((RectTransform)rematch.transform, 0, -90, 460, 110);
            var home = MakeButton(result.transform, "Home", "Home", font, 30, Raised, TextSecondary);
            Place((RectTransform)home.transform, 0, -220, 220, 72);
            var notes = MakeButton(result.transform, "Notes", "Download match notes", font, 26, Panel, TextMuted);
            Place((RectTransform)notes.transform, 0, -320, 440, 64);
            var notesLabel = notes.GetComponentInChildren<Text>();
            result.SetActive(false);

            var menu = BuildMenu(canvasGo.transform, font, levels, out var menuButtons);
            var openSettings = MakeButton(menu.transform, "OpenSettings", "SETTINGS", font, 26, Raised, Color.white);
            Place((RectTransform)openSettings.transform, 160, -445, 280, 68);
            var howToPlay = MakeButton(menu.transform, "HowToPlay", "HOW TO PLAY", font, 26, Raised, Color.white);
            Place((RectTransform)howToPlay.transform, -160, -445, 300, 68);
            var settingsCard = BuildSettingsCard(menu.transform, font);

            var boot = canvasGo.AddComponent<Boot>();

            var duelScreen = canvasGo.AddComponent<DuelScreen>();
            Wire(duelScreen, ("feel", feel), ("banner", bannerText), ("bannerLine", banner), ("length", length), ("timerRing", timerRing),
                ("pausedCover", paused), ("resultCover", result), ("resultTitle", resultTitle), ("resultScore", resultScore),
                ("rematch", rematch), ("home", home), ("notes", notes), ("notesLabel", notesLabel),
                ("rematchLabel", rematch.GetComponentInChildren<Text>()), ("menu", menu), ("openSettings", openSettings),
                ("settingsCard", settingsCard), ("pause", pause), ("resume", resume), ("quit", quit), ("howToPlay", howToPlay));
            WireList(duelScreen, "players", panels);
            WireList(duelScreen, "menuButtons", menuButtons);

            var runner = canvasGo.AddComponent<DuelRunner>();
            var soundBoard = BuildSoundBoard(canvasGo, mix);
            var tutorialScript = canvasGo.AddComponent<Tutorial>();
            Wire(tutorialScript, ("board", board), ("screen", duelScreen), ("sounds", soundBoard), ("timing", timing));

            var effectsGo = new GameObject("Effects", typeof(RectTransform));
            effectsGo.transform.SetParent(canvasGo.transform, false);
            Stretch((RectTransform)effectsGo.transform, Vector2.zero, Vector2.one);
            var effects = effectsGo.AddComponent<Effects>();
            Wire(effects, ("feel", feel), ("piece", LoadSprite("disc.png")));

            Wire(runner, ("rules", rules), ("computerLevels", levels), ("timing", timing), ("board", board), ("screen", duelScreen),
                ("sounds", soundBoard), ("effects", effects), ("feel", feel), ("tutorial", tutorialScript));
            Wire(board, ("runner", runner), ("mix", mix));

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

        static SoundBoard BuildSoundBoard(GameObject canvas, AudioMix mix)
        {
            var source = canvas.AddComponent<AudioSource>();
            source.playOnAwake = false;
            source.spatialBlend = 0f;
            var board = canvas.AddComponent<SoundBoard>();
            Wire(board, ("source", source), ("mix", mix));
            var names = new[] { "done", "wrong", "twist", "point", "win", "turn1", "turn2", "click" };
            WireList(board, "clips", System.Array.ConvertAll<string, Object>(names, n => LoadTone($"fx_{n}.wav")));
            return board;
        }

        static GameObject BuildSettingsCard(Transform menu, Font font)
        {
            var card = BuildCover(menu, "Settings", 1f);
            var title = MakeText(card.transform, "Title", "SETTINGS", font, 70, Color.white);
            title.raycastTarget = false;
            Place(title.rectTransform, 0, 250, 800, 100);

            var sound = MakeButton(card.transform, "Sound", "SOUND", font, 32, Raised, Color.white);
            Place((RectTransform)sound.transform, 0, 110, 620, 92);
            var motion = MakeButton(card.transform, "ReduceMotion", "REDUCE MOTION", font, 32, Raised, Color.white);
            Place((RectTransform)motion.transform, 0, 0, 620, 92);
            var slow = MakeButton(card.transform, "SlowPlayback", "SLOWER PATTERNS", font, 32, Raised, Color.white);
            Place((RectTransform)slow.transform, 0, -110, 620, 92);
            var close = MakeButton(card.transform, "Close", "BACK", font, 34, Player1, Cavity);
            Place((RectTransform)close.transform, 0, -250, 320, 92);

            var settings = card.AddComponent<SettingsCard>();
            Wire(settings, ("sound", sound), ("reduceMotion", motion), ("slowPlayback", slow), ("close", close));
            card.SetActive(false);
            return card;
        }

        static GameObject BuildMenu(Transform canvas, Font font, ComputerLevels levels, out MenuButton[] buttons)
        {
            var menu = new GameObject("Menu", typeof(RectTransform));
            menu.transform.SetParent(canvas, false);
            Stretch((RectTransform)menu.transform, Vector2.zero, Vector2.one);
            MakeImage(menu.transform, "Background", LoadSprite("background.png")).raycastTarget = true;

            var logo = MakeImage(menu.transform, "Logo", LoadSprite("logo.png"));
            logo.preserveAspect = true;
            Place(logo.rectTransform, 0, 395, 1100, 190);

            var list = new System.Collections.Generic.List<MenuButton>();

            var duel = MakeMenuCard(menu.transform, font, -570, "TWO PLAYERS", "Same screen, take turns", Player1, 0f);
            list.Add(MakeMenuButton(duel, "PLAY", font, 40, Player1, Cavity, 0, -195, 380, 96, new PlaySetup { mode = GameMode.TwoPlayers }));

            var computer = MakeMenuCard(menu.transform, font, 0, "VS COMPUTER", "Pick how hard it plays", Player2, 0.23f);
            for (int i = 0; i < levels.levels.Length; i++)
                list.Add(MakeMenuButton(computer, levels.levels[i].label, font, 26, Player2, Cavity, (i - 1) * 156, -195, 146, 96,
                    new PlaySetup { mode = GameMode.Computer, computerLevel = i }));

            var practice = MakeMenuCard(menu.transform, font, 570, "PRACTICE", "Just you and the pattern", Twist, 0.46f);
            var kinds = new[]
            {
                (PracticeKind.Classic, "CLASSIC"), (PracticeKind.Endless, "ENDLESS"), (PracticeKind.Twists, "TWISTS"),
                (PracticeKind.SightOnly, "SIGHT ONLY"), (PracticeKind.SoundOnly, "SOUND ONLY"),
            };
            for (int i = 0; i < kinds.Length; i++)
            {
                bool topRow = i < 3;
                float x = topRow ? (i - 1) * 156 : (i - 3.5f) * 196;
                list.Add(MakeMenuButton(practice, kinds[i].Item2, font, 20, Raised, Color.white, x, topRow ? -160 : -232, topRow ? 146 : 186, 62,
                    new PlaySetup { mode = GameMode.Practice, practice = kinds[i].Item1 }));
            }

            buttons = list.ToArray();
            return menu;
        }

        // One big choice on the menu: a little live board, a name and a line saying what it is.
        static Transform MakeMenuCard(Transform menu, Font font, float x, string title, string line, Color color, float attractDelay)
        {
            var card = MakeSliced(menu, title, "panel.png", 40, Panel);
            Place(card.rectTransform, x, -40, 530, 600);
            Stretch(MakeSliced(card.transform, "Line", "panel_line.png", 40, color).rectTransform, Vector2.zero, Vector2.one);

            var board = new GameObject("Board", typeof(RectTransform));
            board.transform.SetParent(card.transform, false);
            Place((RectTransform)board.transform, 0, 130, 210, 210);
            var shape = LoadSprite("pad.png");
            var pads = new Image[Pads.All.Length];
            for (int i = 0; i < pads.Length; i++)
            {
                pads[i] = MakeImage(board.transform, Pads.All[i].ToString(), shape);
                Place(pads[i].rectTransform, i % 2 == 0 ? -56 : 56, i < 2 ? 56 : -56, 104, 104);
                pads[i].color = buildFeel.looks[i].rest;
                var symbol = MakeImage(pads[i].transform, "Symbol", LoadSprite($"symbol_{Pads.All[i].ToString().ToLowerInvariant()}.png"));
                symbol.color = buildFeel.symbolColor;
                Place(symbol.rectTransform, 0, 0, 52, 52);
            }
            var attract = board.AddComponent<AttractPads>();
            var so = new SerializedObject(attract);
            so.FindProperty("startDelay").floatValue = attractDelay;
            so.ApplyModifiedPropertiesWithoutUndo();
            Wire(attract, ("feel", buildFeel));
            WireList(attract, "pads", pads);

            Place(MakeText(card.transform, "Title", title, font, 44, Color.white).rectTransform, 0, -30, 500, 60);
            Place(MakeText(card.transform, "Line", line, font, 26, TextSecondary).rectTransform, 0, -82, 500, 40);
            return card.transform;
        }

        static MenuButton MakeMenuButton(Transform parent, string label, Font font, int size, Color color, Color textColor,
            float x, float y, float width, float height, PlaySetup setup)
        {
            var button = MakeButton(parent, label, label, font, size, color, textColor);
            Place((RectTransform)button.transform, x, y, width, height);
            var choice = button.gameObject.AddComponent<MenuButton>();
            var so = new SerializedObject(choice);
            so.FindProperty("setup.mode").enumValueIndex = (int)setup.mode;
            so.FindProperty("setup.computerLevel").intValue = setup.computerLevel;
            so.FindProperty("setup.practice").enumValueIndex = (int)setup.practice;
            so.ApplyModifiedPropertiesWithoutUndo();
            return choice;
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
            Wire(image.gameObject.AddComponent<HoverPop>(), ("feel", buildFeel));
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

            var halo = MakeImage(go.transform, "Halo", LoadSprite("glow.png"));
            var depth = MakeImage(go.transform, "Depth", shapeSprite);
            var face = new GameObject("Face", typeof(RectTransform));
            face.transform.SetParent(go.transform, false);
            Stretch((RectTransform)face.transform, Vector2.zero, Vector2.one);
            var shape = MakeImage(face.transform, "Shape", shapeSprite);
            var gloss = MakeImage(face.transform, "Gloss", LoadSprite("pad_gloss.png"));
            gloss.rectTransform.anchorMin = gloss.rectTransform.anchorMax = gloss.rectTransform.pivot = new Vector2(0.5f, 1f);
            gloss.rectTransform.sizeDelta = new Vector2(feel.padSize.x * 0.82f, feel.padSize.y * 0.3f);
            var rim = MakeImage(face.transform, "Rim", LoadSprite("pad_rim.png"));
            var ripple = MakeImage(face.transform, "Ripple", LoadSprite("pad_rim.png"));
            ripple.transform.SetAsFirstSibling();
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
            so.FindProperty("halo").objectReferenceValue = halo;
            so.FindProperty("ripple").objectReferenceValue = ripple;
            so.FindProperty("symbol").objectReferenceValue = symbol;
            so.FindProperty("tone").objectReferenceValue = tone;
            so.ApplyModifiedPropertiesWithoutUndo();
            return pad;
        }

        static Image BuildBanner(Transform canvas, Font font, out Text text)
        {
            var line = MakeSliced(canvas, "Banner", "pill.png", 41, Player1);
            Place(line.rectTransform, 0, 456, 980, 84);
            var fill = MakeSliced(line.transform, "Fill", "pill.png", 41, Raised);
            fill.rectTransform.offsetMin = Vector2.one * 3;
            fill.rectTransform.offsetMax = Vector2.one * -3;
            text = MakeText(line.transform, "Text", "", font, 36, Color.white);
            Stretch(text.rectTransform, Vector2.zero, Vector2.one);
            return line;
        }

        static PlayerPanel BuildPlayerPanel(Transform canvas, Font font, float x, string number, string label, RulesConfig rules)
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
            var numberText = MakeText(badge.transform, "Number", number, font, 54, color);
            Stretch(numberText.rectTransform, Vector2.zero, Vector2.one);

            var nameText = MakeText(panel.transform, "Name", label, font, 34, color);
            Place(nameText.rectTransform, 0, 120, 360, 50);
            var pips = new Image[rules.ralliesToWin];
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

            var charge = new Image[rules.chargeToEarnTwist];
            for (int i = 0; i < charge.Length; i++)
            {
                charge[i] = MakeImage(panel.transform, "Charge", LoadSprite("disc.png"));
                charge[i].color = Line;
                Place(charge[i].rectTransform, (i - (charge.Length - 1) / 2f) * 30, -60, 18, 18);
            }

            var card = MakeSliced(panel.transform, "TwistCard", "panel.png", 40, Cavity);
            card.raycastTarget = true;
            Place(card.rectTransform, 0, -160, 320, 120);
            var cardButton = card.gameObject.AddComponent<Button>();
            cardButton.interactable = false;
            var cardLine = MakeSliced(card.transform, "Line", "panel_line.png", 40, Twist);
            Stretch(cardLine.rectTransform, Vector2.zero, Vector2.one);
            var cardText = MakeText(card.transform, "Text", "NO TWIST", font, 28, TwistText);
            cardText.raycastTarget = false;
            Stretch(cardText.rectTransform, Vector2.zero, Vector2.one);

            var keep = MakeButton(panel.transform, "Keep", "KEEP IT", font, 24, Raised, TextSecondary);
            Place((RectTransform)keep.transform, 0, -280, 200, 56);
            keep.gameObject.SetActive(false);

            var player = panel.gameObject.AddComponent<PlayerPanel>();
            Wire(player, ("outline", line), ("yourTurn", turn.gameObject), ("twistCard", cardButton), ("twistLabel", cardText), ("keep", keep), ("nameLabel", nameText), ("numberLabel", numberText));
            WireList(player, "pips", pips);
            WireList(player, "chargePips", charge);
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
            if (importer.textureType != TextureImporterType.Sprite || importer.spriteImportMode != SpriteImportMode.Single || importer.spriteBorder != Vector4.one * border)
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
