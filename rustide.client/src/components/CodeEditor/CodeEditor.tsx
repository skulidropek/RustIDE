import React, { useState, useEffect, useRef, useCallback } from 'react';
import MonacoEditor from 'react-monaco-editor';
import * as monaco from 'monaco-editor';
import './CodeEditor.css';
import { Client, CodeRequest, CompilationResult, CompilationError, SyntaxConfig, CompletionRequest, CompletionItem } from '../../api-client';
import type { languages as monacoLanguages } from 'monaco-editor/esm/vs/editor/editor.api';

interface CodeEditorProps {
    onExecute: (result: CompilationResult) => void;
}

const CodeEditor: React.FC<CodeEditorProps> = ({ onExecute }) => {
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
            Puts($"\{player.displayName} has been killed.");
        }
    }
}`);
  
  const [syntaxConfig, setSyntaxConfig] = useState<SyntaxConfig | null>(null);
  const editorRef = useRef<monaco.editor.IStandaloneCodeEditor | null>(null);
  const client = new Client("https://localhost:7214");

  const checkCode = useCallback(async (codeToCheck: string) => {
    try {
      const request = new CodeRequest({ code: codeToCheck });
      const result: CompilationResult = await client.compile(request);
      onExecute(result);
      if (result.success) {
        if (editorRef.current) {
          showErrorsInEditor([], editorRef.current);
        }
      } else {
        console.error('Compilation errors:', result.errors);
        if (editorRef.current) {
          showErrorsInEditor(result.errors ?? [], editorRef.current);
        }
      }
    } catch (error) {
      const errorResult = new CompilationResult({
        success: false,
        errors: [new CompilationError({ 
          startLine: 0, 
          startColumn: 0, 
          endLine: 0, 
          endColumn: 0, 
          message: 'Failed to connect to the server.', 
          severity: 'Error' 
        })],
      });
      onExecute(errorResult);
      console.error('Failed to connect to the server:', error);
    }
  }, [onExecute]);

  const onChange = useCallback((newValue: string) => {
    setCode(newValue);
    checkCode(newValue);
  }, [checkCode]);

  const loadSyntaxConfig = async () => {
    try {
      const response = await client.syntax();
      setSyntaxConfig(response);
    } catch (error) {
      console.error('Failed to load syntax config:', error);
      onExecute(new CompilationResult({
        success: false,
        errors: [new CompilationError({ startLine: 0, startColumn: 0, endLine: 0, endColumn: 0, message: 'Failed to load syntax config.', severity: 'Error' })],
      }));
    }
  };

  const showErrorsInEditor = (errors: CompilationError[], editor: monaco.editor.IStandaloneCodeEditor) => {
    const model = editor.getModel();
    if (model) {
      const markers = errors.map(error => ({
        startLineNumber: error.startLine || 1,
        startColumn: error.startColumn || 1,
        endLineNumber: error.endLine || error.startLine || 1,
        endColumn: error.endColumn || error.startColumn || 1,
        message: error.message || 'Unknown error',
        severity: monaco.MarkerSeverity.Error,
      }));
      monaco.editor.setModelMarkers(model, 'owner', markers);
    }
  };

  useEffect(() => {
    if (syntaxConfig?.monarchLanguage && syntaxConfig?.languageConfiguration) {
      monaco.languages.register({ id: 'csharp' });
  
      const convertTokenAction = (action: any | string): monaco.languages.IMonarchLanguageAction => {
        if (typeof action === 'string') {
          return { token: action };
        }
        const result: monaco.languages.IMonarchLanguageAction = {};
        if (action.token) result.token = action.token;
        if (action.next) result.next = action.next;
        if (action.cases) result.cases = action.cases;
        return Object.keys(result).length === 0 ? { token: 'source' } : result;
      };
  
      const convertTokenRules = (rules: any[]): monaco.languages.IMonarchLanguageRule[] => {
        return rules.map(rule => {
          const convertedRule: monaco.languages.IMonarchLanguageRule = {
            regex: rule.regex!,
          };
          if (rule.action) {
            convertedRule.action = convertTokenAction(rule.action);
          }
          if (rule.include) {
            convertedRule.include = rule.include;
          }
          return convertedRule;
        });
      };
  
      const tokenizer: { [key: string]: monaco.languages.IMonarchLanguageRule[] } = {};
      if (syntaxConfig.monarchLanguage.tokenizer) {
        for (const [key, rules] of Object.entries(syntaxConfig.monarchLanguage.tokenizer)) {
          tokenizer[key] = convertTokenRules(rules as any);
        }
      }
  
      monaco.languages.setMonarchTokensProvider('csharp', {
        tokenizer,
        defaultToken: syntaxConfig.monarchLanguage.defaultToken || 'source',
        tokenPostfix: syntaxConfig.monarchLanguage.tokenPostfix || '.cs',
      });
  
      const comments: monaco.languages.CommentRule = {
        lineComment: syntaxConfig.languageConfiguration.comments?.lineComment,
        blockComment: syntaxConfig.languageConfiguration.comments?.blockComment
          ? [syntaxConfig.languageConfiguration.comments.blockComment.open!, syntaxConfig.languageConfiguration.comments.blockComment.close!]
          : undefined,
      };
  
      monaco.languages.setLanguageConfiguration('csharp', {
        comments: comments,
        brackets: syntaxConfig.languageConfiguration.brackets?.map(b => [b.open!, b.close!]) || [],
        autoClosingPairs: [
          { open: '{', close: '}' },
          { open: '[', close: ']' },
          { open: '(', close: ')' },
          { open: '"', close: '"' },
          { open: "'", close: "'" }
        ],
        surroundingPairs: [
          { open: '{', close: '}' },
          { open: '[', close: ']' },
          { open: '(', close: ')' },
          { open: '"', close: '"' },
          { open: "'", close: "'" }
        ]
      });

      monaco.languages.registerCompletionItemProvider('csharp', {
        triggerCharacters: ['.'],
        provideCompletionItems: async (model, position) : Promise<monaco.languages.CompletionList> => {
          const wordUntilPosition = model.getWordUntilPosition(position);
          
          try {
            const request = new CompletionRequest({
              code: model.getValue(),
              position: model.getOffsetAt(position),
            });
      
            const response = await client.completion(request);
      
            const suggestions = response.map((item: CompletionItem) => ({
              label: item.label ?? "",  
              kind: monaco.languages.CompletionItemKind[item.kind as keyof typeof monaco.languages.CompletionItemKind],
              insertText: item.insertText ?? "",
              detail: item.detail ?? "",
              documentation: item.documentation ?? "",
              commitCharacters: item.commitCharacters ?? [],
              insertTextRules: item.insertTextRules ? monaco.languages.CompletionItemInsertTextRule[item.insertTextRules as keyof typeof monaco.languages.CompletionItemInsertTextRule] : undefined,
              range: {
                  startLineNumber: position.lineNumber,
                  endLineNumber: position.lineNumber,
                  startColumn: wordUntilPosition.startColumn,
                  endColumn: wordUntilPosition.endColumn,
              }
          }));
          
          return { suggestions };
          
          } catch (error) {
            onExecute(new CompilationResult({
              success: false,
              errors: [new CompilationError({ startLine: 0, startColumn: 0, endLine: 0, endColumn: 0, message: 'Failed to fetch completions.', severity: 'Error' })],
            }));
            return { suggestions: [] };
          }
        }
      });
    }
  }, [syntaxConfig, onExecute]);

  useEffect(() => {
    loadSyntaxConfig();
  }, []);

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
            checkCode(code); // Проверяем код при первой загрузке
          }}
        />
      </div>
      <button onClick={() => checkCode(code)} className="button">Check Code</button>
    </div>
  );
};

export default CodeEditor;