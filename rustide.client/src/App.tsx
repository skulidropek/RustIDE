import React, { useState, useCallback } from 'react';
import './App.css';
import CodeEditor from './components/CodeEditor/CodeEditor';
import ChatWindow from './components/ChatWindow/ChatWindow';
import ConsoleOutput from './components/ConsoleOutput/ConsoleOutput';
import Resizer from './components/Resizer/Resizer';

const App: React.FC = () => {
    const [chatWidth, setChatWidth] = useState(400);
    const [consoleHeight, setConsoleHeight] = useState(200);
    const [consoleOutput, setConsoleOutput] = useState<string[]>([]);

    const handleHorizontalResize = useCallback((newSize: number) => {
        setChatWidth(Math.max(200, Math.min(newSize, window.innerWidth - 205)));
    }, []);

    const handleVerticalResize = useCallback((newSize: number) => {
        setConsoleHeight(Math.max(100, Math.min(newSize, window.innerHeight - 100)));
    }, []);

    const handleCodeExecution = useCallback((output: string) => {
        setConsoleOutput(prev => [...prev, output]);
    }, []);

    return (
        <div className="app-container">
            <div className="chat-container" style={{ width: chatWidth }}>
                <ChatWindow />
            </div>
            <Resizer onResize={handleHorizontalResize} orientation="vertical" />
            <div className="right-panel">
                <div className="editor-container" style={{ height: `calc(100% - ${consoleHeight}px)` }}>
                    <CodeEditor />
                </div>
                <Resizer onResize={handleVerticalResize} orientation="horizontal" />
                <div className="console-container" style={{ height: consoleHeight }}>
                    <ConsoleOutput output={consoleOutput} />
                </div>
            </div>
        </div>
    );
};

export default App;