using UnityEngine;

public class TugEngineUIManager : MonoBehaviour
{
    [Header("Setup")]
    public CombinedEngineControllerUnit controller;
    public TugEngineWidgetUI widgetPrefab;
    public RectTransform container; // e.g. a VerticalLayoutGroup parent

    private TugEngineWidgetUI[] widgets;

    private void Awake()
    {
        if (controller == null)
            controller = FindObjectOfType<CombinedEngineControllerUnit>();

        if (container == null)
            container = GetComponent<RectTransform>();

        BuildWidgets();
    }

    private void BuildWidgets()
    {
        if (controller == null || controller.engines == null || widgetPrefab == null || container == null)
        {
            Debug.LogWarning("TugEngineUIManager: missing references.");
            return;
        }

        // Clear any old children
        for (int i = container.childCount - 1; i >= 0; i--)
        {
            Destroy(container.GetChild(i).gameObject);
        }

        var engines = controller.engines;
        widgets = new TugEngineWidgetUI[engines.Length];

        for (int i = 0; i < engines.Length; i++)
        {
            TugEngine engine = engines[i];
            var widgetInstance = Instantiate(widgetPrefab, container);
            widgetInstance.name = $"EngineWidget_{i}_{engine.engineName}";
            widgetInstance.Bind(engine);
            widgets[i] = widgetInstance;
        }
    }
}