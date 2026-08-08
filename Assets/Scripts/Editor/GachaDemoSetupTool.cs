using GachaGame.Core;
using GachaGame.Data;
using GachaGame.Gacha;
using GachaGame.Managers;
using GachaGame.UI;
using UnityEditor;
using UnityEditor.Events;
using UnityEngine;
using UnityEngine.UI;
using Object = UnityEngine.Object;

namespace GachaGame.Editor
{
    // One-shot scene wiring for a working gacha demo: creates sample BannerData/CharacterBaseData
    // assets, a GachaManager+BannerController in the scene, registers GachaManager with the
    // AppBootstrapper's service list, grants demo gems, and (re)creates the "Evocazione" button
    // bound to GachaUI.PullOnce() - all through SerializedObject/AssetDatabase so the wiring is
    // deterministic instead of manual Inspector drag-and-drop.
    public static class GachaDemoSetupTool
    {
        private const string DataFolder = "Assets/Data/Gacha/Demo";
        private const int DemoStartingGems = 1000;
        private const int DemoCostPerPull = 100;

        [MenuItem("GachaGame/Setup/Create Gacha Demo Setup")]
        public static void CreateDemoSetup()
        {
            if (Application.isPlaying)
            {
                Debug.LogError("Exit Play Mode before running the Gacha demo setup - it edits and saves the scene and assets.");
                return;
            }

            var canvas = GameObject.Find("UI_Canvas");
            if (canvas == null)
            {
                Debug.LogError("Could not find a 'UI_Canvas' GameObject in the open scene. Open the scene that has it, then re-run this tool.");
                return;
            }

            EnsureFolder(DataFolder);

            var fiveStar = GetOrCreateCharacter("demo_5star", "Demo Five-Star", Rarity.FiveStars);
            var fourStar = GetOrCreateCharacter("demo_4star", "Demo Four-Star", Rarity.FourStars);
            var threeStar = GetOrCreateCharacter("demo_3star", "Demo Three-Star", Rarity.ThreeStars);
            var banner = GetOrCreateBanner(fiveStar.id);

            var bannerController = GetOrCreateComponent<BannerController>("GachaManager");
            var gachaManager = GetOrCreateComponent<GachaManager>("GachaManager");

            AssignBannerControllerData(bannerController, banner, new[] { fiveStar, fourStar, threeStar });
            AssignField(gachaManager, "bannerController", bannerController);

            RegisterWithBootstrapper(gachaManager);
            GrantDemoGems();

            var gachaPanel = GetOrCreatePanel(canvas.transform, "GachaPanel");
            var gachaUI = gachaPanel.GetComponent<GachaUI>();
            if (gachaUI == null) gachaUI = gachaPanel.AddComponent<GachaUI>();

            var resultText = GetOrCreateLabel(gachaPanel.transform, "ResultText", new Vector2(0, 80));
            var pityText = GetOrCreateLabel(gachaPanel.transform, "PityText", new Vector2(0, 40));
            AssignField(gachaUI, "resultText", resultText);
            AssignField(gachaUI, "pityText", pityText);

            RecreateEvocazioneButton(gachaPanel.transform, gachaUI);

            var mainMenuUI = Object.FindAnyObjectByType<MainMenuUI>();
            if (mainMenuUI != null)
                AssignField(mainMenuUI, "gachaPanel", gachaPanel);

            EditorUtility.SetDirty(gachaManager);
            EditorUtility.SetDirty(bannerController);
            EditorUtility.SetDirty(gachaUI);
            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();

            Debug.Log("Gacha demo setup complete: GachaManager/BannerController wired to a demo banner + 3 characters, registered with AppBootstrapper, and the 'Evocazione' button was recreated bound to GachaUI.PullOnce(). Remember to save the scene (Ctrl/Cmd+S).");
        }

        private static CharacterBaseData GetOrCreateCharacter(string id, string characterName, Rarity rarity)
        {
            string path = $"{DataFolder}/{id}.asset";
            var existing = AssetDatabase.LoadAssetAtPath<CharacterBaseData>(path);
            if (existing != null) return existing;

            var data = ScriptableObject.CreateInstance<CharacterBaseData>();
            data.id = id;
            data.characterName = characterName;
            data.rarity = rarity;
            data.element = ElementType.Light;
            AssetDatabase.CreateAsset(data, path);
            return data;
        }

        private static BannerData GetOrCreateBanner(string featuredCharacterId)
        {
            string path = $"{DataFolder}/demo_banner.asset";
            var existing = AssetDatabase.LoadAssetAtPath<BannerData>(path);
            if (existing != null) return existing;

            var banner = ScriptableObject.CreateInstance<BannerData>();
            banner.bannerId = "demo_banner";
            banner.bannerName = "Demo Banner";
            banner.featuredCharacterId = featuredCharacterId;
            banner.startDate = string.Empty;
            banner.endDate = string.Empty;
            banner.costPerPull = DemoCostPerPull;
            AssetDatabase.CreateAsset(banner, path);
            return banner;
        }

        private static T GetOrCreateComponent<T>(string gameObjectName) where T : Component
        {
            var go = GameObject.Find(gameObjectName);
            if (go == null) go = new GameObject(gameObjectName);

            var component = go.GetComponent<T>();
            if (component == null) component = go.AddComponent<T>();
            return component;
        }

        private static void AssignBannerControllerData(BannerController controller, BannerData banner, CharacterBaseData[] characters)
        {
            var so = new SerializedObject(controller);

            var bannersProp = so.FindProperty("banners");
            bannersProp.ClearArray();
            bannersProp.InsertArrayElementAtIndex(0);
            bannersProp.GetArrayElementAtIndex(0).objectReferenceValue = banner;

            var poolProp = so.FindProperty("characterPool");
            poolProp.ClearArray();
            for (int i = 0; i < characters.Length; i++)
            {
                poolProp.InsertArrayElementAtIndex(i);
                poolProp.GetArrayElementAtIndex(i).objectReferenceValue = characters[i];
            }

            so.ApplyModifiedProperties();
        }

