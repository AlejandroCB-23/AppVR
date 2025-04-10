using UnityEngine;

public class AutoColliderFitter : MonoBehaviour
{
    [ContextMenu("Fit BoxCollider to Children")]
    void FitBoxCollider()
    {
        BoxCollider boxCollider = GetComponent<BoxCollider>();
        if (boxCollider == null)
            boxCollider = gameObject.AddComponent<BoxCollider>();

        Bounds bounds = new Bounds(transform.position, Vector3.zero);
        foreach (Renderer renderer in GetComponentsInChildren<Renderer>())
        {
            bounds.Encapsulate(renderer.bounds);
        }

        boxCollider.center = transform.InverseTransformPoint(bounds.center);
        boxCollider.size = bounds.size;
    }
}

