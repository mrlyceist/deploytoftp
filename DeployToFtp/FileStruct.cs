using System;

namespace DeployToFtp
{
    /// <summary>
    /// Структура для хранения аттрибутов файла
    /// </summary>
    public struct FileStruct
    {
        /// <summary>
        /// Строка, отвечающая за флаги разрешений файла. См. <seealso cref="ResponseParser.GetFlags"/>
        /// </summary>
        public string Flags;
        /// <summary>
        /// Поле, отвечающее на вопрос "Это что, папка, что ли?!"
        /// </summary>
        public bool IsDirectory;
        /// <summary>
        /// Время создания файла. Или его последнего изменения, я не знаю. Ну что там сервер отдает.
        /// </summary>
        public DateTime CreateTime;
        /// <summary>
        /// Имя файла, аллилуйя!
        /// </summary>
        public string Name;
    }
}