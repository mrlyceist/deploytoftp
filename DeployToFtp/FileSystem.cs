namespace DeployToFtp
{
    /// <summary>
    /// Перечисление возможных файловых систем на сервере.
    /// </summary>
    public enum FileSystem
    {
        /// <summary>
        /// Сервер крутится на юникс-подобной операционной системе.
        /// </summary>
        UnixStyle,
        /// <summary>
        /// Сервер крутится под форточками. Как бы не сдуло.
        /// </summary>
        WindowsStyle,
        /// <summary>
        /// Это что за неведома зверушка?!
        /// </summary>
        Unknown
    }
}