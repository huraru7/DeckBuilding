using System.Collections.Generic;
using System.IO;
using DeckBuilding.Controllers;
using DeckBuilding.Data;
using DeckBuilding.UI;
using TMPro;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem.UI;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

namespace DeckBuilding.EditorTools
{
    /// <summary>
    /// One-time editor tool that generates the character master data, shared card prefab,
    /// and the DeckBuilding scene hierarchy so the deck-building screen can be reviewed/played
    /// without hand-authoring Unity scene/prefab YAML.
    /// </summary>
    public static class DeckBuildingSceneSetup
    {
        private const string CharacterDataFolder = "Assets/Data/Characters";
        private const string PrefabFolder = "Assets/Prefabs/UI";
        private const string SceneFolder = "Assets/Scenes";
        private const string ScenePath = SceneFolder + "/DeckBuilding.unity";
        private const string CardPrefabPath = PrefabFolder + "/CharacterCardView.prefab";

        private static readonly (string name, int cost, int hp)[] CharacterTable =
        {
            ("Warrior", 2, 81),
            ("Mage", 4, 40),
            ("Archer", 2, 20),
            ("Tank", 8, 100),
            ("Scout", 1, 10),
            ("Cleric", 3, 55),
            ("Knight", 6, 90),
            ("Assassin", 2, 30),
            ("Paladin", 5, 75),
            ("Berserker", 7, 60),
        };

        [MenuItem("Tools/Deck Building/Build Scene And Assets")]
        public static void BuildAll()
        {
            EnsureFolders();
            CharacterData[] characters = CreateCharacterDataAssets();
            CharacterCardView cardPrefab = CreateCardPrefab();
            BuildScene(characters, cardPrefab);
            Debug.Log("Deck Building scene and assets generated at " + ScenePath);
        }

        private static void EnsureFolders()
        {
            CreateFolderRecursive("Assets/Data");
            CreateFolderRecursive(CharacterDataFolder);
            CreateFolderRecursive("Assets/Prefabs");
            CreateFolderRecursive(PrefabFolder);
            CreateFolderRecursive(SceneFolder);
        }

        private static void CreateFolderRecursive(string path)
        {
            if (AssetDatabase.IsValidFolder(path)) return;
            string parent = Path.GetDirectoryName(path)?.Replace("\\", "/");
            string leaf = Path.GetFileName(path);
            if (!string.IsNullOrEmpty(parent) && !AssetDatabase.IsValidFolder(parent))
            {
                CreateFolderRecursive(parent);
            }
            AssetDatabase.CreateFolder(parent, leaf);
        }

        private static CharacterData[] CreateCharacterDataAssets()
        {
            var results = new List<CharacterData>();

            for (int i = 0; i < CharacterTable.Length; i++)
            {
                (string name, int cost, int hp) = CharacterTable[i];
                string assetPath = $"{CharacterDataFolder}/Char_{i + 1:00}_{name}.asset";

                CharacterData data = AssetDatabase.LoadAssetAtPath<CharacterData>(assetPath);
                if (data == null)
                {
                    data = ScriptableObject.CreateInstance<CharacterData>();
                    AssetDatabase.CreateAsset(data, assetPath);
                }

                var so = new SerializedObject(data);
                so.FindProperty("id").intValue = i + 1;
                so.FindProperty("characterName").stringValue = name;
                so.FindProperty("cost").intValue = cost;
                so.FindProperty("hp").intValue = hp;
                so.ApplyModifiedPropertiesWithoutUndo();

                results.Add(data);
            }

            AssetDatabase.SaveAssets();
            return results.ToArray();
        }

