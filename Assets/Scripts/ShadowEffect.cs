using UnityEngine;

public class ShadowEffect : MonoBehaviour
{
    GameObject shadowObject;
    [SerializeField] Material shadowMat;
    [SerializeField] Vector3 Offset = new Vector3(-0.1f, -0.1f);

    private void OnEnable()
    {
        shadowObject = new GameObject(gameObject.name + "Shadow");
        shadowObject.transform.parent = transform;

        shadowObject.transform.localPosition = Offset;
        shadowObject.transform.localRotation = Quaternion.identity;
        shadowObject.transform.localScale = Vector3.one;

        SpriteRenderer renderer = this.GetComponent<SpriteRenderer>();
        SpriteRenderer shadowRenderer = shadowObject.AddComponent<SpriteRenderer>();
        shadowRenderer.sprite = renderer.sprite;
        shadowRenderer.material = shadowMat;

        shadowRenderer.sortingLayerName = renderer.sortingLayerName;
        shadowRenderer.sortingOrder = renderer.sortingOrder - 1;
    }

    private void LateUpdate()
    {
        shadowObject.transform.localPosition = Offset;
    }
}
