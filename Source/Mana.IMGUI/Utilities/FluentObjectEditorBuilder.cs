using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using Mana.Utilities;

namespace Mana.IMGUI.Utilities
{
    public class FluentObjectEditorBuilder
    {
        private Dictionary<string, IRef> _properties = new Dictionary<string, IRef>();

        public FluentObjectEditorBuilder With<T>(string name, Expression<Func<T>> getter)
        {
            _properties.Add(name, Ref<T>.Of(getter));
            return this;
        }

        public ObjectEditor Build()
        {
            return new ObjectEditor(_properties);
        }
    }
}