        private static CharacterCardView CreateCardPrefab()
        {
            GameObject existing = AssetDatabase.LoadAssetAtPath<GameObject>(CardPrefabPath);
            if (existing != null)
            {
                return existing.GetComponent<CharacterCardView>();
            }

            var root = new GameObject("CharacterCardView", typeof(RectTransform), typeof(CanvasGroup), typeof(Image));
            var rootRect = (RectTransform)root.transform;
            rootRect.sizeDelta = new Vector2(160, 200);
            root.GetComponent<Image>().color = new Color(1f, 1f, 1f, 0.95f);

            var portraitGO = new GameObject("Portrait", typeof(RectTransform), typeof(Image));
            portraitGO.transform.SetParent(root.transform, false);
            SetAnchors((RectTransform)portraitGO.transform, new Vector2(0.1f, 0.42f), new Vector2(0.9f, 0.95f));

            TMP_Text nameText = CreateText(root.transform, "NameText", new Vector2(0f, 0.30f), new Vector2(1f, 0.42f));
            TMP_Text costText = CreateText(root.transform, "CostText", new Vector2(0f, 0.15f), new Vector2(1f, 0.30f));
            TMP_Text hpText = CreateText(root.transform, "HpText", new Vector2(0f, 0f), new Vector2(1f, 0.15f));
            TMP_Text quantityText = CreateText(root.transform, "QuantityText", new Vector2(0.6f, 0.85f), new Vector2(1f, 1f));

            var cardView = root.AddComponent<CharacterCardView>();
            root.AddComponent<DraggableCardView>();

            var cardSO = new SerializedObject(cardView);
            cardSO.FindProperty("portraitImage").objectReferenceValue = portraitGO.GetComponent<Image>();
            cardSO.FindProperty("nameText").objectReferenceValue = nameText;
            cardSO.FindProperty("costText").objectReferenceValue = costText;
            cardSO.FindProperty("hpText").objectReferenceValue = hpText;
            cardSO.FindProperty("quantityText").objectReferenceValue = quantityText;
            cardSO.ApplyModifiedPropertiesWithoutUndo();

            PrefabUtility.SaveAsPrefabAsset(root, CardPrefabPath);
            Object.DestroyImmediate(root);

            GameObject prefabAsset = AssetDatabase.LoadAssetAtPath<GameObject>(CardPrefabPath);
            return prefabAsset.GetComponent<CharacterCardView>();
        }

        private static void BuildScene(CharacterData[] characters, CharacterCardView cardPrefab)
        {
            EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);

            // Re-resolve asset references fresh after the scene switch: EditorSceneManager.NewScene
            // can invalidate the managed handles obtained before the switch, which would otherwise
            // silently serialize as null (fileID: 0) references below.
            characters = ReloadCharacterDataAssets();
            cardPrefab = ReloadCardPrefab();

            var canvasGO = new GameObject("Canvas", typeof(RectTransform), typeof(Canvas), typeof(CanvasScaler), typeof(GraphicRaycaster));
            var canvas = canvasGO.GetComponent<Canvas>();
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;
            var scaler = canvasGO.GetComponent<CanvasScaler>();
            scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            scaler.referenceResolution = new Vector2(1080, 1920);

            new GameObject("EventSystem", typeof(EventSystem), typeof(InputSystemUIInputModule));

            // ---- Deck panel (top half) ----
            GameObject deckPanel = CreatePanel(canvasGO.transform, "DeckPanel", new Vector2(0f, 0.55f), new Vector2(1f, 1f));

            var deckSlots = new DeckSlotDropZone[GameConstants.MaxDeckSize];
            float slotWidth = 1f / GameConstants.MaxDeckSize;
            for (int i = 0; i < GameConstants.MaxDeckSize; i++)
            {
                GameObject slotGO = CreatePanel(
                    deckPanel.transform, $"DeckSlot_{i}",
                    new Vector2(slotWidth * i + 0.02f, 0.55f), new Vector2(slotWidth * (i + 1) - 0.02f, 0.95f));
                slotGO.AddComponent<Image>().color = new Color(0.85f, 0.85f, 0.85f, 1f);

                var containerGO = new GameObject("CardContainer", typeof(RectTransform));
                containerGO.transform.SetParent(slotGO.transform, false);
                SetAnchors((RectTransform)containerGO.transform, Vector2.zero, Vector2.one);

                var dropZone = slotGO.AddComponent<DeckSlotDropZone>();
                var dropZoneSO = new SerializedObject(dropZone);
                dropZoneSO.FindProperty("slotIndex").intValue = i;
                dropZoneSO.FindProperty("cardContainer").objectReferenceValue = containerGO.transform;
                dropZoneSO.ApplyModifiedPropertiesWithoutUndo();

                deckSlots[i] = dropZone;
            }

            TMP_Text totalCostText = CreateText(deckPanel.transform, "TotalCostText", new Vector2(0.02f, 0.28f), new Vector2(0.6f, 0.5f));
            totalCostText.alignment = TextAlignmentOptions.Left;
            totalCostText.fontSize = 28;
            totalCostText.text = "Total cost:0";

            GameObject sortButtonGO = CreateButton(deckPanel.transform, "SortButton", new Vector2(0.65f, 0.28f), new Vector2(0.98f, 0.5f), out TMP_Text sortLabel);
            sortLabel.text = "ソート\n入手順";

