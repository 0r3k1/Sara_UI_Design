using System.ComponentModel;

namespace Sara_UI_Design.SaraControls {
    /// <summary>
    /// Proporciona al diseñador de propiedades la lista de nombres canónicos
    /// disponibles en <see cref="SaraUI_IconLibrary"/>.
    /// </summary>
    public class IconNameConverter:StringConverter {
        /// <inheritdoc/>
        public override bool GetStandardValuesSupported(ITypeDescriptorContext? context) => true;

        /// <inheritdoc/>
        public override bool GetStandardValuesExclusive(ITypeDescriptorContext? context) => false;

        /// <inheritdoc/>
        public override StandardValuesCollection GetStandardValues(ITypeDescriptorContext? context) {
            var icons = SaraUI_IconLibrary.GetAvailableIcons();
            icons.Insert(0, "None");
            return new StandardValuesCollection(icons);
        }
    }
}
