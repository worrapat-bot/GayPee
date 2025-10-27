using UnityEngine;
using UnityEditor; // ต้องมีสำหรับ PropertyDrawer
using UnityEngine.PostProcessing;
// ไม่จำเป็นต้องเพิ่ม System.Collections หากไม่ได้ใช้งาน

namespace UnityEditor.PostProcessing
{
    // ✅ แก้ไข: ระบุ Namespace ของ MinAttribute ให้ชัดเจน (ใช้ตัวจาก PostProcessing)
    [CustomPropertyDrawer(typeof(UnityEngine.PostProcessing.MinAttribute))]
    sealed class MinDrawer : PropertyDrawer
    {
        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            // 💡 Cast Attribute ไปยังคลาสที่ถูกต้อง
            UnityEngine.PostProcessing.MinAttribute minAttr = (UnityEngine.PostProcessing.MinAttribute)attribute;
            
            // ใช้ PropertyField เพื่อแสดงผลค่า
            EditorGUI.PropertyField(position, property, label);
            
            // ตรวจสอบและจำกัดค่า (Clamp) ให้ไม่ต่ำกว่าค่า Min
            if (property.propertyType == SerializedPropertyType.Float)
            {
                property.floatValue = Mathf.Max(property.floatValue, minAttr.min);
            }
            else if (property.propertyType == SerializedPropertyType.Integer)
            {
                property.intValue = Mathf.Max(property.intValue, (int)minAttr.min);
            }
        }
        
        public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
        {
            // ใช้ความสูงมาตรฐานสำหรับ Property Drawer
            return EditorGUI.GetPropertyHeight(property);
        }
    }
}