            GameObject decideButtonGO = CreateButton(deckPanel.transform, "DecideButton", new Vector2(0.3f, 0.02f), new Vector2(0.7f, 0.2f), out TMP_Text decideLabel);
            decideLabel.text = "決定";

            // ---- Owned list panel (bottom half) ----
            GameObject ownedPanel = CreatePanel(canvasGO.transform, "OwnedListPanel", new Vector2(0f, 0f), new Vector2(1f, 0.55f));
            ownedPanel.AddComponent<Image>().color = new Color(0.95f, 0.95f, 0.95f, 1f);

            var scrollViewGO = new GameObject("ScrollView", typeof(RectTransform), typeof(ScrollRect));
            scrollViewGO.transform.SetParent(ownedPanel.transform, false);
            SetAnchors((RectTransform)scrollViewGO.transform, Vector2.zero, Vector2.one);

            var viewportGO = new GameObject("Viewport", typeof(RectTransform), typeof(Image), typeof(Mask));
            viewportGO.transform.SetParent(scrollViewGO.transform, false);
            SetAnchors((RectTransform)viewportGO.transform, Vector2.zero, Vector2.one);
            viewportGO.GetComponent<Image>().color = Color.white;
            viewportGO.GetComponent<Mask>().showMaskGraphic = false;

            var contentGO = new GameObject("Content", typeof(RectTransform), typeof(VerticalLayoutGroup), typeof(ContentSizeFitter));
            contentGO.transform.SetParent(viewportGO.transform, false);
            var contentRect = (RectTransform)contentGO.transform;
            contentRect.anchorMin = new Vector2(0f, 1f);
            contentRect.anchorMax = new Vector2(1f, 1f);
            contentRect.pivot = new Vector2(0.5f, 1f);
            contentRect.offsetMin = Vector2.zero;
            contentRect.offsetMax = Vector2.zero;

            var layoutGroup = contentGO.GetComponent<VerticalLayoutGroup>();
            layoutGroup.childForceExpandWidth = true;
            layoutGroup.childForceExpandHeight = false;
            layoutGroup.spacing = 8f;
            layoutGroup.padding = new RectOffset(8, 8, 8, 8);
            contentGO.GetComponent<ContentSizeFitter>().verticalFit = ContentSizeFitter.FitMode.PreferredSize;

            var scrollRectComp = scrollViewGO.GetComponent<ScrollRect>();
            scrollRectComp.content = contentRect;
            scrollRectComp.viewport = (RectTransform)viewportGO.transform;
            scrollRectComp.horizontal = false;
            scrollRectComp.vertical = true;

            var ownedListView = ownedPanel.AddComponent<OwnedListView>();
            var ownedListViewSO = new SerializedObject(ownedListView);
            ownedListViewSO.FindProperty("cardPrefab").objectReferenceValue = cardPrefab;
            ownedListViewSO.FindProperty("contentContainer").objectReferenceValue = contentGO.transform;
            ownedListViewSO.ApplyModifiedPropertiesWithoutUndo();

            var ownedListDropZone = ownedPanel.AddComponent<OwnedListDropZone>();

            // ---- Error / message popup ----
            GameObject popupGO = CreatePanel(canvasGO.transform, "MessagePopup", new Vector2(0.15f, 0.4f), new Vector2(0.85f, 0.6f));
            popupGO.AddComponent<Image>().color = new Color(0f, 0f, 0f, 0.85f);

            TMP_Text messageText = CreateText(popupGO.transform, "MessageText", new Vector2(0.05f, 0.3f), new Vector2(0.95f, 0.9f));
            messageText.color = Color.white;
            messageText.fontSize = 22;

            GameObject closeButtonGO = CreateButton(popupGO.transform, "CloseButton", new Vector2(0.35f, 0.05f), new Vector2(0.65f, 0.25f), out TMP_Text closeLabel);
            closeLabel.text = "閉じる";

            var popup = popupGO.AddComponent<ErrorMessagePopup>();
            var popupSO = new SerializedObject(popup);
            popupSO.FindProperty("panelRoot").objectReferenceValue = popupGO;
            popupSO.FindProperty("messageText").objectReferenceValue = messageText;
            popupSO.FindProperty("closeButton").objectReferenceValue = closeButtonGO.GetComponent<Button>();
            popupSO.ApplyModifiedPropertiesWithoutUndo();

            // ---- Controller wiring ----
            var controllerGO = new GameObject("DeckBuildController");
            var controller = controllerGO.AddComponent<DeckBuildController>();
            var controllerSO = new SerializedObject(controller);