        private static void AssignField(Object target, string fieldName, Object value)
        {
            var so = new SerializedObject(target);
            var prop = so.FindProperty(fieldName);
            if (prop == null)
            {
                Debug.LogError($"Field '{fieldName}' not found on {target.GetType().Name}.");
                return;
            }

            prop.objectReferenceValue = value;
            so.ApplyModifiedProperties();
        }

        private static void RegisterWithBootstrapper(GachaManager gachaManager)
        {
            var bootstrapper = Object.FindAnyObjectByType<AppBootstrapper>();
            if (bootstrapper == null)
            {
                Debug.LogWarning("No AppBootstrapper found in the scene - GachaManager was not registered with ServiceLocator, so GachaUI won't resolve it at runtime. Add GachaManager to AppBootstrapper's Service Behaviours list manually.");
                return;
            }

            var so = new SerializedObject(bootstrapper);
            var listProp = so.FindProperty("serviceBehaviours");

            for (int i = 0; i < listProp.arraySize; i++)
            {
                if (listProp.GetArrayElementAtIndex(i).objectReferenceValue == gachaManager)
                    return;
            }

            int index = listProp.arraySize;
            listProp.InsertArrayElementAtIndex(index);
            listProp.GetArrayElementAtIndex(index).objectReferenceValue = gachaManager;
            so.ApplyModifiedProperties();
        }

        private static void GrantDemoGems()
        {
            var currencyManager = Object.FindAnyObjectByType<CurrencyManager>();
            if (currencyManager == null)
            {
                Debug.LogWarning("No CurrencyManager found in the scene - pulls will fail with 0 gems until one exists and is registered with AppBootstrapper.");
                return;
            }

            currencyManager.AddCurrency(CurrencyType.Gems, DemoStartingGems);
            EditorUtility.SetDirty(currencyManager);
        }

        private static GameObject GetOrCreatePanel(Transform canvasTransform, string name)
        {
            var existing = canvasTransform.Find(name);
            if (existing != null) return existing.gameObject;

            var panel = new GameObject(name, typeof(RectTransform));
            panel.transform.SetParent(canvasTransform, false);

            var rect = panel.GetComponent<RectTransform>();
            rect.anchorMin = Vector2.zero;
            rect.anchorMax = Vector2.one;
            rect.offsetMin = Vector2.zero;
            rect.offsetMax = Vector2.zero;
            return panel;
        }

        private static Text GetOrCreateLabel(Transform parent, string name, Vector2 anchoredPosition)
        {
            var existing = parent.Find(name);
            GameObject go;
            if (existing != null)
            {
                go = existing.gameObject;
            }
            else
            {
                go = new GameObject(name, typeof(RectTransform), typeof(CanvasRenderer), typeof(Text));
                go.transform.SetParent(parent, false);
            }

            var rect = go.GetComponent<RectTransform>();
            rect.anchorMin = new Vector2(0.5f, 0.5f);
            rect.anchorMax = new Vector2(0.5f, 0.5f);
            rect.sizeDelta = new Vector2(400, 30);
            rect.anchoredPosition = anchoredPosition;

            var text = go.GetComponent<Text>();
            text.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
            text.alignment = TextAnchor.MiddleCenter;
            text.color = Color.white;
            text.fontSize = 18;
            return text;
        }

        private static void RecreateEvocazioneButton(Transform parent, GachaUI gachaUI)
        {
            var existing = parent.Find("EvocazioneButton");
            if (existing != null) Object.DestroyImmediate(existing.gameObject);

            var buttonGo = new GameObject("EvocazioneButton", typeof(RectTransform), typeof(CanvasRenderer), typeof(Image), typeof(Button));
            buttonGo.transform.SetParent(parent, false);

            var rect = buttonGo.GetComponent<RectTransform>();
            rect.anchorMin = new Vector2(0.5f, 0.5f);
            rect.anchorMax = new Vector2(0.5f, 0.5f);
            rect.sizeDelta = new Vector2(180, 40);
            rect.anchoredPosition = Vector2.zero;

            var image = buttonGo.GetComponent<Image>();
            image.color = new Color(0.85f, 0.85f, 0.85f);

            AddStretchedLabel(buttonGo.transform, "Evocazione");

            var button = buttonGo.GetComponent<Button>();
            UnityEventTools.AddPersistentListener(button.onClick, gachaUI.PullOnce);
        }

        private static void AddStretchedLabel(Transform parent, string text)
        {
            var go = new GameObject("Label", typeof(RectTransform), typeof(CanvasRenderer), typeof(Text));
            go.transform.SetParent(parent, false);

            var rect = go.GetComponent<RectTransform>();
            rect.anchorMin = Vector2.zero;
            rect.anchorMax = Vector2.one;
            rect.offsetMin = Vector2.zero;
            rect.offsetMax = Vector2.zero;

            var label = go.GetComponent<Text>();
            label.text = text;
            label.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
            label.alignment = TextAnchor.MiddleCenter;
            label.color = Color.black;
            label.fontSize = 18;
        }

        private static void EnsureFolder(string path)
        {
            if (AssetDatabase.IsValidFolder(path)) return;

            string[] parts = path.Split('/');
            string current = parts[0];
            for (int i = 1; i < parts.Length; i++)
            {
                string next = $"{current}/{parts[i]}";
                if (!AssetDatabase.IsValidFolder(next))
                    AssetDatabase.CreateFolder(current, parts[i]);
                current = next;
            }
        }
    }
}
