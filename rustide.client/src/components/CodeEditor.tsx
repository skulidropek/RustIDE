import React, { useState, useEffect, useRef } from 'react';
import MonacoEditor from 'react-monaco-editor';
import * as monaco from 'monaco-editor';
import './CodeEditor.css';
import { Client, CodeRequest, CompilationResult, MonarchLanguage, MonarchLanguageRule, MonarchLanguageAction, ICompilationResult, CompilationError, SyntaxConfig, CompletionRequest, CompletionItem } from '../api-client';
import { languages } from 'monaco-editor/esm/vs/editor/editor.api';
import type { languages as monacoLanguages } from 'monaco-editor/esm/vs/editor/editor.api';
type IMonarchLanguage = monacoLanguages.IMonarchLanguage;

const CodeEditor: React.FC = () => {
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
  
  const [errors, setErrors] = useState<CompilationResult[]>([]);
  const [output, setOutput] = useState<string>('');
  const [syntaxConfig, setSyntaxConfig] = useState<SyntaxConfig | null>(null);
  const editorRef = useRef<monaco.editor.IStandaloneCodeEditor | null>(null);
  const client = new Client("https://localhost:7214");

  const onChange = (newValue: string) => {
    setCode(newValue);
  };

  const loadSyntaxConfig = async () => {
    try {
      const response = await client.syntax();
      setSyntaxConfig(response);
    } catch (error) {
      setErrors([new CompilationResult({
        errors: [new CompilationError({ startLine: 0, startColumn: 0, endLine: 0, endColumn: 0, message: 'Failed to load syntax config.', severity: 'Error' })],
      })]);
    }
  };

  const checkCode = async () => {
    try {
      const request = new CodeRequest({ code });
      const result: CompilationResult = await client.compile(request);
      if (result.success) {
        setOutput(result.output ?? "");
        setErrors([]);
      } else {
        setErrors(result.errors ?? []);
      }
    } catch (error) {
      setErrors([new CompilationResult({
        errors: [new CompilationError({ startLine: 0, startColumn: 0, endLine: 0, endColumn: 0, message: 'Failed to connect to the server.', severity: 'Error' })],
      })]);
      setOutput('');
    }
  };

  useEffect(() => {
    if (syntaxConfig?.monarchLanguage && syntaxConfig?.languageConfiguration) {
      monaco.languages.register({ id: 'csharp' });
  
      const convertTokenAction = (action: MonarchLanguageAction | string): monaco.languages.IMonarchLanguageAction => {
        if (typeof action === 'string') {
          return { token: action };
        }
        const result: monaco.languages.IMonarchLanguageAction = {};
        if (action.token) result.token = action.token;
        if (action.next) result.next = action.next;
        if (action.cases) result.cases = action.cases;
        return Object.keys(result).length === 0 ? { token: 'source' } : result;
      };
  
      const convertTokenRules = (rules: MonarchLanguageRule[]): monaco.languages.IMonarchLanguageRule[] => {
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
          tokenizer[key] = convertTokenRules(rules);
        }
      }
  
      tokenizer['symbols'] = [{ regex: /[=><!~?:&|+\-*\/\^%]+/, action: { token: 'operator' } }];
  
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
        provideCompletionItems: async (model, position, context, token) : Promise<monaco.languages.CompletionList> => {
          const wordUntilPosition = model.getWordUntilPosition(position);
          
          // Логируем позицию, текущий код и контекст триггера
          console.log("Запрос автодополнения. Позиция:", position);
          console.log("Контекст триггера:", context);
          console.log("Код на момент вызова:", model.getValue());
      
          try {
            const request = new CompletionRequest({
              code: model.getValue(),
              position: model.getOffsetAt(position),
            });
      
            // Логируем запрос перед отправкой
            console.log("Отправляем запрос на сервер автодополнения:", request);
      
            const response = await client.completion(request);
      
            // Логируем ответ от сервера
            console.log("Ответ от сервера автодополнения:", response);
      
            const suggestions = response.map((item: CompletionItem) => ({
              label: item.label ?? "",  // Устанавливаем значение по умолчанию для label
              kind: monaco.languages.CompletionItemKind[item.kind as keyof typeof monaco.languages.CompletionItemKind], // Приводим типы для kind
              insertText: item.insertText ?? "", // Если insertText отсутствует, используем пустую строку
              detail: item.detail ?? "", // Аналогично для detail
              documentation: item.documentation ?? "", // Проверка на undefined
              commitCharacters: item.commitCharacters ?? [], // Если commitCharacters отсутствуют, используем пустой массив
              insertTextRules: item.insertTextRules ? monaco.languages.CompletionItemInsertTextRule[item.insertTextRules as keyof typeof monaco.languages.CompletionItemInsertTextRule] : undefined, // Приводим тип для insertTextRules
              range: {
                  startLineNumber: position.lineNumber,
                  endLineNumber: position.lineNumber,
                  startColumn: wordUntilPosition.startColumn,
                  endColumn: wordUntilPosition.endColumn,
              }
          }));
          
          return { suggestions };
          
          } catch (error) {
            // Логируем ошибки
            console.error("Ошибка при запросе автодополнений:", error);
            setErrors([new CompilationResult({
              errors: [new CompilationError({ startLine: 0, startColumn: 0, endLine: 0, endColumn: 0, message: 'Failed to fetch completions.', severity: 'Error' })],
            })]);
            return { suggestions: [] };
          }
        }
      });
    }
  }, [syntaxConfig]);

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
              error.errors && error.errors.length > 0 ? (
                <li key={index}>{error.errors[0].message} (Line {error.errors[0].startLine}, Column {error.errors[0].startColumn})</li>
              ) : null
            ))}
          </ul>
        </div>
      )}
    </div>
  );
};

export default CodeEditor;
