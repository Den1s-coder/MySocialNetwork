import { useEffect, useRef, useState } from 'react';
import { getChatConnection } from '../signalr';

export function useChatHub({ baseUrl, getToken, chatId, onMessage }) {
    const [connected, setConnected] = useState(false);
    const connRef = useRef(null);
    const startingRef = useRef(false);

    useEffect(() => {
        const connection = getChatConnection({ baseUrl, getToken });
        connRef.current = connection;

        const start = async () => {
            if (!connection) return;
            if (connection.state !== 'Disconnected' || startingRef.current) return;
            startingRef.current = true;
            try {
                await connection.start();
                setConnected(true);
                // Користувач автоматично додається до всіх чатів при підключенні
                // Не потрібно викликати JoinChat вручну
            } catch (e) {
                setConnected(false);
                setTimeout(() => { startingRef.current = false; start(); }, 1500);
            } finally {
                startingRef.current = false;
            }
        };

        const handler = msg => {
            console.log('Received message:', msg);
            onMessage?.(msg);
        };
        connection.on('ReceiveMessage', handler);

        start();

        return () => {
            connection.off('ReceiveMessage', handler);
        };
    }, [baseUrl, getToken, onMessage]);

    // Видалити joinChat або зробити його пустим
    const joinChat = async (chatId, userId) => {
        // Користувач вже автоматично доданий до всіх чатів при підключенні
        console.log('User already joined all chats automatically');
    };

    const sendMessage = async (chatId, content) => {
        if (!connRef.current || connRef.current.state !== 'Connected') {
            console.warn('SignalR not connected');
            return;
        }

        try {
            console.log('Sending message:', { chatId, content });
            await connRef.current.invoke('SendMessage', chatId, content);
            console.log('Message sent successfully');
        } catch (e) {
            console.error('Error sending message:', e);
            throw e;
        }
    };

    return { connected, joinChat, sendMessage };
}