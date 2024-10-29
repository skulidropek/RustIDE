// useCodeEditorLogic.ts
import { useState, useEffect, useRef, useCallback } from 'react';
import { Client, CodeRequest, CompilationResult, CompilationError, SyntaxConfig, CompletionRequest, CompletionItem } from '../../api-client';
import config from '../../config';
import * as monaco from 'monaco-editor';

export const useCodeEditorLogic = (onCodeChange: (newCode: string) => void, onExecute: (result: CompilationResult) => void) => {
  const [syntaxConfig, setSyntaxConfig] = useState<SyntaxConfig | null>(null);
  const editorRef = useRef<monaco.editor.IStandaloneCodeEditor | null>(null);
  const client = new Client(config.apiUrl);

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
    onCodeChange(newValue);
    checkCode(newValue);
  }, [onCodeChange, checkCode]);

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
    loadSyntaxConfig();
  }, []);

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
        ignoreCase: syntaxConfig.monarchLanguage.ignoreCase || false,
        unicode: syntaxConfig.monarchLanguage.unicode || false,
        brackets: syntaxConfig.monarchLanguage.brackets?.map(bracket => ({
          open: bracket.open || '',
          close: bracket.close || '',
          token: bracket.token || ''
        })) || [],
        start: syntaxConfig.monarchLanguage.start || 'root',
        includeLF: syntaxConfig.monarchLanguage.includeLF || false,
      });
  
      const comments: monaco.languages.CommentRule = {
        lineComment: syntaxConfig.languageConfiguration.comments?.lineComment,
        blockComment: syntaxConfig.languageConfiguration.comments?.blockComment
          ? [syntaxConfig.languageConfiguration.comments.blockComment.open!, syntaxConfig.languageConfiguration.comments.blockComment.close!]
          : undefined,
      };
  
      monaco.languages.setLanguageConfiguration('csharp', {
        comments: comments,
        brackets: syntaxConfig.languageConfiguration.brackets?.map((b) => [b.open, b.close] as monaco.languages.CharacterPair) || [],
        wordPattern: syntaxConfig.languageConfiguration.wordPattern ? new RegExp(syntaxConfig.languageConfiguration.wordPattern) : undefined,
        indentationRules: syntaxConfig.languageConfiguration.indentationRules ? {
          increaseIndentPattern: new RegExp(syntaxConfig.languageConfiguration.indentationRules.increaseIndentPattern || ''),
          decreaseIndentPattern: new RegExp(syntaxConfig.languageConfiguration.indentationRules.decreaseIndentPattern || ''),
          indentNextLinePattern: syntaxConfig.languageConfiguration.indentationRules.indentNextLinePattern ? new RegExp(syntaxConfig.languageConfiguration.indentationRules.indentNextLinePattern) : undefined,
          unIndentedLinePattern: syntaxConfig.languageConfiguration.indentationRules.unIndentedLinePattern ? new RegExp(syntaxConfig.languageConfiguration.indentationRules.unIndentedLinePattern) : undefined,
        } : undefined,
        onEnterRules: syntaxConfig.languageConfiguration.onEnterRules?.map(rule => ({
          beforeText: new RegExp(rule.beforeText || ''),
          afterText: rule.afterText ? new RegExp(rule.afterText) : undefined,
          action: rule.action ? {
            indentAction: mapIndentAction(rule.action.indentAction),
            appendText: rule.action.appendText,
            removeText: rule.action.removeText
          } : { indentAction: monaco.languages.IndentAction.None }
        })),
        autoClosingPairs: syntaxConfig.languageConfiguration.autoClosingPairs?.map(pair => ({
          open: pair.open || '',
          close: pair.close || '',
          notIn: pair.notIn,
        })),
        surroundingPairs: syntaxConfig.languageConfiguration.surroundingPairs?.map(pair => ({
          open: pair.open || '',
          close: pair.close || '',
        })),
        folding: syntaxConfig.languageConfiguration.folding ? {
          markers: syntaxConfig.languageConfiguration.folding.markers ? {
            start: new RegExp(syntaxConfig.languageConfiguration.folding.markers.start || ''),
            end: new RegExp(syntaxConfig.languageConfiguration.folding.markers.end || '')
          } : undefined,
          offSide: syntaxConfig.languageConfiguration.folding.offSide
        } : undefined,
        autoCloseBefore: syntaxConfig.languageConfiguration.autoCloseBefore || ")}]',;"
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

  const mapIndentAction = (action: string | undefined): monaco.languages.IndentAction => {
    switch (action) {
      case 'indent':
        return monaco.languages.IndentAction.Indent;
      case 'indentOutdent':
        return monaco.languages.IndentAction.IndentOutdent;
      case 'outdent':
        return monaco.languages.IndentAction.Outdent;
      case 'none':
      default:
        return monaco.languages.IndentAction.None;
    }
  };

  return {
    editorRef,
    onChange,
    checkCode,
  };
};
