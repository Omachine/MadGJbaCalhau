using UnityEngine;
using UnityEngine.UI;
using TMPro;

/// <summary>
/// Attach this to any GameObject in the scene (e.g. an empty "UIBuilder").
/// It creates the WorkTable and PingPong UI panels at runtime so you don't
/// need to wire anything manually in the Inspector.
/// </summary>
public class UIBuilder : MonoBehaviour
{
    [Header("Interactables — drag from Hierarchy")]
    [SerializeField] private WorkTable    workTable;
    [SerializeField] private PingPongTable pingPongTable;

    private void Awake()
    {
        BuildWorkTableUI();
        BuildPingPongUI();
    }

    // ══════════════════════════════════════════════════════════════════════
    // WORK TABLE UI
    // ══════════════════════════════════════════════════════════════════════

    private void BuildWorkTableUI()
    {
        // Canvas
        Canvas canvas = CreateCanvas("WorkTableCanvas");

        // Dark panel
        GameObject panel = CreatePanel(canvas.transform, "WorkTablePanel",
            new Vector2(600, 400), new Color(0.15f, 0.12f, 0.08f, 0.97f));
        panel.SetActive(false);

        // Title
        CreateTMPText(panel.transform, "Title", "Work Table",
            new Vector2(0, 130), new Vector2(560, 60), 36, FontStyles.Bold,
            new Color(1f, 0.85f, 0.4f));

        // Close button
        Button closeBtn = CreateButton(panel.transform, "CloseButton", "Close",
            new Vector2(0, -150), new Vector2(160, 50),
            new Color(0.8f, 0.2f, 0.2f));

        // WorkTableUI script
        WorkTableUI wtUI = canvas.gameObject.AddComponent<WorkTableUI>();
        UIReflectionHelper.SetPrivate(wtUI, "panel",       panel);
        UIReflectionHelper.SetPrivate(wtUI, "closeButton", closeBtn);

        // Wire to interactable
        if (workTable != null)
            UIReflectionHelper.SetPrivate(workTable, "workTableUI", wtUI);
    }

    // ══════════════════════════════════════════════════════════════════════
    // PING PONG UI
    // ══════════════════════════════════════════════════════════════════════

    private void BuildPingPongUI()
    {
        Canvas canvas = CreateCanvas("PingPongCanvas");

        // Green panel
        GameObject panel = CreatePanel(canvas.transform, "PingPongPanel",
            new Vector2(700, 500), new Color(0.05f, 0.25f, 0.1f, 0.97f));
        panel.SetActive(false);

        // Paddle (left)
        RectTransform paddle = CreateImage(panel.transform, "Paddle",
            new Vector2(-310, 0), new Vector2(20, 120), Color.white).rectTransform;

        // Ball (center, yellow)
        RectTransform ball = CreateImage(panel.transform, "Ball",
            new Vector2(0, 0), new Vector2(24, 24), new Color(1f, 0.9f, 0.1f)).rectTransform;

        // Right wall
        RectTransform wallRight = CreateImage(panel.transform, "WallRight",
            new Vector2(310, 0), new Vector2(20, 500), Color.white).rectTransform;

        // Score text (top center)
        TMP_Text scoreText = CreateTMPText(panel.transform, "ScoreText", "Score: 0",
            new Vector2(0, 210), new Vector2(200, 40), 28, FontStyles.Bold, Color.white);

        // Quit button (top-right)
        Button quitBtn = CreateButton(panel.transform, "QuitButton", "Quit",
            new Vector2(290, 210), new Vector2(80, 36),
            new Color(0.8f, 0.2f, 0.2f));

        // PingPongGameUI script
        PingPongGameUI ppUI = canvas.gameObject.AddComponent<PingPongGameUI>();
        UIReflectionHelper.SetPrivate(ppUI, "panel",        panel);
        UIReflectionHelper.SetPrivate(ppUI, "closeButton",  quitBtn);
        UIReflectionHelper.SetPrivate(ppUI, "ball",         ball);
        UIReflectionHelper.SetPrivate(ppUI, "playerPaddle", paddle);
        UIReflectionHelper.SetPrivate(ppUI, "wallRight",    wallRight);
        UIReflectionHelper.SetPrivate(ppUI, "scoreText",    scoreText);

        // Wire to interactable
        if (pingPongTable != null)
            UIReflectionHelper.SetPrivate(pingPongTable, "pingPongUI", ppUI);
    }

