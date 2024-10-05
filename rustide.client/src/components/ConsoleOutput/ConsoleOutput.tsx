import React from 'react';
import './ConsoleOutput.css';

interface ConsoleOutputProps {
    output: string[];
}

const ConsoleOutput: React.FC<ConsoleOutputProps> = ({ output }) => {
    return (
        <div className="console-output">
            <div className="console-header">Console Output</div>
            <div className="console-content">
                {output.map((line, index) => (
                    <div key={index} className="console-line">{line}</div>
                ))}
            </div>
        </div>
    );
};

export default ConsoleOutput;
