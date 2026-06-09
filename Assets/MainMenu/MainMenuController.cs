using System.IO;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class MainMenuController : MonoBehaviour
{
    const string GAME_SCENE = "TestScene";

    static readonly Color BG_COLOR      = new Color(0.031f, 0.039f, 0.059f);
    static readonly Color PANEL_BG      = new Color(0.015f, 0.020f, 0.038f, 0.98f);
    static readonly Color ACCENT        = new Color(0.18f,  0.85f,  1.00f);
    static readonly Color ACCENT_BRIGHT = new Color(0.55f,  0.96f,  1.00f);
    static readonly Color TEXT_NORMAL   = new Color(0.75f,  0.92f,  1.00f);
    static readonly Color TEXT_DISABLED = new Color(0.22f,  0.30f,  0.38f);

    Font uiFont;
    GameObject creditsPanel;
    AudioSource audioSource;
    AudioClip sfxHover;
    AudioClip sfxClick;
    AudioClip sfxOpen;
    float menuBaseVolume = 0.5f;

    void Awake()
    {
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;

        uiFont = LoadFont();

        audioSource = gameObject.AddComponent<AudioSource>();
        audioSource.playOnAwake = false;
        sfxHover = Resources.Load<AudioClip>("Sounds/hover_button_lone");
        sfxClick = Resources.Load<AudioClip>("Sounds/click-button-lone");
        sfxOpen  = Resources.Load<AudioClip>("Sounds/menu-open-lone");

        if (Camera.main != null)
        {
            Camera.main.backgroundColor = BG_COLOR;
            Camera.main.clearFlags = CameraClearFlags.SolidColor;
        }

        BuildUI();

        if (sfxOpen != null) audioSource.PlayOneShot(sfxOpen, menuBaseVolume);
    }

    Font LoadFont()
    {
        Font f = Resources.Load<Font>("Fonts/Orbitron-Bold");
        if (f != null) return f;

        f = Resources.Load<Font>("MainMenu/Fonts/Orbitron-Bold");
        if (f != null) return f;

        f = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
        if (f == null) f = Resources.GetBuiltinResource<Font>("Arial.ttf");
        return f;
    }

    void BuildUI()
    {
        var canvasGO = new GameObject("Canvas");
        var canvas = canvasGO.AddComponent<Canvas>();
        canvas.renderMode = RenderMode.ScreenSpaceOverlay;
        canvas.sortingOrder = 10;

        var scaler = canvasGO.AddComponent<CanvasScaler>();
        scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
        scaler.referenceResolution = new Vector2(1920, 1080);
        scaler.matchWidthOrHeight = 0.5f;

        canvasGO.AddComponent<GraphicRaycaster>();

        MakeImage(canvasGO.transform, "Background", PANEL_BG, Vector2.zero, Vector2.one);

        // HUD corner brackets
        MakeHUDBracket(canvasGO.transform, false, true);   // top-left
        MakeHUDBracket(canvasGO.transform, true,  true);   // top-right
        MakeHUDBracket(canvasGO.transform, false, false);  // bottom-left
        MakeHUDBracket(canvasGO.transform, true,  false);  // bottom-right

        // Title
        var titleTxt = MakeText(canvasGO.transform, "TitleText", "LONE",
            96, FontStyle.Bold, TextAnchor.MiddleCenter,
            new Vector2(0.10f, 0.68f), new Vector2(0.90f, 0.95f));
        titleTxt.color = ACCENT;
        titleTxt.resizeTextForBestFit = true;
        titleTxt.resizeTextMinSize = 24;
        titleTxt.resizeTextMaxSize = 120;

        var subTxt = MakeText(canvasGO.transform, "SubtitleText",
            "DESIGNATION: UNKNOWN   //   SECTOR X-17   //   ENVIRONMENT: HOSTILE",
            15, FontStyle.Normal, TextAnchor.MiddleCenter,
            new Vector2(0.10f, 0.635f), new Vector2(0.90f, 0.662f));
        subTxt.color = new Color(ACCENT.r, ACCENT.g, ACCENT.b, 0.45f);

        // Thick accent rule
        MakeImage(canvasGO.transform, "TitleDivider",
            new Color(ACCENT.r, ACCENT.g, ACCENT.b, 0.30f),
            new Vector2(0.10f, 0.625f), new Vector2(0.90f, 0.633f));

        // Buttons
        float topY  = 0.565f;
        float stepY = 0.105f;
        float btnH  = 0.082f;

        MakeButton(canvasGO.transform, "NEW GAME",
            new Vector2(0.25f, topY - 0 * stepY - btnH),
            new Vector2(0.75f, topY - 0 * stepY), OnNewGame);

        MakeButton(canvasGO.transform, "CONTINUE",
            new Vector2(0.25f, topY - 1 * stepY - btnH),
            new Vector2(0.75f, topY - 1 * stepY), OnContinue);


        MakeButton(canvasGO.transform, "CREDITS",
            new Vector2(0.25f, topY - 3 * stepY - btnH),
            new Vector2(0.75f, topY - 3 * stepY), OnCredits);


        var buildTxt = MakeText(canvasGO.transform, "BuildVer",
            "Version 1.0.0", 13, FontStyle.Normal, TextAnchor.LowerRight,
            new Vector2(0.60f, 0.05f), new Vector2(0.87f, 0.09f));
        buildTxt.color = new Color(ACCENT.r, ACCENT.g, ACCENT.b, 0.18f);

        creditsPanel = BuildCreditsPanel(canvasGO.transform);
        creditsPanel.SetActive(false);
    }

    void MakeHUDBracket(Transform parent, bool isRight, bool isTop)
    {
        Color col = new Color(ACCENT.r, ACCENT.g, ACCENT.b, 0.65f);

        float arm    = 0.080f;
        // Anchor coords are normalised to canvas size, so a fraction in X maps to
        // a different pixel count than the same fraction in Y on a 16:9 screen.
        // Using separate values keeps both arms visually equal-width (≈12 px each).
        float thickH = 0.011f;  // height of the horizontal arm  (×1080 ≈ 12 px)
        float thickV = 0.006f;  // width  of the vertical   arm  (×1920 ≈ 12 px)

        float ex = isRight ? 0.968f : 0.032f;
        float ey = isTop   ? 0.962f : 0.038f;

        // Horizontal arm
        float hx0 = isRight ? ex - arm : ex;
        float hx1 = isRight ? ex       : ex + arm;
        float hy0 = isTop   ? ey - thickH : ey;
        float hy1 = isTop   ? ey           : ey + thickH;
        MakeImage(parent, "H", col, new Vector2(hx0, hy0), new Vector2(hx1, hy1));

        // Vertical arm
        float vx0 = isRight ? ex - thickV : ex;
        float vx1 = isRight ? ex           : ex + thickV;
        float vy0 = isTop   ? ey - arm : ey;
        float vy1 = isTop   ? ey       : ey + arm;
        MakeImage(parent, "V", col, new Vector2(vx0, vy0), new Vector2(vx1, vy1));
    }

    // Credits 

    GameObject BuildCreditsPanel(Transform parent)
    {
        var panel = MakeImage(parent, "CreditsPanel", PANEL_BG,
            Vector2.zero, Vector2.one).gameObject;

        // HUD brackets inside credits panel too
        MakeHUDBracket(panel.transform, false, true);
        MakeHUDBracket(panel.transform, true,  true);
        MakeHUDBracket(panel.transform, false, false);
        MakeHUDBracket(panel.transform, true,  false);

        var headerTxt = MakeText(panel.transform, "CreditsHeader", "CREDITS",
            72, FontStyle.Bold, TextAnchor.MiddleCenter,
            new Vector2(0.05f, 0.83f), new Vector2(0.95f, 0.97f));
        headerTxt.color = ACCENT;
        headerTxt.resizeTextForBestFit = true;
        headerTxt.resizeTextMinSize = 20;
        headerTxt.resizeTextMaxSize = 72;

        MakeImage(panel.transform, "HeaderLine",
            new Color(ACCENT.r, ACCENT.g, ACCENT.b, 0.30f),
            new Vector2(0.10f, 0.803f), new Vector2(0.90f, 0.811f));

        string body =
            "PROGRAMMING\n" +
            "    <color=#5DDCFF>Aryan P., Julian S., Carter V., & Jack N.</color>\n\n" +
            "GAME DESIGN  //  LEVEL DESIGN  //  ART\n" +
            "    <color=#5DDCFF>Nathanael T. & Carter V.</color>\n\n" +
            "UI\n" +
            "    <color=#5DDCFF>Aryan P. & Julian S.</color>\n\n" +
            "Built with Unity 6  ·  URP 2D";

        var bodyTxt = MakeText(panel.transform, "CreditsBody", body,
            22, FontStyle.Normal, TextAnchor.MiddleCenter,
            new Vector2(0.15f, 0.22f), new Vector2(0.85f, 0.80f));
        bodyTxt.color = TEXT_NORMAL;
        bodyTxt.horizontalOverflow = HorizontalWrapMode.Wrap;
        bodyTxt.verticalOverflow = VerticalWrapMode.Overflow;
        bodyTxt.lineSpacing = 1.15f;

        MakeButton(panel.transform, "<< [BACK]",
            new Vector2(0.38f, 0.06f), new Vector2(0.62f, 0.15f),
            OnCloseCredits);

        return panel;
    }

    // Button handlers

    void OnNewGame()
    {
        string savePath = Path.Combine(Application.persistentDataPath, "save.txt");
        if (File.Exists(savePath)) File.Delete(savePath);
        SceneManager.LoadScene(GAME_SCENE);
    }

    void OnContinue() => SceneManager.LoadScene(GAME_SCENE);

    void OnCredits()      { if (creditsPanel != null) creditsPanel.SetActive(true);  }
    void OnCloseCredits() { if (creditsPanel != null) creditsPanel.SetActive(false); }

    // UI helpers

    void MakeButton(Transform parent, string label, Vector2 anchorMin, Vector2 anchorMax,
                    UnityEngine.Events.UnityAction onClick, bool disabled = false)
    {
        var go = new GameObject(label);
        go.transform.SetParent(parent, false);
        SetAnchors(go.AddComponent<RectTransform>(), anchorMin, anchorMax);

        var bg = MakeImage(go.transform, "HoverBg",
            new Color(ACCENT.r, ACCENT.g, ACCENT.b, 0.00f),
            Vector2.zero, Vector2.one);

        var lbl = MakeText(go.transform, "Label", label,
            34, FontStyle.Bold, TextAnchor.MiddleCenter,
            Vector2.zero, Vector2.one);
        lbl.color = disabled ? TEXT_DISABLED : TEXT_NORMAL;

        if (disabled) return;

        var img = go.AddComponent<Image>();
        img.color = Color.clear;

        var btn = go.AddComponent<Button>();
        var cols = btn.colors;
        cols.normalColor = cols.highlightedColor = cols.pressedColor = cols.selectedColor = Color.clear;
        btn.colors = cols;
        btn.targetGraphic = img;

        if (onClick != null)
            btn.onClick.AddListener(() =>
            {
                if (sfxClick != null) audioSource.PlayOneShot(sfxClick, menuBaseVolume);
                onClick();
            });

        var trigger = go.AddComponent<EventTrigger>();

        var enter = new EventTrigger.Entry { eventID = EventTriggerType.PointerEnter };
        enter.callback.AddListener(_ =>
        {
            lbl.color = ACCENT_BRIGHT;
            bg.color  = new Color(ACCENT.r, ACCENT.g, ACCENT.b, 0.09f);
            if (sfxHover != null) audioSource.PlayOneShot(sfxHover, menuBaseVolume);
        });
        trigger.triggers.Add(enter);

        var leave = new EventTrigger.Entry { eventID = EventTriggerType.PointerExit };
        leave.callback.AddListener(_ =>
        {
            lbl.color = TEXT_NORMAL;
            bg.color  = new Color(ACCENT.r, ACCENT.g, ACCENT.b, 0.00f);
        });
        trigger.triggers.Add(leave);
    }

    Image MakeImage(Transform parent, string name, Color color,
                    Vector2 anchorMin, Vector2 anchorMax)
    {
        var go = new GameObject(name);
        go.transform.SetParent(parent, false);
        SetAnchors(go.AddComponent<RectTransform>(), anchorMin, anchorMax);
        var img = go.AddComponent<Image>();
        img.color = color;
        return img;
    }

    Text MakeText(Transform parent, string name, string content, int size,
                  FontStyle style, TextAnchor anchor,
                  Vector2 anchorMin, Vector2 anchorMax)
    {
        var go = new GameObject(name);
        go.transform.SetParent(parent, false);
        SetAnchors(go.AddComponent<RectTransform>(), anchorMin, anchorMax);
        var txt = go.AddComponent<Text>();
        txt.font = uiFont;
        txt.text = content;
        txt.fontSize = size;
        txt.fontStyle = style;
        txt.alignment = anchor;
        txt.supportRichText = true;
        txt.horizontalOverflow = HorizontalWrapMode.Overflow;
        txt.verticalOverflow = VerticalWrapMode.Overflow;
        return txt;
    }

    void SetAnchors(RectTransform rt, Vector2 min, Vector2 max)
    {
        rt.anchorMin = min;
        rt.anchorMax = max;
        rt.sizeDelta = Vector2.zero;
        rt.anchoredPosition = Vector2.zero;
    }
}
