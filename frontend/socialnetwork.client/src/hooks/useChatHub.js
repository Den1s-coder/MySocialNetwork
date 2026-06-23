import { useEffect, useRef, useState } from 'react';
import * as signalR from '@microsoft/signalr';
import { createChatConnection } from '../signalr';

export function useChatHub({ baseUrl, getToken, onMessage, onMessageUpdated }) {
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

        const onUpdated = (msg) => {
            console.log('Message updated:', msg);
            onMessageUpdated?.(msg);
        };

        connection.on('ReceiveMessage', onReceive);
        connection.on('MessageUpdated', onUpdated);

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
                        } catch {
                            /* ignore */
                        }
                    }, 2000);
                }
            } finally {
                startPromiseRef.current = null;
            }
        };

        start();

        return () => {
            mountedRef.current = false;

            try { connection.off('ReceiveMessage', onReceive); } catch { /* ignore */ }
            try { connection.off('MessageUpdated', onUpdated); } catch { /* ignore */ }
            try {
                const p = startPromiseRef.current;
                if (p) {
                    p.finally(() => {
                        connection.stop().catch(() => { /* ignore */ });
                    });
                } else {
                    connection.stop().catch(() => { /* ignore */ });
                }
            } catch {
                /* ignore */
            }
        };
    }, [baseUrl, getToken, onMessage, onMessageUpdated]);

    const joinChat = async (_chatId, userId) => {
        try {
            if (!connRef.current || connRef.current.state !== signalR.HubConnectionState.Connected) return;
            await connRef.current.invoke('JoinChat', _chatId, userId);
        } catch (e) {
            console.error('JoinChat failed', e);
        }
    };

    const sendMessage = async (_chatId, content, photoUrl = null) => {
        const conn = connRef.current;
        if (!conn || conn.state !== signalR.HubConnectionState.Connected) {
            console.warn('SignalR not connected');
            return;
        }
        try {
            console.log('Sending message:', { _chatId, content, photoUrl });
            await conn.invoke('SendMessage', _chatId, content, photoUrl);
            console.log('Message sent');
        } catch (e) {
            console.error('SendMessage failed', e);
            throw e;
        }
    };

    const editMessage = async (messageId, newContent) => {
        const conn = connRef.current;
        if (!conn || conn.state !== signalR.HubConnectionState.Connected) {
            console.warn('SignalR not connected');
            return;
        }
        try {
            console.log('Editing message:', { messageId, newContent });
            await conn.invoke('EditMessage', messageId, newContent);
            console.log('Message edited');
        } catch (e) {
            console.error('EditMessage failed', e);
            throw e;
        }
    };

    return { connected, joinChat, sendMessage, editMessage };
}