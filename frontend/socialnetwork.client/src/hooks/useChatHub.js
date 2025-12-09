import { useEffect, useRef, useState } from 'react';
import { createChatConnection } from '../signalr';

export function useChatHub({ baseUrl, getToken, chatId, onMessage }) {
    const [connected, setConnected] = useState(false);
    const connRef = useRef(null);

    useEffect(() => {
        let mounted = true;
        const connection = createChatConnection({ baseUrl, getToken });
        connRef.current = connection;

        const onReceive = (msg) => {
            console.log('Received message:', msg);
            onMessage?.(msg);
        };

        connection.on('ReceiveMessage', onReceive);

        connection.onreconnecting((err) => {
            console.warn('SignalR reconnecting', err);
            if (mounted) setConnected(false);
        });
        connection.onreconnected((id) => {
            console.info('SignalR reconnected', id);
            if (mounted) setConnected(true);
        });
        connection.onclose((err) => {
            console.warn('SignalR closed', err);
            if (mounted) setConnected(false);
        });

        const start = async () => {
            try {
                if (connection.state === 'Connected') {
                    if (mounted) setConnected(true);
                    return;
                }
                await connection.start();
                if (mounted) setConnected(true);
            } catch (err) {
                console.error('SignalR start failed:', err);
                if (mounted) setConnected(false);
                setTimeout(() => {
                    if (mounted) start();
                }, 2000);
            }
        };

        start();

        return () => {
            mounted = false;
            try { connection.off('ReceiveMessage', onReceive); } catch {}
            connection.stop().catch(() => {});
        };
    }, [baseUrl, getToken, onMessage, chatId]);

    const joinChat = async (_chatId, userId) => {
        try {
            if (!connRef.current || connRef.current.state !== 'Connected') return;
            await connRef.current.invoke('JoinChat', _chatId, userId);
        } catch (e) {
            console.error('JoinChat failed', e);
        }
    };

    const sendMessage = async (_chatId, content) => {
        const conn = connRef.current;
        if (!conn || conn.state !== 'Connected') {
            console.warn('SignalR not connected');
            return;
        }
        try {
            console.log('Sending message:', { _chatId, content });
            await conn.invoke('SendMessage', _chatId, content);
            console.log('Message sent');
        } catch (e) {
            console.error('SendMessage failed', e);
            throw e;
        }
    };

    return { connected, joinChat, sendMessage };
}