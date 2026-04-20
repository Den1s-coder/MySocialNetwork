import React, { useCallback, useEffect, useMemo, useState } from 'react';
import { useAuth } from '../hooks/useAuth';
import { authFetch } from '../hooks/authFetch';
import { useNotificationHub } from '../hooks/useNotificationHub';
import './NotificationBell.css';

const API_BASE = 'https://localhost:7142';

function parseMessage(msgOrRaw) {
    if (!msgOrRaw) return { text: '' };
    try {
        const obj = typeof msgOrRaw === 'string' ? JSON.parse(msgOrRaw) : msgOrRaw;
        if (obj?.type === 'comment_created') {
            const author = obj.authorName ?? obj.authorId;
            return { text: `${author} прокомментировал(а) ваш пост`, data: obj };
        }
        if (obj?.type === 'post_created') {
            const author = obj.authorName ?? obj.authorId;
            return { text: `${author} опубликовал(а) новый пост`, data: obj };
        }
        if (obj?.message) return { text: obj.message, data: obj };
        return { text: JSON.stringify(obj), data: obj };
    } catch {
        return { text: String(msgOrRaw) };
    }
}

export default function NotificationBell({ apiBase = API_BASE }) {
    const { accessToken } = useAuth();
    const [notifications, setNotifications] = useState([]); 
    const [open, setOpen] = useState(false);
    const [loading, setLoading] = useState(false);
    const [busy, setBusy] = useState(false);

    const getToken = useCallback(async () => accessToken ?? localStorage.getItem('accessToken'), [accessToken]);

    const handleIncoming = useCallback((payload) => {
        const raw = payload;
        const parsed = parseMessage(payload);
        const id = parsed.data?.notificationId ?? parsed.data?.commentId ?? parsed.data?.postId ?? (Math.random() * 1e9).toString();
        const createdAt = parsed.data?.createdAt ? new Date(parsed.data.createdAt).toISOString() : new Date().toISOString();

        setNotifications(prev => [
            { id, isRead: false, rawMessage: raw, text: parsed.text, data: parsed.data, createdAt },
            ...prev
        ]);
    }, []);

    const { connection } = useNotificationHub({ baseUrl: apiBase, getToken, onNotification: handleIncoming });

    const loadNotifications = useCallback(async () => {
        setLoading(true);
        try {
            const res = await authFetch('/api/Notification', { });
            if (!res.ok) throw new Error(`HTTP ${res.status}`);
            const data = await res.json();
            const mapped = (Array.isArray(data) ? data : []).map(n => {
                const parsed = parseMessage(n.message ?? n);
                return {
                    id: n.id ?? n.Id,
                    isRead: n.isRead ?? n.IsRead ?? false,
                    rawMessage: n.message ?? parsed.data ?? null,
                    text: parsed.text,
                    data: parsed.data,
                    createdAt: n.createdAt ?? n.CreatedAt
                };
            });
            setNotifications(mapped);
        } catch (e) {
            console.error('Failed to load notifications', e);
        } finally {
            setLoading(false);
        }
    }, []);

    useEffect(() => {
        loadNotifications();
    }, [loadNotifications]);

    const unreadCount = useMemo(() => notifications.filter(n => !n.isRead).length, [notifications]);

    const markAsRead = async (id) => {
        if (busy) return;
        setBusy(true);
        try {
            const res = await authFetch(`/api/Notification/markread/${id}`, { method: 'POST' });
            if (!res.ok) {
                throw new Error(`HTTP ${res.status}`);
            }
            setNotifications(prev => prev.map(n => n.id === id ? { ...n, isRead: true } : n));
        } catch (e) {
            console.error('Mark as read failed', e);
        } finally {
            setBusy(false);
        }
    };

    const markAllRead = async () => {
        if (busy) return;
        setBusy(true);
        try {
            const res = await authFetch('/api/Notification/markreadall', { method: 'POST' });
            if (!res.ok) throw new Error(`HTTP ${res.status}`);
            setNotifications(prev => prev.map(n => ({ ...n, isRead: true })));
        } catch (e) {
            console.error('Mark all read failed', e);
        } finally {
            setBusy(false);
        }
    };

    const onToggleOpen = () => {
        setOpen(o => !o);
        if (!open) loadNotifications();
    };

    const onItemClick = async (n) => {
        if (!n.isRead) await markAsRead(n.id);
    };

    return (
        <div className="notification-bell" style={{ position: 'relative' }}>
            <button className="bell-button" onClick={onToggleOpen} title="Уведомления">
                🔔
                {unreadCount > 0 && <span className="badge">{unreadCount}</span>}
            </button>

            {open && (
                <div className="notification-dropdown">
                    <div className="dropdown-header">
                        <strong>Уведомления</strong>
                        <div style={{ display: 'flex', gap: 8, alignItems: 'center' }}>
                            <button onClick={markAllRead} className="mark-read-btn" disabled={busy || unreadCount === 0}>
                                Отметить все
                            </button>
                            <button onClick={loadNotifications} className="mark-read-btn" disabled={loading}>
                                Обновить
                            </button>
                        </div>
                    </div>

                    <div className="dropdown-list">
                        {loading && <div style={{ padding: 12 }}>Загрузка…</div>}
                        {!loading && notifications.length === 0 && <div className="empty">Пока нет уведомлений</div>}

                        {notifications.map(n => (
                            <div
                                key={n.id}
                                className={`notification-item ${n.isRead ? 'read' : 'unread'}`}
                                onClick={() => onItemClick(n)}
                                style={{ cursor: 'pointer' }}
                            >
                                <div style={{ display: 'flex', justifyContent: 'space-between', gap: 8 }}>
                                    <div className="notification-text">{n.text}</div>
                                    <div style={{ minWidth: 90, textAlign: 'right', fontSize: 11, color: '#888' }}>
                                        {n.createdAt ? new Date(n.createdAt).toLocaleString() : ''}
                                    </div>
                                </div>
                                <div style={{ marginTop: 6 }}>
                                    {n.data && n.data.commentId && (
                                        <div style={{ fontSize: 13, color: '#444' }}>
                                            Комментарий: <span style={{ color: '#222' }}>{n.data?.snippet ?? ''}</span>
                                        </div>
                                    )}
                                </div>
                                <div style={{ marginTop: 8, display: 'flex', gap: 8 }}>
                                    {!n.isRead && (
                                        <button onClick={(e) => { e.stopPropagation(); markAsRead(n.id); }} disabled={busy}>
                                            Отметить как прочитанное
                                        </button>
                                    )}
                                </div>
                            </div>
                        ))}
                    </div>
                </div>
            )}
        </div>
    );
}