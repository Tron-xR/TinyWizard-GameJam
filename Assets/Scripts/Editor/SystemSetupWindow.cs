using UnityEngine;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine.UI;

public class SystemSetupWindow : EditorWindow
{
    [MenuItem("Tiny Wizard/System Setup")]
    public static void ShowWindow()
    {
        GetWindow<SystemSetupWindow>("System Setup");
    }

    private void OnGUI()
    {
        GUILayout.Label("New System Setup", EditorStyles.boldLabel);
        GUILayout.Space(10);

        if (GUILayout.Button("1. Create ManaUI on Canvas", GUILayout.Height(30)))
        {
            CreateManaUI();
        }

        if (GUILayout.Button("2. Create Pause Canvas", GUILayout.Height(30)))
        {
            CreatePauseCanvas();
        }

        if (GUILayout.Button("3. Wire PauseManager", GUILayout.Height(30)))
        {
            WirePauseManager();
        }

        GUILayout.Space(5);

        if (GUILayout.Button("4. Fix PlayerHealth Renderer", GUILayout.Height(30)))
        {
            FixPlayerHealthRenderer();
        }

        GUILayout.Space(10);
        if (GUILayout.Button("Do All Steps", GUILayout.Height(40)))
        {
            CreateManaUI();
            CreatePauseCanvas();
            WirePauseManager();
            FixPlayerHealthRenderer();
            EditorUtility.DisplayDialog("Done", "All systems wired up!\n- ManaUI added to Canvas\n- Pause Canvas created\n- PauseManager wired\n- PlayerHealth renderer set", "OK");
        }
    }

    private void CreateManaUI()
    {
        Canvas canvas = FindFirstObjectByType<Canvas>();
        if (canvas == null)
        {
            EditorUtility.DisplayDialog("Error", "No Canvas found in scene.", "OK");
            return;
        }

        ManaUI existing = FindFirstObjectByType<ManaUI>();
        if (existing != null)
        {
            EditorUtility.DisplayDialog("Already Exists", "ManaUI already exists in scene. Delete the duplicates first, or just re-run the Wire step.", "OK");
            return;
        }

        GameObject manaGO = new GameObject("ManaUI");
        manaGO.transform.SetParent(canvas.transform, false);
        RectTransform rt = manaGO.AddComponent<RectTransform>();
        rt.anchorMin = new Vector2(0.5f, 1);
        rt.anchorMax = new Vector2(0.5f, 1);
        rt.pivot = new Vector2(0.5f, 1);
        rt.anchoredPosition = new Vector2(0, -80);
        rt.sizeDelta = new Vector2(300, 30);

        ManaUI manaUI = manaGO.AddComponent<ManaUI>();

        GameObject fillGO = new GameObject("ManaBar_Fill");
        fillGO.transform.SetParent(manaGO.transform, false);
        RectTransform fillRT = fillGO.AddComponent<RectTransform>();
        fillRT.anchorMin = Vector2.zero;
        fillRT.anchorMax = Vector2.one;
        fillRT.offsetMin = Vector2.zero;
        fillRT.offsetMax = Vector2.zero;
        Image fillImage = fillGO.AddComponent<Image>();
        fillImage.color = Color.blue;
        fillImage.type = Image.Type.Filled;
        fillImage.fillMethod = Image.FillMethod.Horizontal;
        manaUI.manaFill = fillImage;

        GameObject textGO = new GameObject("ManaText");
        textGO.transform.SetParent(manaGO.transform, false);
        RectTransform textRT = textGO.AddComponent<RectTransform>();
        textRT.anchorMin = Vector2.zero;
        textRT.anchorMax = Vector2.one;
        textRT.offsetMin = Vector2.zero;
        textRT.offsetMax = Vector2.zero;
        Text manaText = textGO.AddComponent<Text>();
        manaText.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
        manaText.fontSize = 18;
        manaText.alignment = TextAnchor.MiddleRight;
        manaText.color = Color.white;
        manaText.text = "100 / 100";
        textGO.AddComponent<CanvasRenderer>();
        manaUI.manaText = manaText;

        EditorSceneManager.MarkSceneDirty(EditorSceneManager.GetActiveScene());
        Debug.Log("ManaUI created on Canvas.");
    }

