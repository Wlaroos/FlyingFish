using UnityEngine;
using UnityEditor;

[CustomPropertyDrawer(typeof(MinMaxRangeAttribute))]
public class MinMaxDrawer : PropertyDrawer
{
    public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
    {
        // Ensure the property is a Vector2 or Vector2Int before continuing
        if (property.propertyType == SerializedPropertyType.Vector2)
        {
            MinMaxRangeAttribute attr = (MinMaxRangeAttribute)attribute;
            Vector2 range = property.vector2Value;

            float min = range.x;
            float max = range.y;

            // Draw the default label prefix
            position = EditorGUI.PrefixLabel(position, GUIUtility.GetControlID(FocusType.Passive), label);

            // Calculate split rectangles
            float fieldWidth = 50f;
            float padding = 5f;
            float sliderWidth = position.width - (fieldWidth * 2) - (padding * 2);

            Rect minFieldRect = new Rect(position.x, position.y, fieldWidth, position.height);
            Rect sliderRect = new Rect(position.x + fieldWidth + padding, position.y, sliderWidth, position.height);
            Rect maxFieldRect = new Rect(position.x + fieldWidth + padding + sliderWidth + padding, position.y, fieldWidth, position.height);

            // Draw and handle manual minimum value input
            min = EditorGUI.FloatField(minFieldRect, min);

            // Draw and handle the native MinMaxSlider control
            EditorGUI.MinMaxSlider(sliderRect, ref min, ref max, attr.minLimit, attr.maxLimit);

            // Draw and handle manual maximum value input
            max = EditorGUI.FloatField(maxFieldRect, max);

            // Enforce constraints
            min = Mathf.Clamp(min, attr.minLimit, max);
            max = Mathf.Clamp(max, min, attr.maxLimit);

            // Save values back to the underlying serialized property
            property.vector2Value = new Vector2(min, max);
        }
        else
        {
            // Fallback display message
            EditorGUI.LabelField(position, label.text, "Use [MinMax] with Vector2 fields only.");
        }
    }
}
