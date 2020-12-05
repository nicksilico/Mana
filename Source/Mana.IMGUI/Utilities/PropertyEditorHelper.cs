using System;
using System.Collections.Generic;
using System.Numerics;
using ImGuiNET;
using Mana.Graphics;
using Mana.Utilities;

namespace Mana.IMGUI.Utilities
{
    public static class PropertyEditorHelper
    {
        private static Dictionary<Type, IPropertyEditor> _customEditorsByType = new Dictionary<Type,IPropertyEditor>();

        public static void RegisterTypeHandler(Type type, IPropertyEditor editor)
        {
            _customEditorsByType.Add(type, editor);
        }

        public static bool EditRef(string label, IRef reference)
        {
            switch (reference)
            {
                case Ref<int> intRef:
                {
                    int old = intRef.Value;
                    bool ret = ImGui.DragInt(label, ref old);
                    intRef.Value = old;
                    return ret;
                }
                case Ref<float> floatRef:
                {
                    float old = floatRef.Value;
                    bool ret = ImGui.DragFloat(label, ref old);
                    floatRef.Value = old;
                    return ret;
                }
                case Ref<bool> boolRef:
                {
                    bool old = boolRef.Value;
                    bool ret = ImGui.Checkbox(label, ref old);
                    boolRef.Value = old;
                    return ret;
                }
                case Ref<string> stringRef:
                {
                    string old = stringRef.Value;
                    bool ret = ImGui.InputText(label, ref old, 128);
                    stringRef.Value = old;
                    return ret;
                }
                case Ref<Vector2> vector2Ref:
                {
                    Vector2 old = vector2Ref.Value;
                    bool ret = ImGui.DragFloat2(label, ref old);
                    vector2Ref.Value = old;
                    return ret;
                }
                case Ref<Vector3> vector3Ref:
                {
                    Vector3 old = vector3Ref.Value;
                    bool ret = ImGui.DragFloat3(label, ref old);
                    vector3Ref.Value = old;
                    return ret;
                }
                case Ref<Quaternion> quaternionRef:
                {
                    Quaternion q = quaternionRef.Value;
                    Vector4 old = new Vector4(q.X, q.Y, q.Z, q.W);
                    bool ret = ImGui.DragFloat4(label, ref old);
                    quaternionRef.Value = new Quaternion(old.X, old.Y, old.Z, old.W);
                    return ret;
                }
                case Ref<Color> colorRef:
                {
                    Vector4 old = colorRef.Value.ToVector4();
                    bool ret = ImGui.ColorEdit4(label, ref old);
                    colorRef.Value = Color.FromVector4(old);
                    return ret;
                }
            }

            if (_customEditorsByType.TryGetValue(reference.TypeOfValue, out IPropertyEditor editor))
            {
                return editor.Edit(label, reference);
            }

            throw new InvalidOperationException($"No registered editor for type {reference.TypeOfValue.Name} exists.");
        }
    }

    public interface IPropertyEditor
    {
        bool Edit(string label, IRef reference);
    }
}
