import { useEffect, useRef, useState } from 'react';
import { getChatConnection } from './signalr';

export function useChatHub({ baseUrl, getToken, chatId, onMessage }) {
	const [connected, setConnected] = useState(false);
	const connRef = useRef(null);

	useEffect(() => {
		const connection = getChatConnection({ baseUrl, getToken });
		connRef.current = connection;

		const start = async () => {
			if (connection.state === 'Connected') return;
			try {
				await connection.start();
				setConnected(true);
			} catch (e) {
				setConnected(false);
				setTimeout(start, 1500);
			}
		};

		const handler = msg => {
			onMessage?.(msg);
		};
		connection.on('ReceiveMessage', handler);

		start();

		return () => {
			connection.off('ReceiveMessage', handler);
		};
	}, [baseUrl, getToken, onMessage]);

	const joinChat = async (chatId, userId) => {
		if (!connRef.current || connRef.current.state !== 'Connected') return;
		await connRef.current.invoke('JoinChat', chatId, userId);
	};

	const sendMessage = async (chatId, content) => {
		if (!connRef.current || connRef.current.state !== 'Connected') return;
		await connRef.current.invoke('SendMessage', chatId, content);
	};

	return { connected, joinChat, sendMessage };
}