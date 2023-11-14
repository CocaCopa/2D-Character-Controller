using UnityEngine;

public class Test : MonoBehaviour {

    [SerializeField] private Transform circleTransform;
    [SerializeField] private bool circle;
    [SerializeField] private bool box;

    private void OnDrawGizmos() {
        if (circleTransform == null) {
            return;
        }
        Gizmos.color = Color.green;
        //Circle();
        //Box();
    }

    private void Circle() {
        if (circle) {
            box = false;
            Vector3 position = circleTransform.position;
            float radius = (circleTransform.lossyScale.x / 2 + circleTransform.lossyScale.y / 2) / 2;
            Gizmos.DrawWireSphere(position, radius);
        }
    }

    private void Box() {
        if (box) {
            circle = false;
            Vector3 position = circleTransform.position;
            Vector3 size = circleTransform.lossyScale;
            Gizmos.DrawWireCube(position, size);
        }
    }
}
