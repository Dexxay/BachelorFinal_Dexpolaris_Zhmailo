using UnityEngine;
using UnityEngine.SceneManagement;

public class ObjectRadius : MonoBehaviour
{
    [SerializeField] public float radius = 5.0f;
    [SerializeField] private Color radiusColor = Color.red;
    [SerializeField, Range(0f, 1f)] private float radiusTransparency = 0.1f;
    [SerializeField] private float planeHeightOffset = 0.1f;
    [SerializeField] private string editorSceneName = "LevelEditorTemplate";
    [SerializeField] private string editorPlaneTag = "EditorPlane";

    private GameObject radiusSpriteInstance;
    private float editorPlaneHeight = 0f;

    void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, radius);
    }

    void OnEnable()
    {
        SceneManager.sceneLoaded += OnSceneLoaded;
        FindEditorPlaneHeight();
    }

    void OnDisable()
    {
        SceneManager.sceneLoaded -= OnSceneLoaded;
        HideRadiusVisualization();
    }

    void OnDestroy()
    {
        HideRadiusVisualization();
    }

    void OnValidate()
    {
        FindEditorPlaneHeight();
    }

    void Update()
    {
        if (radiusSpriteInstance != null && SceneManager.GetActiveScene().name == editorSceneName)
        {
            if (editorPlaneHeight == 0f && GameObject.FindGameObjectWithTag(editorPlaneTag) != null)
            {
                FindEditorPlaneHeight();
            }
            radiusSpriteInstance.transform.position = new Vector3(transform.position.x, editorPlaneHeight + planeHeightOffset, transform.position.z);

            SpriteRenderer spriteRenderer = radiusSpriteInstance.GetComponent<SpriteRenderer>();
            if (spriteRenderer != null)
            {
                Color spriteColor = radiusColor;
                spriteColor.a = radiusTransparency;
                spriteRenderer.color = spriteColor;
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
                Debug.LogWarning($"Editor plane object with tag '{editorPlaneTag}' not found in scene. Radius sprite height will be based on default 0.");
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
        if (SceneManager.GetActiveScene().name == editorSceneName)
        {
            ShowRadiusVisualization(true);
        }
        else
        {
            ShowRadiusVisualization(false);
        }
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
                int textureSize = 128;
                Texture2D circleTexture = new Texture2D(textureSize, textureSize);
                Color circleColor = radiusColor;
                circleColor.a = radiusTransparency;

                for (int y = 0; y < textureSize; y++)
                {
                    for (int x = 0; x < textureSize; x++)
                    {
                        float dist = Vector2.Distance(new Vector2(x, y), new Vector2(textureSize / 2, textureSize / 2));
                        if (dist <= textureSize / 2)
                        {
                            circleTexture.SetPixel(x, y, circleColor);
                        }
                        else
                        {
                            circleTexture.SetPixel(x, y, Color.clear);
                        }
                    }
                }
                circleTexture.Apply();

                Sprite circleSprite = Sprite.Create(circleTexture, new Rect(0, 0, textureSize, textureSize), new Vector2(0.5f, 0.5f), 100f);

                radiusSpriteInstance = new GameObject("Radius Sprite");
                SpriteRenderer spriteRenderer = radiusSpriteInstance.AddComponent<SpriteRenderer>();
                spriteRenderer.sprite = circleSprite;
                spriteRenderer.sortingOrder = -1;

                radiusSpriteInstance.transform.position = new Vector3(transform.position.x, editorPlaneHeight + planeHeightOffset, transform.position.z);
                radiusSpriteInstance.transform.rotation = Quaternion.Euler(90f, 0f, 0f);

                float baseSpriteWorldDiameter = textureSize / 100f;
                float desiredWorldDiameter = radius * 2f;
                float scaleFactor = desiredWorldDiameter / baseSpriteWorldDiameter;

                radiusSpriteInstance.transform.localScale = new Vector3(scaleFactor, scaleFactor, 1f);

                if (Application.isEditor)
                {
                    radiusSpriteInstance.hideFlags = HideFlags.HideAndDontSave | HideFlags.HideInHierarchy;
                }


                radiusSpriteInstance.SetActive(true);
            }
            else
            {
                radiusSpriteInstance.SetActive(true);
                radiusSpriteInstance.transform.position = new Vector3(transform.position.x, editorPlaneHeight + planeHeightOffset, transform.position.z);
                SpriteRenderer spriteRenderer = radiusSpriteInstance.GetComponent<SpriteRenderer>();
                if (spriteRenderer != null)
                {
                    Color spriteColor = radiusColor;
                    spriteColor.a = radiusTransparency;
                    spriteRenderer.color = spriteColor;
                }
            }
        }
        else
        {
            if (radiusSpriteInstance != null)
            {
                radiusSpriteInstance.SetActive(false);
            }
        }
    }

    public void HideRadiusVisualization()
    {
        if (radiusSpriteInstance != null)
        {
            DestroyImmediate(radiusSpriteInstance);
        }
    }
}