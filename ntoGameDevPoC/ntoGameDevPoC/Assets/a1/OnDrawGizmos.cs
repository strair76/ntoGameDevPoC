using UnityEngine;
public class DrawWireCollider : MonoBehaviour {
    void OnDrawGizmos() {
        if (TryGetComponent<Collider>(out var c)) {
            Gizmos.color = Color.green;
            Gizmos.matrix = transform.localToWorldMatrix;
            // Пример для Box
            if (c is BoxCollider bc) Gizmos.DrawWireCube(bc.center, bc.size);
        }
    }
}
