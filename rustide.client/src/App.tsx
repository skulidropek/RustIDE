import React, { useState, useCallback, useEffect } from 'react';
import './App.css';
import CodeEditor from './components/CodeEditor/CodeEditor';
import ChatWindow from './components/ChatWindow/ChatWindow';
import ConsoleOutput from './components/ConsoleOutput/ConsoleOutput';
import Resizer from './components/Resizer/Resizer';

const App: React.FC = () => {
    const [chatWidth, setChatWidth] = useState(400);
    const [consoleHeight, setConsoleHeight] = useState(200);
    const [consoleOutput, setConsoleOutput] = useState<string[]>([]);

    const handleHorizontalResize = useCallback((deltaX: number) => {
        setChatWidth(prevWidth => {
            const minChatWidth = 400;
            const minRightPanelWidth = 400;
            const maxChatWidth = window.innerWidth - minRightPanelWidth;
            return Math.max(minChatWidth, Math.min(prevWidth + deltaX, maxChatWidth));
        });
    }, []);

    const handleVerticalResize = useCallback((deltaY: number) => {
        setConsoleHeight(prevHeight => {
            const minConsoleHeight = 100;
            const maxConsoleHeight = window.innerHeight - 100;
            return Math.max(minConsoleHeight, Math.min(prevHeight + deltaY, maxConsoleHeight));
        });
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
            <div className="right-panel" style={{ width: `calc(100% - ${chatWidth}px)` }}>
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