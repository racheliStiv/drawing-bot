import React, { useState } from 'react';
import axios from 'axios';
const ChatPanel = ({ onReceiveShapes, shapes }) => {
  const [messages, setMessages] = useState([]);
  const [input, setInput] = useState('');

  const handleSend = async () => {
    if (!input.trim()) return;

    const userMessage = { role: 'user', text: input };
    setMessages(prev => [...prev, userMessage]);


    await axios.post('https://localhost:7270/api/Ai/generate', {
      prompt: input,
      existingDrawingsJson: JSON.stringify(shapes)
    })
      .then(res => {

        console.log(JSON.parse(res.data.drawingJson));
        const data = JSON.parse(res.data.drawingJson);
        if (!data || data.length === 0) {
          const errorMessage = { role: 'bot', text: '⚠️ שגיאה בקבלת הצורות' };
          setMessages(prev => [...prev, errorMessage]);
          return;
        }
        const botMessage = { role: 'bot', text: '🎨 הבוט צייר בהצלחה!' };
        setMessages(prev => [...prev, botMessage]);

        if (data && Array.isArray(data)) {
          onReceiveShapes(data);
        }
      })
      .catch(err => {
        const errorMessage = { role: 'bot', text: '⚠️ שגיאה בשליחת הפרומפט' };
        setMessages(prev => [...prev, errorMessage]);
        console.error(err);
      });

    setInput('');
  };

  return (
    <div className="ChatPanel">
      <h2>שיחה עם הבוט</h2>
      <div className="messages">
        {messages.map((msg, index) => (
          <div key={index} style={{ textAlign: msg.role === 'user' ? 'right' : 'left', display: 'flex' }}>
            <strong>{msg.role === 'user' ? 'את/ה' : 'בוט'}:</strong> {msg.text}
          </div>
        ))}
      </div>

      <div className="input-row">
        <input
          type="text"
          value={input}
          onChange={(e) => setInput(e.target.value)}
          placeholder="כתוב פרומפט לציור..."
        />
        <button onClick={handleSend}>שלח</button>
      </div>
    </div>
  );
};
export default ChatPanel;