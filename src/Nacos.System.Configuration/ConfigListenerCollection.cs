namespace Nacos.System.Configuration
{
    using global::System.Configuration;

    public class ConfigListenerCollection : ConfigurationElementCollection
    {
        public ConfigListenerCollection() => AddElementName = "listener";

        /// <summary>
        /// Gets or sets the <see cref="ConfigListener"/> at the specified index.
        /// </summary>
        /// <value>
        /// The <see cref="ConfigListener"/>.
        /// </value>
        /// <param name="index">The index.</param>
        public ConfigListener this[int index]
        {
            get => BaseGet(index) as ConfigListener;
            set
            {
                if (BaseGet(index) != null) BaseRemoveAt(index);

                BaseAdd(index, value);
            }
        }

        /// <summary>
        /// Creates the new element.
        /// </summary>
        protected override ConfigurationElement CreateNewElement() => new ConfigListener();

        /// <summary>
        /// Gets the element key.
        /// </summary>
        /// <param name="element">The element.</param>
        protected override object GetElementKey(ConfigurationElement element)
            => $"{((ConfigListener)element).Group}#{((ConfigListener)element).DataId}";
    }
}