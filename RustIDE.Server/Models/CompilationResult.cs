namespace RustIDE.Server.Models
{
    public class CompilationError
    {
        public int StartLine { get; set; }    // Номер строки, где начинается ошибка
        public int StartColumn { get; set; }  // Номер столбца, где начинается ошибка
        public int EndLine { get; set; }      // Номер строки, где заканчивается ошибка
        public int EndColumn { get; set; }    // Номер столбца, где заканчивается ошибка
        public string Message { get; set; }   // Сообщение об ошибке
        public string Severity { get; set; }  // Уровень ошибки (Error, Warning, Info)
    }

    // Модель для результата компиляции
    public class CompilationResult
    {
        public bool Success { get; set; }   // Успешна ли компиляция
        public string Output { get; set; }  // Вывод программы
        public List<CompilationError> Errors { get; set; } = new List<CompilationError>(); // Список ошибок
    }
}