            var masterProp = controllerSO.FindProperty("allCharacterMaster");
            masterProp.arraySize = characters.Length;
            for (int i = 0; i < characters.Length; i++)
            {
                masterProp.GetArrayElementAtIndex(i).objectReferenceValue = characters[i];
            }

            controllerSO.FindProperty("ownedListView").objectReferenceValue = ownedListView;

            var slotsProp = controllerSO.FindProperty("deckSlots");
            slotsProp.arraySize = deckSlots.Length;
            for (int i = 0; i < deckSlots.Length; i++)
            {
                slotsProp.GetArrayElementAtIndex(i).objectReferenceValue = deckSlots[i];
            }

            controllerSO.FindProperty("slotCardPrefab").objectReferenceValue = cardPrefab;
            controllerSO.FindProperty("totalCostText").objectReferenceValue = totalCostText;
            controllerSO.FindProperty("normalCostColor").colorValue = Color.black;
            controllerSO.FindProperty("overLimitCostColor").colorValue = Color.red;
            controllerSO.FindProperty("sortButton").objectReferenceValue = sortButtonGO.GetComponent<Button>();
            controllerSO.FindProperty("sortButtonLabel").objectReferenceValue = sortLabel;
            controllerSO.FindProperty("decideButton").objectReferenceValue = decideButtonGO.GetComponent<Button>();
            controllerSO.FindProperty("messagePopup").objectReferenceValue = popup;
            controllerSO.ApplyModifiedPropertiesWithoutUndo();

            foreach (DeckSlotDropZone slot in deckSlots)
            {
                var slotSO = new SerializedObject(slot);
                slotSO.FindProperty("controller").objectReferenceValue = controller;
                slotSO.ApplyModifiedPropertiesWithoutUndo();
            }

            var ownedListDropZoneSO = new SerializedObject(ownedListDropZone);
            ownedListDropZoneSO.FindProperty("controller").objectReferenceValue = controller;
            ownedListDropZoneSO.ApplyModifiedPropertiesWithoutUndo();

            EditorSceneManager.SaveScene(SceneManager.GetActiveScene(), ScenePath);
        }

        private static CharacterData[] ReloadCharacterDataAssets()
        {
            string[] guids = AssetDatabase.FindAssets($"t:{nameof(CharacterData)}", new[] { CharacterDataFolder });
            var list = new List<CharacterData>();
            foreach (string guid in guids)
            {
                string path = AssetDatabase.GUIDToAssetPath(guid);
                list.Add(AssetDatabase.LoadAssetAtPath<CharacterData>(path));
            }
            list.Sort((a, b) => a.Id.CompareTo(b.Id));
            return list.ToArray();
        }

        private static CharacterCardView ReloadCardPrefab()
        {
            GameObject prefab = AssetDatabase.LoadAssetAtPath<GameObject>(CardPrefabPath);
            return prefab.GetComponent<CharacterCardView>();
        }

        private static GameObject CreatePanel(Transform parent, string name, Vector2 anchorMin, Vector2 anchorMax)
        {
            var go = new GameObject(name, typeof(RectTransform));
            go.transform.SetParent(parent, false);
            SetAnchors((RectTransform)go.transform, anchorMin, anchorMax);
            return go;
        }

        private static void SetAnchors(RectTransform rect, Vector2 anchorMin, Vector2 anchorMax)
        {
            rect.anchorMin = anchorMin;
            rect.anchorMax = anchorMax;
            rect.offsetMin = Vector2.zero;
            rect.offsetMax = Vector2.zero;
        }

        private static TMP_Text CreateText(Transform parent, string name, Vector2 anchorMin, Vector2 anchorMax)
        {
            var go = new GameObject(name, typeof(RectTransform));
            go.transform.SetParent(parent, false);
            SetAnchors((RectTransform)go.transform, anchorMin, anchorMax);

            var text = go.AddComponent<TextMeshProUGUI>();
            text.alignment = TextAlignmentOptions.Center;
            text.fontSize = 16;
            text.color = Color.black;
            return text;
        }

        private static GameObject CreateButton(Transform parent, string name, Vector2 anchorMin, Vector2 anchorMax, out TMP_Text label)
        {
            var go = new GameObject(name, typeof(RectTransform), typeof(Image), typeof(Button));
            go.transform.SetParent(parent, false);
            SetAnchors((RectTransform)go.transform, anchorMin, anchorMax);
            go.GetComponent<Image>().color = new Color(0.8f, 0.8f, 0.9f, 1f);

            label = CreateText(go.transform, "Label", Vector2.zero, Vector2.one);
            return go;
        }
    }
}
