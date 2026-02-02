import { useEffect, useRef, useState } from 'react';
import * as signalR from '@microsoft/signalr';

export function useNotificationHub({ baseUrl, getToken, onNotification }) {
    const connRef = useRef(null);
    const startPromiseRef = useRef(null);
    const [connected, setConnected] = useState(false);
    const mountedRef = useRef(true);

    const parseJwt = (token) => {
        try {
            const base64Url = token.split('.')[1];
            const base64 = base64Url.replace(/-/g, '+').replace(/_/g, '/');
            const jsonPayload = decodeURIComponent(atob(base64).split('').map(c => {
                return '%' + ('00' + c.charCodeAt(0).toString(16)).slice(-2);
            }).join(''));
            return JSON.parse(jsonPayload);
        } catch {
            return null;
        }
    };

    const tryRefreshToken = async () => {
        try {
            const refreshToken = localStorage.getItem('refreshToken');
            if (!refreshToken) return null;

            const resp = await fetch(new URL('/api/Auth/refresh-token', baseUrl).toString(), {
                method: 'POST',
                headers: { 'Content-Type': 'application/json' },
                body: JSON.stringify(refreshToken)
            });

            if (!resp.ok) {
                localStorage.removeItem('accessToken');
                localStorage.removeItem('refreshToken');
                window.dispatchEvent(new Event('tokens-updated'));
                return null;
            }

            const data = await resp.json();
            if (data?.accessToken && data?.refreshToken) {
                localStorage.setItem('accessToken', data.accessToken);
                localStorage.setItem('refreshToken', data.refreshToken);
                window.dispatchEvent(new Event('tokens-updated'));
                return data.accessToken;
            }

            return null;
        } catch (e) {
            console.warn('tryRefreshToken failed', e);
            return null;
        }
    };

    const ensureValidToken = async (timeoutMs = 10000) => {
        let token = localStorage.getItem('accessToken') || (getToken ? await getToken() : null);
        if (token) {
            const payload = parseJwt(token);
            const now = Math.floor(Date.now() / 1000);

            if (payload?.exp && payload.exp > now + 5) {
                return token;
            }

            const refreshed = await tryRefreshToken();
            if (refreshed) return refreshed;
            return null;
        }

        return new Promise((resolve) => {
            let resolved = false;
            const onTokensUpdated = async () => {
                try {
                    const t = localStorage.getItem('accessToken') || (getToken ? await getToken() : null);
                    if (t) {
                        resolved = true;
                        window.removeEventListener('tokens-updated', onTokensUpdated);
                        resolve(t);
                    }
                } catch { }
            };
            window.addEventListener('tokens-updated', onTokensUpdated);

            setTimeout(() => {
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
                try { await startPromiseRef.current; } catch { }
                return;
            }

            const validToken = await ensureValidToken(10000);
            if (!validToken) {
                console.warn('NotificationHub: no valid access token available — skipping connection start');
                return;
            }

            const connection = new signalR.HubConnectionBuilder()
                .withUrl(new URL('/notificationHub', baseUrl).toString(), {
                    accessTokenFactory: async () => {
                        const t = localStorage.getItem('accessToken') || (getToken ? await getToken() : null);
                        console.debug('NotificationHub: accessToken length=', t ? t.length : 0);
                        return t;
                    }
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
            } catch { }
        };
    }, [baseUrl, getToken, onNotification]);

    return { connected, connection: connRef.current };
}