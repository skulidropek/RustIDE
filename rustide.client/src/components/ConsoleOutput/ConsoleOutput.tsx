import React from 'react';
import { CompilationResult, CompilationError } from '../../api-client'; // Убедитесь, что путь правильный
import './ConsoleOutput.css';

interface ConsoleOutputProps {
    results: CompilationResult[];
}

const ConsoleOutput: React.FC<ConsoleOutputProps> = ({ results }) => {
    return (
        <div className="console-output">
            <div className="console-header">Console Output</div>
            <div className="console-content">
                {results.map((result, index) => (
                    <div key={index} className={`result ${result.success ? 'success' : 'error'}`}>
                        {result.success ? (
                            <pre>{result.output}</pre>
                        ) : (
                            <div>
                                <p>Compilation failed:</p>
                                {result.errors?.map((error: CompilationError, errorIndex: number) => (
                                    <pre key={errorIndex} className="error-message">
                                        Line {error.startLine}, Column {error.startColumn}: {error.message}
                                    </pre>
                                ))}
                            </div>
                        )}
                    </div>
                ))}
            </div>
        </div>
    );
};

export default ConsoleOutput;
