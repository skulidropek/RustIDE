import React, { useState, useEffect, useRef } from 'react';
import MonacoEditor from 'react-monaco-editor';
import * as monaco from 'monaco-editor';
import axios from 'axios';
import './CodeEditor.css';
import { SyntaxConfig, TokenRule, TokenAction } from '../models/SyntaxConfig';
import { CompilationError, CompilationResult } from '../models/CompilationResult';

const CodeEditor: React.FC = () => {
  const [code, setCode] = useState(`using Oxide.Core.Plugins;
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

  const [errors, setErrors] = useState<CompilationError[]>([]);
  const [output, setOutput] = useState<string>('');
  const [syntaxConfig, setSyntaxConfig] = useState<SyntaxConfig | null>(null);
  const editorRef = useRef<monaco.editor.IStandaloneCodeEditor | null>(null);

  const onChange = (newValue: string) => {
    setCode(newValue);
  };

  const loadSyntaxConfig = async () => {
    try {
      const response = await axios.get<SyntaxConfig>('https://localhost:7214/api/editorconfig/syntax');
      setSyntaxConfig(response.data);
      console.log('Loaded syntax config:', response.data);
    } catch (error) {
      console.error('Error loading syntax config:', error);
    }
  };

  const checkCode = async () => {
    try {
      const response = await axios.post<CompilationResult>('https://localhost:7214/api/code/compile', { code });
      const result = response.data;

      if (result.success) {
        setErrors([]);
        setOutput(result.output);
        if (editorRef.current) {
          monaco.editor.setModelMarkers(editorRef.current.getModel()!, 'owner', []);
        }
      } else {
        setOutput('');
        setErrors(result.errors);
        addMarkers(result.errors);
      }
    } catch (error) {
      console.error('Error during compilation:', error);
      setErrors([{
        startLine: 0, startColumn: 0, endLine: 0, endColumn: 0,
        message: 'Failed to connect to the server.', severity: 'Error'
      }]);
      setOutput('');
    }
  };

  const addMarkers = (errors: CompilationError[]) => {
    if (editorRef.current) {
      const model = editorRef.current.getModel();
      if (model) {
        const markers = errors.map((error) => ({
          startLineNumber: error.startLine,
          startColumn: error.startColumn,
          endLineNumber: error.endLine,
          endColumn: error.endColumn,
          message: error.message,
          severity: monaco.MarkerSeverity.Error,
        }));

        monaco.editor.setModelMarkers(model, 'owner', markers);
      }
    }
  };

  useEffect(() => {
    loadSyntaxConfig();
  }, []);

  useEffect(() => {
    if (syntaxConfig) {
      console.log('Configuring Monaco Editor with syntax config:', syntaxConfig);
      
      monaco.languages.register({ id: 'csharp' });

      const convertTokenRules = (rules: TokenRule[]): monaco.languages.IMonarchLanguageRule[] => {
        return rules.map(rule => ({
          regex: rule.regex,
          action: rule.action ? convertTokenAction(rule.action) : '',
          include: rule.include
        }));
      };

      const convertTokenAction = (action: TokenAction): string | monaco.languages.IMonarchLanguageAction => {
        if (action === null || action === undefined) {
          console.warn('Received null or undefined TokenAction');
          return '';
        }

        if (typeof action === 'string') {
          return action;
        }

        if (typeof action === 'object') {
          if (action.token && !action.next && !action.cases) {
            return action.token;
          }
          return {
            token: action.token || '',
            next: action.next,
            cases: action.cases
          };
        }

        console.warn('Unexpected TokenAction format:', action);
        return '';
      };

      const tokenizer: { [key: string]: monaco.languages.IMonarchLanguageRule[] } = {};
      for (const [key, rules] of Object.entries(syntaxConfig.tokenizer)) {
        if (Array.isArray(rules)) {
          tokenizer[key] = convertTokenRules(rules);
        } else {
          console.warn(`Unexpected format for tokenizer rule set '${key}':`, rules);
        }
      }

      monaco.languages.setMonarchTokensProvider('csharp', {
        defaultToken: '',
        tokenPostfix: '.cs',
        keywords: syntaxConfig.keywords,
        operators: syntaxConfig.operators,
        symbols: /[=><!~?:&|+\-*\/\^%]+/,
        escapes: /\\(?:[abfnrtv\\"']|x[0-9A-Fa-f]{1,4}|u[0-9A-Fa-f]{4}|U[0-9A-Fa-f]{8})/,
        tokenizer: tokenizer
      });

      monaco.languages.setLanguageConfiguration('csharp', {
        comments: {
          lineComment: syntaxConfig.comments.lineComment,
          blockComment: syntaxConfig.comments.blockComment as [string, string]
        },
        brackets: syntaxConfig.brackets.map(b => [b.open, b.close] as [string, string]),
        autoClosingPairs: [
          { open: '{', close: '}' },
          { open: '[', close: ']' },
          { open: '(', close: ')' },
          { open: '"', close: '"', notIn: ['string'] },
          { open: "'", close: "'", notIn: ['string', 'comment'] }
        ],
        surroundingPairs: [
          { open: '{', close: '}' },
          { open: '[', close: ']' },
          { open: '(', close: ')' },
          { open: '"', close: '"' },
          { open: "'", close: "'" }
        ]
      });

      // Добавляем провайдер автодополнения
      monaco.languages.registerCompletionItemProvider('csharp', {
        triggerCharacters: ['.'],
        provideCompletionItems: async (model, position) => {
          const wordUntilPosition = model.getWordUntilPosition(position);
          const word = wordUntilPosition.word;
      
          try {
            const response = await axios.post('https://localhost:7214/api/completion', {
              code: model.getValue(),
              position: model.getOffsetAt(position)
            });
      
            const suggestions = response.data.map((item: any) => ({
              label: item.label,
              kind: monaco.languages.CompletionItemKind[item.kind],
              insertText: item.kind === 'Method' ? `${item.insertText}()` : item.insertText, // Если метод, добавляем скобки
              detail: item.detail,
              range: {
                startLineNumber: position.lineNumber,
                endLineNumber: position.lineNumber,
                startColumn: wordUntilPosition.startColumn,
                endColumn: wordUntilPosition.endColumn
              }
            }));
      
            return { suggestions };
          } catch (error) {
            console.error('Error fetching completions:', error);
            return { suggestions: [] };
          }
        }
      });

      if (editorRef.current) {
        editorRef.current.addCommand(
          monaco.KeyMod.CtrlCmd | monaco.KeyCode.KeyS,
          () => {
            checkCode();
          }
        );
      }

      console.log('Monaco Editor configuration complete');
    }
  }, [syntaxConfig]);

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
          }}
        />
      </div>
      <button onClick={checkCode} className="button">Check Code</button>

      {output && (
        <div className="outputContainer">
          <h4>Program Output:</h4>
          <pre>{output}</pre>
        </div>
      )}

      {errors.length > 0 && (
        <div className="errorContainer">
          <h4>Compilation Errors:</h4>
          <ul>
            {errors.map((error, index) => (
              <li key={index}>{error.message} (Line {error.startLine}, Column {error.startColumn})</li>
            ))}
          </ul>
        </div>
      )}
    </div>
  );
};

export default CodeEditor;