namespace Nacos.V2.Naming.Remote.Grpc
{
    public enum RedoType
    {
        /// <summary>
        /// Redo register.
        /// </summary>
        REGISTER,

        /// <summary>
        /// Redo unregister.
        /// </summary>
        UNREGISTER,

        /// <summary>
        /// Redo nothing.
        /// </summary>
        NONE,

        /// <summary>
        /// Remove redo data.
        /// </summary>
        REMOVE
    }
}
