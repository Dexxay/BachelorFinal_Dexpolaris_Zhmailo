// Based on the user's provided code, applying specific texture size requirements and contour clarity.
using UnityEngine;
using UnityEngine.SceneManagement;

public class ObjectRadius : MonoBehaviour
{
    [SerializeField] private float radius = 5.0f;
    [SerializeField] private Color radiusColor = Color.red;
    [SerializeField, Range(0f, 1f)] private float radiusTransparency = 0.1f;

    [SerializeField] private float range = 0.0f;
    [SerializeField] private Color rangeColor = Color.blue;
    [SerializeField, Range(0f, 1f)] private float rangeTransparency = 0.1f;

    [SerializeField] private float planeHeightOffset = 0.1f;
    [SerializeField] private string editorSceneName = "LevelEditorTemplate";
    [SerializeField] private string editorPlaneTag = "EditorPlane";

 
    [SerializeField] private int rangeTextureSize = 512;
    [SerializeField] private int radiusTextureSize = 128;  

    private GameObject radiusSpriteInstance;
    private GameObject rangeSpriteInstance;
    private float editorPlaneHeight = 0f;

    public float Radius => radius;

    public void SetRange(float newRange)
    {
        range = newRange;
        UpdateVisualization();
        CheckAndShowRadiusVisualization();
    }

    void OnEnable()
    {
        SceneManager.sceneLoaded += OnSceneLoaded;
        FindEditorPlaneHeight();
        CheckAndShowRadiusVisualization();
    }

    void OnDisable()
    {
        SceneManager.sceneLoaded -= OnSceneLoaded;
        HideVisualization();
    }

    void OnDestroy()
    {
        HideVisualization();
    }

    void OnValidate()
    {
        FindEditorPlaneHeight();
        if (radiusSpriteInstance != null || rangeSpriteInstance != null)
        {
            UpdateVisualization();
        }
    }

    void Update()
    {
        if (Application.isEditor && SceneManager.GetActiveScene().name == editorSceneName)
        {
            if (editorPlaneHeight == 0f && GameObject.FindGameObjectWithTag(editorPlaneTag) != null)
            {
                FindEditorPlaneHeight();
            }
            if (radiusSpriteInstance != null && radiusSpriteInstance.activeSelf || (rangeSpriteInstance != null && rangeSpriteInstance.activeSelf))
            {
                UpdateVisualization();
            }
        }
    }

