import { useEffect, useRef, useState } from 'react';
import * as signalR from '@microsoft/signalr';
import { createChatConnection } from '../signalr';

export function useChatHub({ baseUrl, getToken, chatId, onMessage }) {
    const [connected, setConnected] = useState(false);
    const connRef = useRef(null);
    const startPromiseRef = useRef(null);
    const mountedRef = useRef(true);

    useEffect(() => {
        mountedRef.current = true;
        const connection = createChatConnection({ baseUrl, getToken });
        connRef.current = connection;

        const onReceive = (msg) => {
            console.log('Received message:', msg);
            onMessage?.(msg);
        };

        connection.on('ReceiveMessage', onReceive);

        connection.onreconnecting((err) => {
            console.warn('SignalR reconnecting', err);
            if (mountedRef.current) setConnected(false);
        });
        connection.onreconnected((id) => {
            console.info('SignalR reconnected', id);
            if (mountedRef.current) setConnected(true);
        });
        connection.onclose((err) => {
            console.warn('SignalR closed', err);
            if (mountedRef.current) setConnected(false);
        });

        const start = async () => {
            try {
                if (connection.state === signalR.HubConnectionState.Connected) {
                    if (mountedRef.current) setConnected(true);
                    return;
                }

                startPromiseRef.current = connection.start();
                await startPromiseRef.current;
                if (mountedRef.current) setConnected(true);
            } catch (err) {
                const isAbort = err && (err.name === 'AbortError' || /AbortError/i.test(String(err)));
                console.error('SignalR start failed:', err);
                if (!isAbort) {
                    if (mountedRef.current) setConnected(false);
                }
                if (mountedRef.current) {
                    setTimeout(() => {
                        try {
                            start();
                        } catch {}
                    }, 2000);
                }
            } finally {
                startPromiseRef.current = null;
            }
        };

        start();

        return () => {
            mountedRef.current = false;
            try { connection.off('ReceiveMessage', onReceive); } catch {}
            try {
                const p = startPromiseRef.current;
                if (p) {
                    p.finally(() => {
                        connection.stop().catch(() => {});
                    });
                } else {
                    connection.stop().catch(() => {});
                }
            } catch {}
        };
    }, [baseUrl, getToken, onMessage]);

    const joinChat = async (_chatId, userId) => {
        try {
            if (!connRef.current || connRef.current.state !== signalR.HubConnectionState.Connected) return;
            await connRef.current.invoke('JoinChat', _chatId, userId);
        } catch (e) {
            console.error('JoinChat failed', e);
        }
    };

    const sendMessage = async (_chatId, content) => {
        const conn = connRef.current;
        if (!conn || conn.state !== signalR.HubConnectionState.Connected) {
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