    private void CreatePauseCanvas()
    {
        GameObject existing = GameObject.Find("PauseCanvas");
        if (existing != null)
        {
            PauseManager existingPm = FindFirstObjectByType<PauseManager>();
            if (existingPm != null)
            {
                existingPm.pauseCanvas = existing;
                EditorUtility.SetDirty(existingPm);
            }
            EditorUtility.DisplayDialog("Already Exists", "PauseCanvas already exists. Just re-wired it to PauseManager.", "OK");
            return;
        }

        GameObject pauseCanvasGO = new GameObject("PauseCanvas");
        Canvas canvas = pauseCanvasGO.AddComponent<Canvas>();
        canvas.renderMode = RenderMode.ScreenSpaceOverlay;
        canvas.sortingOrder = 100;
        pauseCanvasGO.AddComponent<CanvasScaler>();
        pauseCanvasGO.AddComponent<GraphicRaycaster>();
        pauseCanvasGO.SetActive(false);

        GameObject overlayGO = new GameObject("Overlay");
        overlayGO.transform.SetParent(pauseCanvasGO.transform, false);
        RectTransform overlayRT = overlayGO.AddComponent<RectTransform>();
        overlayRT.anchorMin = Vector2.zero;
        overlayRT.anchorMax = Vector2.one;
        overlayRT.sizeDelta = Vector2.zero;
        Image overlayImage = overlayGO.AddComponent<Image>();
        overlayImage.color = new Color(0, 0, 0, 0.7f);

        GameObject textGO = new GameObject("PauseText");
        textGO.transform.SetParent(pauseCanvasGO.transform, false);
        RectTransform textRT = textGO.AddComponent<RectTransform>();
        textRT.anchorMin = new Vector2(0.5f, 0.5f);
        textRT.anchorMax = new Vector2(0.5f, 0.5f);
        textRT.pivot = new Vector2(0.5f, 0.5f);
        textRT.anchoredPosition = Vector2.zero;
        textRT.sizeDelta = new Vector2(400, 100);
        Text pauseText = textGO.AddComponent<Text>();
        pauseText.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
        pauseText.fontSize = 48;
        pauseText.alignment = TextAnchor.MiddleCenter;
        pauseText.color = Color.white;
        pauseText.text = "PAUSED";
        textGO.AddComponent<CanvasRenderer>();

        PauseManager pm = FindFirstObjectByType<PauseManager>();
        if (pm != null)
            pm.pauseCanvas = pauseCanvasGO;

        EditorSceneManager.MarkSceneDirty(EditorSceneManager.GetActiveScene());
        Debug.Log("Pause Canvas created.");
    }

    private void WirePauseManager()
    {
        PauseManager pm = FindFirstObjectByType<PauseManager>();
        if (pm == null)
        {
            EditorUtility.DisplayDialog("Error", "No PauseManager found. Run GameManager setup first.", "OK");
            return;
        }

        GameObject pauseCanvas = GameObject.Find("PauseCanvas");
        if (pauseCanvas != null)
        {
            pm.pauseCanvas = pauseCanvas;
            EditorUtility.SetDirty(pm);
        }

        EditorSceneManager.MarkSceneDirty(EditorSceneManager.GetActiveScene());
        Debug.Log("PauseManager wired.");
    }

    private void FixPlayerHealthRenderer()
    {
        PlayerHealth health = FindFirstObjectByType<PlayerHealth>();
        if (health == null)
        {
            EditorUtility.DisplayDialog("Error", "No PlayerHealth found.", "OK");
            return;
        }

        GameObject player = health.gameObject;
        Renderer renderer = player.GetComponentInChildren<Renderer>();
        if (renderer == null)
        {
            Debug.LogWarning("PlayerHealth: no Renderer found under " + player.name);
            return;
        }

        SerializedObject so = new SerializedObject(health);
        so.FindProperty("playerRenderer").objectReferenceValue = renderer;
        so.ApplyModifiedProperties();
        EditorUtility.SetDirty(health);

        EditorSceneManager.MarkSceneDirty(EditorSceneManager.GetActiveScene());
        Debug.Log("PlayerHealth renderer set to " + renderer.name);
    }
}
