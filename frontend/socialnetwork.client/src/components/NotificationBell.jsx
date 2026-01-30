import React, { useCallback, useMemo, useState } from 'react';
import { useNotificationHub } from '../hooks/useNotificationHub';
import { useAuth } from '../hooks/useAuth'; 
import './NotificationBell.css';

export default function NotificationBell({ apiBase = 'https://localhost:7142' }) {
    const { accessToken } = useAuth(); 
    const [notifications, setNotifications] = useState([]);
    const [open, setOpen] = useState(false);

    const getToken = useCallback(async () => accessToken ?? localStorage.getItem('token'), [accessToken]);

    const onNotification = useCallback((payload) => {
        const n = typeof payload === 'string' ? JSON.parse(payload) : payload;
        setNotifications(prev => [{ id: (n.postId ?? n.id ?? Math.random()), text: n.message ?? n.type ?? JSON.stringify(n), data: n, read: false }, ...prev]);
    }, []);

    useNotificationHub({ baseUrl: apiBase, getToken, onNotification });

    const unreadCount = useMemo(() => notifications.filter(n => !n.read).length, [notifications]);

    const markAllRead = () => {
        setNotifications(prev => prev.map(n => ({ ...n, read: true })));
        // TODO: вызвать API для пометки прочитанных
    };

    return (
        <div className="notification-bell" style={{ position: 'relative' }}>
            <button className="bell-button" onClick={() => setOpen(o => !o)} title="Уведомления">
                🔔
                {unreadCount > 0 && <span className="badge">{unreadCount}</span>}
            </button>

            {open && (
                <div className="notification-dropdown">
                    <div className="dropdown-header">
                        <strong>Уведомления</strong>
                        <button onClick={markAllRead} className="mark-read-btn">Отметить все</button>
                    </div>

                    <div className="dropdown-list">
                        {notifications.length === 0 && <div className="empty">Пока нет уведомлений</div>}
                        {notifications.map(n => (
                            <div key={n.id} className={`notification-item ${n.read ? 'read' : 'unread'}`}>
                                <div className="notification-text">{n.text}</div>
                                <div className="notification-meta">{n.data?.createdAt ? new Date(n.data.createdAt).toLocaleString() : ''}</div>
                            </div>
                        ))}
                    </div>
                </div>
            )}
        </div>
    );
}