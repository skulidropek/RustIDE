import React, { useState, useCallback } from 'react';
import './App.css';
import CodeEditor from './components/CodeEditor/CodeEditor';
import ChatWindow from './components/ChatWindow/ChatWindow';
import ConsoleOutput from './components/ConsoleOutput/ConsoleOutput';
import Resizer from './components/Resizer/Resizer';
import { CompilationResult } from './api-client'; // Убедитесь, что путь к api-client правильный

const App: React.FC = () => {
    const [chatWidth, setChatWidth] = useState(400);
    const [consoleHeight, setConsoleHeight] = useState(200);
    const [consoleOutput, setConsoleOutput] = useState<CompilationResult[]>([]);
    const [code, setCode] = useState<string>(`using Oxide.Core.Plugins;
using Oxide.Core.Libraries.Covalence;

namespace Oxide.Plugins
{
    [Info("ExamplePlugin", "YourName", "1.0.0")]
    [Description("A simple plugin for Rust.")]
    public class ExamplePlugin : CovalencePlugin
    {
        private void Init()
        {
            Puts("Example plugin has been loaded!");
        }

        [Command("example")]
        private void ExampleCommand(IPlayer player, string command, string[] args)
        {
            player.Reply("You just used the /example command!");
        }

        private void OnPlayerDeath(BasePlayer player, HitInfo info)
        {
            Puts($"{player.displayName} has been killed.");
        }
    }
}`);

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

    const handleCodeExecution = useCallback((result: CompilationResult) => {
        setConsoleOutput([result]);
    }, []);

    const handleCodeChange = useCallback((newCode: string) => {
        setCode(newCode);
    }, []);

    return (
        <div className="app-container">
            <div className="chat-container" style={{ width: chatWidth }}>
                <ChatWindow code={code} />
            </div>
            <Resizer onResize={handleHorizontalResize} orientation="vertical" />
            <div className="right-panel" style={{ width: `calc(100% - ${chatWidth}px)` }}>
                <div className="editor-container" style={{ height: `calc(100% - ${consoleHeight}px)` }}>
                    <CodeEditor 
                        code={code} 
                        onCodeChange={handleCodeChange} 
                        onExecute={handleCodeExecution} 
                    />
                </div>
                <Resizer onResize={handleVerticalResize} orientation="horizontal" />
                <div className="console-container" style={{ height: consoleHeight }}>
                    <ConsoleOutput results={consoleOutput} />
                </div>
            </div>
        </div>
    );
};

export default App;