    private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        FindEditorPlaneHeight();
        CheckAndShowRadiusVisualization();
    }

    private void FindEditorPlaneHeight()
    {
        if (SceneManager.GetActiveScene().name == editorSceneName)
        {
            GameObject editorPlane = GameObject.FindGameObjectWithTag(editorPlaneTag);
            if (editorPlane != null)
            {
                Collider planeCollider = editorPlane.GetComponent<Collider>();
                if (planeCollider != null)
                {
                    editorPlaneHeight = planeCollider.bounds.max.y;
                }
                else
                {
                    editorPlaneHeight = editorPlane.transform.position.y;
                    Debug.LogWarning($"Editor plane object '{editorPlaneTag}' found but has no Collider. Using its Y position for height.");
                }
            }
            else
            {
                Debug.LogWarning($"Editor plane object with tag '{editorPlaneTag}' not found in scene. Visualization height will be based on default 0.");
                editorPlaneHeight = 0f;
            }
        }
        else
        {
            editorPlaneHeight = 0f;
        }
    }

    public void CheckAndShowRadiusVisualization()
    {
        bool show = Application.isEditor && SceneManager.GetActiveScene().name == editorSceneName;
        ShowRadiusVisualization(show);
    }

    public void ShowRadiusVisualization(bool show)
    {
        if (show)
        {
            if (editorPlaneHeight == 0f && SceneManager.GetActiveScene().name == editorSceneName && GameObject.FindGameObjectWithTag(editorPlaneTag) != null)
            {
                FindEditorPlaneHeight();
            }

             if (radiusSpriteInstance == null)
            {
                int textureSize = radiusTextureSize;  
                Texture2D circleTexture = new Texture2D(textureSize, textureSize);
                Color fillColor = radiusColor;
                fillColor.a = radiusTransparency;

                for (int y = 0; y < textureSize; y++)
                {
                    for (int x = 0; x < textureSize; x++)
                    {
                        float dist = Vector2.Distance(new Vector2(x, y), new Vector2(textureSize / 2, textureSize / 2));
                        if (dist <= textureSize / 2)
                        {
                            circleTexture.SetPixel(x, y, fillColor);
                        }
                        else
                        {
                            circleTexture.SetPixel(x, y, Color.clear);
                        }
                    }
                }
                circleTexture.Apply();

                Sprite circleSprite = Sprite.Create(circleTexture, new Rect(0, 0, textureSize, textureSize), new Vector2(0.5f, 0.5f), 100f);

                radiusSpriteInstance = new GameObject("Radius Fill");
                SpriteRenderer spriteRenderer = radiusSpriteInstance.AddComponent<SpriteRenderer>();
                spriteRenderer.sprite = circleSprite;
                spriteRenderer.sortingOrder = -1;

                radiusSpriteInstance.hideFlags = HideFlags.HideAndDontSave | HideFlags.HideInHierarchy;
            }
            radiusSpriteInstance.SetActive(true);

             if (range > 0.0f)
            {
                if (rangeSpriteInstance == null)
                {
                    int textureSize = rangeTextureSize;  
                    Texture2D contourTexture = new Texture2D(textureSize, textureSize);
                    Color contourColor = rangeColor;
                    contourColor.a = rangeTransparency;
                    float outlineThickness = 2f;  

                    for (int y = 0; y < textureSize; y++)
                    {
                        for (int x = 0; x < textureSize; x++)
                        {
                            float dist = Vector2.Distance(new Vector2(x, y), new Vector2(textureSize / 2, textureSize / 2));
                            if (dist >= textureSize / 2 - outlineThickness && dist <= textureSize / 2)
                            {
                                contourTexture.SetPixel(x, y, contourColor);
                            }
                            else
                            {
                                contourTexture.SetPixel(x, y, Color.clear);
                            }
                        }
                    }
                    contourTexture.Apply();

                    Sprite contourSprite = Sprite.Create(contourTexture, new Rect(0, 0, textureSize, textureSize), new Vector2(0.5f, 0.5f), 100f);

                    rangeSpriteInstance = new GameObject("Range Contour");
                    SpriteRenderer spriteRenderer = rangeSpriteInstance.AddComponent<SpriteRenderer>();
                    spriteRenderer.sprite = contourSprite;
                    spriteRenderer.sortingOrder = 0;

                    rangeSpriteInstance.hideFlags = HideFlags.HideAndDontSave | HideFlags.HideInHierarchy;
                }
                rangeSpriteInstance.SetActive(true);
            }
            else
            {
                if (rangeSpriteInstance != null)
                {
                    DestroyImmediate(rangeSpriteInstance);
                    rangeSpriteInstance = null;
                }
            }

            UpdateVisualization();

        }
        else
        {
            if (radiusSpriteInstance != null)
            {
                radiusSpriteInstance.SetActive(false);
            }
            if (rangeSpriteInstance != null)
            {
                rangeSpriteInstance.SetActive(false);
            }
        }
    }

    public void HideVisualization()
    {
        if (radiusSpriteInstance != null)
        {
            DestroyImmediate(radiusSpriteInstance);
            radiusSpriteInstance = null;
        }
        if (rangeSpriteInstance != null)
        {
            DestroyImmediate(rangeSpriteInstance);
            rangeSpriteInstance = null;
        }
    }

    private void UpdateVisualization()
    {
        if (radiusSpriteInstance == null && rangeSpriteInstance == null) return;

         if (radiusSpriteInstance != null)
        {
            radiusSpriteInstance.transform.position = new Vector3(transform.position.x, editorPlaneHeight + planeHeightOffset, transform.position.z);
            radiusSpriteInstance.transform.rotation = Quaternion.Euler(90f, 0f, 0f);

            int textureSize = radiusTextureSize;  
            float baseSpriteWorldDiameter = textureSize / 100f;

            float desiredWorldDiameter = radius * 2f;
            float scaleFactor = desiredWorldDiameter / baseSpriteWorldDiameter;
            radiusSpriteInstance.transform.localScale = new Vector3(scaleFactor, scaleFactor, 1f);

            SpriteRenderer spriteRenderer = radiusSpriteInstance.GetComponent<SpriteRenderer>();
            if (spriteRenderer != null)
            {
                Color spriteColor = radiusColor;
                spriteColor.a = radiusTransparency;
                spriteRenderer.color = spriteColor;
            }
        }

         if (rangeSpriteInstance != null)
        {
            rangeSpriteInstance.transform.position = new Vector3(transform.position.x, editorPlaneHeight + planeHeightOffset, transform.position.z);
            rangeSpriteInstance.transform.rotation = Quaternion.Euler(90f, 0f, 0f);

            int textureSize = rangeTextureSize;  
            float baseSpriteWorldDiameter = textureSize / 100f;

            float desiredWorldDiameter = range * 2f;
            float scaleFactor = desiredWorldDiameter / baseSpriteWorldDiameter;
            rangeSpriteInstance.transform.localScale = new Vector3(scaleFactor, scaleFactor, 1f);

            SpriteRenderer spriteRenderer = rangeSpriteInstance.GetComponent<SpriteRenderer>();
            if (spriteRenderer != null)
            {
                Color spriteColor = rangeColor;
                spriteColor.a = rangeTransparency;
                spriteRenderer.color = spriteColor;
            }
        }
    }
}