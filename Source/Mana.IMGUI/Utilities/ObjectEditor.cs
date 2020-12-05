using System.Collections.Generic;
using Mana.Utilities;

namespace Mana.IMGUI.Utilities
{
    public class ObjectEditor
    {
        private Dictionary<string, IRef> _properties;

        internal ObjectEditor(Dictionary<string, IRef> properties)
        {
            _properties = properties;
        }

        public static FluentObjectEditorBuilder Create()
        {
            return new FluentObjectEditorBuilder();
        }

        public void DrawGUI()
        {
            foreach (var kvp in _properties)
            {
                PropertyEditorHelper.EditRef(kvp.Key, kvp.Value);
            }
        }
    }
}
