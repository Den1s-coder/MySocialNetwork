import { useState, useCallback } from 'react';
import { useChatHub } from '../useChatHub';

const BASE_URL = 'https://localhost:7222'; 
const getToken = () => localStorage.getItem('token');

export default function Chat({ chatId, currentUserId }) {
	const [messages, setMessages] = useState([]);
	const onMessage = useCallback((msg) => {
		setMessages(prev => [...prev, msg]);
	}, []);

	const { connected, sendMessage, joinChat } = useChatHub({
		baseUrl: BASE_URL,
		getToken,
		chatId,
		onMessage
	});

	const [text, setText] = useState('');

	const handleSend = async (e) => {
		e.preventDefault();
		if (!text.trim()) return;
		await sendMessage(chatId, text.trim());
		setText('');
	};

	const handleJoin = async () => {
		await joinChat(chatId, currentUserId);
	};

	return (
		<div>
			<div>Статус: {connected ? 'Підключено' : 'Відключено'}</div>
			<button onClick={handleJoin}>Join (optional)</button>

			<ul>
				{messages.map(m => (
					<li key={m.id ?? `${m.chatId}-${m.sentAt}-${m.senderId}`}>
						<b>{m.senderId}</b>: {m.content}
					</li>
				))}
			</ul>

			<form onSubmit={handleSend}>
				<input value={text} onChange={e => setText(e.target.value)} placeholder="Повідомлення..." />
				<button type="submit" disabled={!connected}>Надіслати</button>
			</form>
		</div>
	);
}