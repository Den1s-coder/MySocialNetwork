import { useEffect, useRef, useState } from 'react';
import * as signalR from '@microsoft/signalr';
import { createNotificationConnection } from '../signalr';

export function useNotificationHub({ baseUrl, getToken, onNotification }) {
    const connRef = useRef(null);
    const startPromiseRef = useRef(null);
    const [connected, setConnected] = useState(false);
    const mountedRef = useRef(true);

    useEffect(() => {
        mountedRef.current = true;

        const create = async () => {
            if (startPromiseRef.current) {
                try { 
                    await startPromiseRef.current; 
                } catch { }
                return;
            }

            const connection = createNotificationConnection({ baseUrl, getToken });
            connRef.current = connection;

            connection.on('ReceiveNotification', (payload) => {
                try { 
                    onNotification?.(payload); 
                } catch (e) { 
                    console.error('onNotification failed', e); 
                }
            });

            connection.onreconnecting(err => {
                console.warn('NotificationHub reconnecting', err);
                if (mountedRef.current) setConnected(false);
            });

            connection.onreconnected(id => {
                console.info('NotificationHub reconnected', id);
                if (mountedRef.current) setConnected(true);
            });

            connection.onclose(err => {
                console.warn('NotificationHub closed', err);
                if (mountedRef.current) setConnected(false);
            });

            startPromiseRef.current = (async () => {
                try {
                    if (connection.state === signalR.HubConnectionState.Connected) {
                        if (mountedRef.current) setConnected(true);
                        return;
                    }
                    await connection.start();
                    if (mountedRef.current) setConnected(true);
                    console.info('NotificationHub connected');
                } catch (err) {
                    const isAbort = err && (err.name === 'AbortError' || /AbortError/i.test(String(err)));
                    console.error('NotificationHub start failed:', err);
                    if (!isAbort && mountedRef.current) {
                        setConnected(false);
                    }
                    if (mountedRef.current) {
                        setTimeout(() => {
                            create().catch(() => {});
                        }, 2000);
                    }
                } finally {
                    startPromiseRef.current = null;
                }
            })();

            try {
                await startPromiseRef.current;
            } catch (err) {
                console.error('Failed to start NotificationHub connection', err);
            }
        };

        create();

        return () => {
            mountedRef.current = false;
            try {
                const c = connRef.current;
                const p = startPromiseRef.current;
                if (p) {
                    p.finally(() => {
                        if (c) c.stop().catch(() => {});
                    });
                } else {
                    if (c) c.stop().catch(() => {});
                }
            } catch { }
        };
    }, [baseUrl, getToken, onNotification]);

    return { connected, connection: connRef.current };
}