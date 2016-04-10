using UnityEngine;
using UnityEditor;
using System.Reflection;

public class ProceduralViewer : EditorWindow {

    public int Views {
        get { return views; }
        set {
            if (views != value) {
                views = value;
                InitializeViews();
            }
        }
    }

    public MonoScript Script {
        get { return script; }
        set {
            if (script != value && value.GetClass().IsSubclassOf(typeof(ProceduralObject))) {
                script = value;
                scriptFold = true;
                parameters = new System.Object[script.GetClass().GetConstructors()[0].GetParameters().Length];

                for (int i = 0; i < parameters.Length; i++) {
                    parameters[i] = script.GetClass().GetConstructors()[0].GetParameters()[i].IsOptional ? script.GetClass().GetConstructors()[0].GetParameters()[i].DefaultValue : 0;
                }
            }
        }
    }

    public static int panelWidth = 300;
    public static Color clearColor = Color.black;
    public static int textureSize = 2048;

    public int views = 4;
    public float rotationSpeed = 2;
    public float cameraAngle = 0.5f;
    public float cameraDistance = 2;
    public MonoScript script;
    public System.Object[] parameters;

    private GameObject root;
    private RenderTexture texture;
    private Camera[,] cameras;
    private GameObject proceduralObject;

    private int layer = LayerMask.NameToLayer("ProceduralViewer");

    private bool scriptFold = true;

    [MenuItem("Window/Procedural Viewer")]
    static void Init() { GetWindow<ProceduralViewer>("Viewer"); }

    void OnGUI() {
        //options
        GUILayoutOption[] options = { GUILayout.MaxWidth(panelWidth - 5) };

        // Panel Area
        Views = EditorGUILayout.IntSlider("Views", views, 1, 8, options);
        rotationSpeed = EditorGUILayout.Slider("Rotation Speed", rotationSpeed, -50, 50, options);
        cameraAngle = EditorGUILayout.Slider("Camera Angle", cameraAngle, 0, 1, options);
        cameraDistance = EditorGUILayout.Slider("Camera Distance", cameraDistance, 1, 10, options);
        Script = EditorGUILayout.ObjectField("Procedural Object", script, typeof(MonoScript), false, options) as MonoScript;

        // Procedural Object Parameters
        scriptFold = EditorGUILayout.InspectorTitlebar(scriptFold, script) && script != null;
        if (scriptFold) {
            if (parameters == null) script = null;
            else {
                EditorGUILayout.Space();
                ParameterInfo[] paramInfo = script.GetClass().GetConstructors()[0].GetParameters();
                for (int i = 0; i < paramInfo.Length; i++) {
                    if (paramInfo[i].ParameterType == typeof(float)) {
                        parameters[i] = EditorGUILayout.FloatField(paramInfo[i].Name, System.Convert.ToSingle(parameters[i]), options);
                    }
                    else if (paramInfo[i].ParameterType == typeof(int)) {
                        parameters[i] = EditorGUILayout.IntField(paramInfo[i].Name, System.Convert.ToInt32(parameters[i]), options);
                    }
                }

                // Generate Button
                if (GUILayout.Button("Generate", options)) {
                    if (proceduralObject != null) DestroyImmediate(proceduralObject);
                    proceduralObject = (script.GetClass().GetConstructors()[0].Invoke(parameters) as ProceduralObject).GameObject();
                    proceduralObject.layer = LayerMask.NameToLayer("ProceduralViewer");
                    proceduralObject.transform.parent = root.transform;
                }
            }
        }

        if (texture == null || cameras == null) InitializeViews();

        // Preview Area
        UpdateViews();
        Rect previewArea = new Rect(panelWidth, 0, position.width - panelWidth, position.height);
        EditorGUI.DrawRect(previewArea, clearColor);
        if (texture != null) EditorGUI.DrawPreviewTexture(previewArea, texture, null, ScaleMode.ScaleToFit);
    }

    void Update() {
        if (proceduralObject != null) proceduralObject.transform.Rotate(new Vector3(0, rotationSpeed, 0));
        Repaint();
    }

    void OnDestroy() {
        DestroyImmediate(root);
    }

    private void UpdateViews() {
        int viewSize = textureSize / views;
        for (int i = 0; i < views; i++) {
            for (int j = 0; j < views; j++) {
                if (proceduralObject != null) {
                    cameras[i, j].transform.position = new Vector3(0, -cameraDistance * Mathf.Sin(-cameraAngle * Mathf.PI / 2), -cameraDistance * Mathf.Cos(-cameraAngle * Mathf.PI / 2));
                    cameras[i, j].transform.LookAt(proceduralObject.transform);
                }
                cameras[i, j].pixelRect = new Rect(i * viewSize, j * viewSize, viewSize, viewSize);
            }
        }
    }

    private void InitializeViews() {
        // Clean Up
        DestroyImmediate(root);

        // Create Root
        root = new GameObject("ProceduralViewer");
        root.layer = layer;

        GameObject cameraRoot = new GameObject("Camera");

        // Create RenderTexture
        texture = new RenderTexture(textureSize, textureSize, 16);
        texture.antiAliasing = 8;
        texture.Create();

        //Create Cameras & Objects to hold them
        GameObject[,] cameraObjects = new GameObject[views, views];
        cameras = new Camera[views, views];

        float viewSize = (float)textureSize / views;

        for (int i = 0; i < views; i++) {
            for (int j = 0; j < views; j++) {
                cameraObjects[i, j] = new GameObject("Camera[" + i + ", " + j + "]");
                cameraObjects[i, j].transform.parent = cameraRoot.transform;
                cameraObjects[i, j].layer = layer;
                cameraObjects[i, j].transform.position = new Vector3(0, 0, -5);

                cameras[i, j] = cameraObjects[i, j].AddComponent<Camera>();
                cameras[i, j].pixelRect = new Rect(i * viewSize, j * viewSize, viewSize, viewSize);
                cameras[i, j].clearFlags = CameraClearFlags.SolidColor;
                cameras[i, j].backgroundColor = clearColor;
                cameras[i, j].cullingMask = 1 << layer;
                cameras[i, j].targetTexture = texture;
            }
        }
    }
}
