import React, { useState } from 'react';
import './ChatWindow.css';
import { Client, UserMessageRequest } from '../../api-client';

interface ChatWindowProps {
  code: string;
}

const ChatWindow: React.FC<ChatWindowProps> = ({ code }) => {
  const [messages, setMessages] = useState<Array<{ text: string; isUser: boolean }>>([]);
  const [inputMessage, setInputMessage] = useState('');
  const [client] = useState(new Client("https://localhost:7214"));
  const [isLoading, setIsLoading] = useState(false);

  const handleSendMessage = async () => {
    if (inputMessage.trim() !== '') {
      // Добавляем сообщение пользователя
      setMessages(prevMessages => [...prevMessages, { text: inputMessage, isUser: true }]);
      setIsLoading(true);
      
      try {
        // Отправляем сообщение ИИ
        const request = new UserMessageRequest({ message: inputMessage });
        const response = await client.aI(request);
        
        // Добавляем ответ ИИ
        setMessages(prevMessages => [...prevMessages, { text: response, isUser: false }]);
      } catch (error) {
        console.error('Ошибка при отправке сообщения ИИ:', error);
        setMessages(prevMessages => [...prevMessages, { text: 'Произошла ошибка при общении с ИИ.', isUser: false }]);
      } finally {
        setIsLoading(false);
      }

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
        {isLoading && <div className="message ai">ИИ печатает...</div>}
      </div>
      <div className="chat-input">
        <input
          type="text"
          value={inputMessage}
          onChange={(e) => setInputMessage(e.target.value)}
          onKeyPress={handleKeyPress}
          placeholder="Введите сообщение..."
        />
        <button onClick={handleSendMessage} disabled={isLoading}>Отправить</button>
      </div>
    </div>
  );
};

export default ChatWindow;