using UnityEngine;

namespace IndAssets.Scripts.AI
{
    public class PvMnAggroLine : MonoBehaviour
    {
        [SerializeField] private LineRenderer lineRenderer;
        [SerializeField] private int resolution = 20;
        [SerializeField] private float curveHeight = 2.0f;

        void Awake()
        {
            lineRenderer.positionCount = resolution;
            lineRenderer.enabled = false;
        }

        public void DrawCurve(Vector3 start, Vector3 end)
        {
            lineRenderer.enabled = true;

            Vector3 midPoint = (start + end) / 2f;
            Vector3 controlPoint = midPoint + Vector3.up * curveHeight;

            for (int i = 0; i < resolution; i++)
            {
                float t = i / (float)(resolution - 1);
                Vector3 point = CalculateBezierPoint(t, start, controlPoint, end);
                lineRenderer.SetPosition(i, point);
            }
        }

        public void Hide() => lineRenderer.enabled = false;

        private Vector3 CalculateBezierPoint(float t, Vector3 p0, Vector3 p1, Vector3 p2)
        {
            return Mathf.Pow(1 - t, 2) * p0 + 2 * (1 - t) * t * p1 + Mathf.Pow(t, 2) * p2;
        }
    }
}