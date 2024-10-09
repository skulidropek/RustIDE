import React, { useState } from 'react';
import './ChatWindow.css';

interface ChatWindowProps {
  code: string;
}

const ChatWindow: React.FC<ChatWindowProps> = ({ code }) => {
  const [messages, setMessages] = useState<Array<{ text: string; isUser: boolean }>>([]);
  const [inputMessage, setInputMessage] = useState('');

  const handleSendMessage = () => {
    if (inputMessage.trim() !== '') {
      setMessages([...messages, { text: inputMessage, isUser: true }]);
      setInputMessage('');
    }
  };

  const handleKeyPress = (event: React.KeyboardEvent) => {
    if (event.key === 'Enter') {
      handleSendMessage();
    }
  };

  return (
    <div className="chat-window">
      <div className="chat-header">
        <h2>Чат с ИИ</h2>
      </div>
      <div className="chat-messages">
        {messages.map((message, index) => (
          <div key={index} className={`message ${message.isUser ? 'user' : 'ai'}`}>
            {message.text}
          </div>
        ))}
      </div>
      <div className="chat-input">
        <input
          type="text"
          value={inputMessage}
          onChange={(e) => setInputMessage(e.target.value)}
          onKeyPress={handleKeyPress}
          placeholder="Введите сообщение..."
        />
        <button onClick={handleSendMessage}>Отправить</button>
      </div>
    </div>
  );
};

export default ChatWindow;