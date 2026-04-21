using System;
using System.ComponentModel;
using System.Linq;
using System.Reflection;

namespace Sara_UI_Design.SaraControls {
    public class IconNameConverter:StringConverter {
        public override bool GetStandardValuesSupported(ITypeDescriptorContext context) => true;

        // Si es false, te deja ESCRIBIR el nombre para buscarlo rápido
        public override bool GetStandardValuesExclusive(ITypeDescriptorContext context) => false;

        public override StandardValuesCollection GetStandardValues(ITypeDescriptorContext context) {
            var methods = typeof(SaraUI_IconLibrary).GetMethods(BindingFlags.Public | BindingFlags.Static)
                .Where(m => m.Name.StartsWith("Draw") && m.Name != "DrawIcon")
                .Select(m => m.Name.Replace("Draw", ""))
                .OrderBy(n => n)
                .ToList();

            methods.Insert(0, "None");
            return new StandardValuesCollection(methods);
        }
    }
}