  export interface CompilationError {
    startLine: number;  // Номер строки, где начинается ошибка
    startColumn: number;  // Номер столбца, где начинается ошибка
    endLine: number;  // Номер строки, где заканчивается ошибка
    endColumn: number;  // Номер столбца, где заканчивается ошибка
    message: string;  // Сообщение об ошибке
    severity: string;  // Уровень ошибки (Error, Warning, Info)
  }
  
  // Модель для результата компиляции
  export interface CompilationResult {
    success: boolean;  // Успешна ли компиляция
    output: string;  // Вывод программы
    errors: CompilationError[];  // Список ошибок
  }
  