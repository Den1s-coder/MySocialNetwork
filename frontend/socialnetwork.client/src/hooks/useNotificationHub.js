import { useEffect, useRef, useState } from 'react';
import * as signalR from '@microsoft/signalr';

export function useNotificationHub({ baseUrl, getToken, onNotification }) {
    const connRef = useRef(null);
    const startPromiseRef = useRef(null);
    const [connected, setConnected] = useState(false);
    const mountedRef = useRef(true);

    const waitForToken = async (timeoutMs = 10000) => {
        const token = await getToken();
        if (token) return token;

        return new Promise((resolve) => {
            let resolved = false;
            const onTokensUpdated = async () => {
                try {
                    const t = await getToken();
                    if (t) {
                        resolved = true;
                        window.removeEventListener('tokens-updated', onTokensUpdated);
                        resolve(t);
                    }
                } catch {}
            };

            window.addEventListener('tokens-updated', onTokensUpdated);

            const to = setTimeout(() => {
                if (!resolved) {
                    window.removeEventListener('tokens-updated', onTokensUpdated);
                    resolve(null);
                }
            }, timeoutMs);
        });
    };

    useEffect(() => {
        mountedRef.current = true;

        const create = async () => {
            if (startPromiseRef.current) {
                try { await startPromiseRef.current; } catch {}
                return;
            }

            const token = await waitForToken(10000); 
            if (!token) {
                console.warn('NotificationHub: no access token available after wait Ч skipping connection start');
                return;
            }

            const connection = new signalR.HubConnectionBuilder()
                .withUrl(new URL('/notificationHub', baseUrl).toString(), {
                    accessTokenFactory: async () => {
                        try {
                            const t = await getToken();
                            // debug: длина токена (убрать в production)
                            console.debug('NotificationHub: accessToken length=', t ? t.length : 0);
                            return t;
                        } catch (e) {
                            console.warn('NotificationHub: accessTokenFactory failed', e);
                            return null;
                        }
                    },
                })
                .withAutomaticReconnect()
                .configureLogging(signalR.LogLevel.Information)
                .build();

            connRef.current = connection;

            connection.on('ReceiveNotification', (payload) => {
                try { onNotification?.(payload); } catch (e) { console.error('onNotification failed', e); }
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
                } finally {
                    startPromiseRef.current = null;
                }
            })();

            try {
                await startPromiseRef.current;
            } catch (err) {
                console.error('NotificationHub start failed', err);
                if (mountedRef.current) {
                    setTimeout(() => {
                        create().catch(() => {});
                    }, 2000);
                }
            }
        };

        create();

        return () => {
            mountedRef.current = false;
            try {
                const c = connRef.current;
                if (c) c.stop().catch(() => {});
            } catch {}
        };
    }, [baseUrl, getToken, onNotification]);

    return { connected, connection: connRef.current };
}