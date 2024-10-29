// CodeEditor.tsx
import React from 'react';
import MonacoEditor from 'react-monaco-editor';
import * as monaco from 'monaco-editor';
import './CodeEditor.css';
import { useCodeEditorLogic } from './useCodeEditorLogic';
import { CompilationResult } from '../../api-client';

interface CodeEditorProps {
    code: string;
    onCodeChange: (newCode: string) => void;
    onExecute: (result: CompilationResult) => void;
}

const CodeEditor: React.FC<CodeEditorProps> = ({ code, onCodeChange, onExecute }) => {
    const { editorRef, onChange, checkCode } = useCodeEditorLogic(onCodeChange, onExecute);

    const options: monaco.editor.IStandaloneEditorConstructionOptions = {
        selectOnLineNumbers: true,
        automaticLayout: true,
        minimap: { enabled: false },
        scrollBeyondLastLine: false,
        fontSize: 14,
        wordWrap: 'on',
    };

    return (
        <div className="container">
            <div className="editorContainer">
                <MonacoEditor
                    language="csharp"
                    theme="vs-dark"
                    value={code}
                    options={options}
                    onChange={onChange}
                    editorDidMount={(editor) => {
                        editorRef.current = editor;
                        checkCode(code);
                    }}
                />
            </div>
            <button onClick={() => checkCode(code)} className="button">Check Code</button>
        </div>
    );
};

export default CodeEditor;
