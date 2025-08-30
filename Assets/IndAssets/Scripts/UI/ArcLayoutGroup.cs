using UnityEngine;
using UnityEngine.UI;

namespace ProjectCI.Runtime.GUI.Battle
{
    [ExecuteAlways]
    [AddComponentMenu("UI/Layouts/Arc Layout Group")]
    public class ArcLayoutGroup : LayoutGroup
    {
        [Header("Arc Settings")]
        [Min(0.01f)] public float radius = 220f;            // Arc radius (UI pixels)
        [Range(0f, 360f)] public float arcDegrees = 40f;    // Interval angle between elements (degrees)
        [Range(-360f, 360f)] public float startAngle;       // Start angle (degrees), 0=right, 90=up, 180=left, 270=down
        public bool centered = true;                        // Center spread (true: centered on startAngle)
        public bool clockwise = true;                       // Clockwise/counterclockwise

        [Header("Item Options")]
        public bool alignToTangent = true;                  // Rotate children along tangent
        public Vector2 centerOffset = Vector2.zero;         // Arc center offset in this Rect
        public float itemAngleOffset;                       // Extra Z rotation for each item
        [Range(0f, 10f)] public float rootSpacing = 1f;     // Root spacing multiplier (0=overlap, 1=normal, 2=double spacing)

        // Required LayoutGroup implementations
        public override void CalculateLayoutInputHorizontal()
        {
            base.CalculateLayoutInputHorizontal();
            SetLayout(); // Our layout logic
        }

        public override void CalculateLayoutInputVertical()
        {
            // No extra size calculation needed, handled by parent Rect or Content Size Fitter
        }

        public override void SetLayoutHorizontal() { /* Position set in SetLayout() */ }
        public override void SetLayoutVertical() { /* Position set in SetLayout() */ }

        protected override void OnEnable()
        {
            base.OnEnable();
            SetDirty();
        }

        protected override void OnRectTransformDimensionsChange()
        {
            base.OnRectTransformDimensionsChange();
            SetDirty();
        }

        protected override void OnTransformChildrenChanged()
        {
            base.OnTransformChildrenChanged();
            SetDirty();
        }

        void SetLayout()
        {
            if (!IsActive()) return;

            Debug.Log("Update ArcLayoutGroup");

            // Count valid children (ignore disabled ones)
            var currentRectChildren = rectChildren; // LayoutGroup's built-in child list (only layoutable ones)
            int count = currentRectChildren.Count;
            if (count == 0) return;

            float dir = clockwise ? -1f : 1f;

            // Calculate total arc range based on interval angle
            float totalArcDegrees = (count <= 1) ? 0f : (arcDegrees * (count - 1));
            float step = arcDegrees; // Fixed interval between elements
            float start = centered
                ? (startAngle - dir * totalArcDegrees * 0.5f)
                : startAngle;

            // Arc center (use local coordinate system)
            Vector2 center = centerOffset;

            for (int i = 0; i < count; i++)
            {
                RectTransform child = currentRectChildren[i];

                // Angle (degrees to radians)
                float angleDeg = start + dir * (step * i);
                float rad = angleDeg * Mathf.Deg2Rad;

                // Calculate root position (on the arc)
                Vector2 rootPos = center + new Vector2(Mathf.Cos(rad), Mathf.Sin(rad)) * radius;

                // Calculate element center position (root position + half element length outward with spacing)
                float elementLength = child.rect.size.x; // Assume main length is on X axis
                Vector2 elementCenterPos = rootPos +
                                           new Vector2(Mathf.Cos(rad), Mathf.Sin(rad)) *
                                           (elementLength * 0.5f * rootSpacing);

                // Set anchor/pivot to center for intuitive control
                child.anchorMin = child.anchorMax = new Vector2(0.5f, 0.5f);
                child.pivot = new Vector2(0.5f, 0.5f);

                // Use localPosition instead of SetChildAlongAxis
                child.localPosition = elementCenterPos;

                if (alignToTangent)
                {
                    // For petal effect: each item points outward from arc center
                    float rotZ = angleDeg + itemAngleOffset;
                    child.localRotation = Quaternion.Euler(0f, 0f, rotZ);
                }
                else
                {
                    child.localRotation = Quaternion.Euler(0f, 0f, itemAngleOffset);
                }

                // Let LayoutGroup's padding/child alignment work (optional)
                // We don't change child size here; set child.SetSizeWithCurrentAnchors(...) if uniform size needed
            }
        }
    }
}