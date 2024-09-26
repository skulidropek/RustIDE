export interface SyntaxConfig {
  keywords: string[];
  operators: string[];
  comments: CommentConfig;
  brackets: BracketPair[];
  tokenizer: { [key: string]: TokenRule[] };
}

export interface CommentConfig {
  lineComment: string;
  blockComment: string[];
}

export interface BracketPair {
  open: string;
  close: string;
}

export interface TokenRule {
  regex: string;
  action: TokenAction;
  include?: string;
}

export interface TokenAction {
  token?: string;
  next?: string;
  cases?: { [key: string]: string };
}