    // ══════════════════════════════════════════════════════════════════════
    // HELPERS
    // ══════════════════════════════════════════════════════════════════════

    private static Canvas CreateCanvas(string name)
    {
        GameObject go = new GameObject(name);
        DontDestroyOnLoad(go);
        Canvas canvas = go.AddComponent<Canvas>();
        canvas.renderMode = RenderMode.ScreenSpaceOverlay;
        canvas.sortingOrder = 10;
        go.AddComponent<CanvasScaler>().uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
        go.AddComponent<GraphicRaycaster>();
        return canvas;
    }

    private static GameObject CreatePanel(Transform parent, string name,
        Vector2 size, Color color)
    {
        GameObject go = new GameObject(name);
        go.transform.SetParent(parent, false);
        RectTransform rt = go.AddComponent<RectTransform>();
        rt.sizeDelta = size;
        rt.anchoredPosition = Vector2.zero;
        Image img = go.AddComponent<Image>();
        img.color = color;
        return go;
    }

    private static Image CreateImage(Transform parent, string name,
        Vector2 position, Vector2 size, Color color)
    {
        GameObject go = new GameObject(name);
        go.transform.SetParent(parent, false);
        RectTransform rt = go.AddComponent<RectTransform>();
        rt.anchorMin = rt.anchorMax = new Vector2(0.5f, 0.5f);
        rt.anchoredPosition = position;
        rt.sizeDelta = size;
        Image img = go.AddComponent<Image>();
        img.color = color;
        return img;
    }

    private static TMP_Text CreateTMPText(Transform parent, string name, string text,
        Vector2 position, Vector2 size, float fontSize, FontStyles style, Color color)
    {
        GameObject go = new GameObject(name);
        go.transform.SetParent(parent, false);
        RectTransform rt = go.AddComponent<RectTransform>();
        rt.anchorMin = rt.anchorMax = new Vector2(0.5f, 0.5f);
        rt.anchoredPosition = position;
        rt.sizeDelta = size;
        TMP_Text tmp = go.AddComponent<TextMeshProUGUI>();
        tmp.text = text;
        tmp.fontSize = fontSize;
        tmp.fontStyle = style;
        tmp.color = color;
        tmp.alignment = TextAlignmentOptions.Center;
        return tmp;
    }

    private static Button CreateButton(Transform parent, string name, string label,
        Vector2 position, Vector2 size, Color bgColor)
    {
        GameObject go = new GameObject(name);
        go.transform.SetParent(parent, false);
        RectTransform rt = go.AddComponent<RectTransform>();
        rt.anchorMin = rt.anchorMax = new Vector2(0.5f, 0.5f);
        rt.anchoredPosition = position;
        rt.sizeDelta = size;
        Image img = go.AddComponent<Image>();
        img.color = bgColor;
        Button btn = go.AddComponent<Button>();

        // Label child
        GameObject labelGo = new GameObject("Label");
        labelGo.transform.SetParent(go.transform, false);
        RectTransform lrt = labelGo.AddComponent<RectTransform>();
        lrt.anchorMin = Vector2.zero;
        lrt.anchorMax = Vector2.one;
        lrt.sizeDelta = Vector2.zero;
        lrt.anchoredPosition = Vector2.zero;
        TMP_Text tmp = labelGo.AddComponent<TextMeshProUGUI>();
        tmp.text = label;
        tmp.fontSize = 22;
        tmp.fontStyle = FontStyles.Bold;
        tmp.color = Color.white;
        tmp.alignment = TextAlignmentOptions.Center;

        return btn;
    }
}

