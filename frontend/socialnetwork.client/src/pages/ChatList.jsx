import { useEffect, useState } from 'react';
import { Link, useNavigate } from 'react-router-dom';
import { useAuth } from '../hooks/useAuth';
import { authFetch } from '../hooks/authFetch';
import Avatar from '../components/Avatar';

const API_BASE = 'https://localhost:7142';

export default function ChatList() {
    const navigate = useNavigate();
    const { accessToken, isAuthenticated, currentUserId, currentUserName } = useAuth();
    const [chats, setChats] = useState([]);
    const [users, setUsers] = useState([]);
    const [status, setStatus] = useState('idle');
    const [error, setError] = useState(null);

    useEffect(() => {
        if (!isAuthenticated) {
            navigate('/login');
            return;
        }
    }, [isAuthenticated, navigate]);

    useEffect(() => {
        if (!accessToken) return;

        const loadData = async () => {
            setStatus('loading');
            try {
                const chatsRes = await authFetch(`${API_BASE}/api/Chat/chats`, {
                    headers: { 'Authorization': `Bearer ${accessToken}` }
                });
                if (!chatsRes.ok) throw new Error(`HTTP ${chatsRes.status}`);
                const chatsData = await chatsRes.json();
                setChats(chatsData);

                const usersRes = await authFetch(`${API_BASE}/api/User/users`, {
                    headers: { 'Authorization': `Bearer ${accessToken}` }
                });
                if (!usersRes.ok) throw new Error(`HTTP ${usersRes.status}`);
                const usersData = await usersRes.json();
                setUsers(usersData);

                setStatus('idle');
            } catch (e) {
                setError(e.message || 'Помилка завантаження');
                setStatus('error');
            }
        };
        loadData();
    }, [accessToken]);

    const createPrivateChat = async (otherUserId) => {
        try {
            const res = await authFetch(`${API_BASE}/api/Chat/private/${otherUserId}`, {
                method: 'POST',
                headers: { 'Authorization': `Bearer ${accessToken}` }
            });
            if (!res.ok) throw new Error(`HTTP ${res.status}`);
            const { chatId } = await res.json();

            console.log('Created chat with ID:', chatId);

            navigate(`/chat/${chatId}`);
        } catch (e) {
            setError(e.message || 'Помилка створення чату');
        }
    };

    const normalizeParticipants = (chat) => {
        const ucs = chat.userChats || chat.UserChats || [];
        return (ucs || []).map(uc => ({
            userId: (uc.userId || uc.UserId || '')?.toString(),
            userName: uc.userName || uc.UserName || '',
            profilePictureUrl: uc.profilePictureUrl || uc.ProfilePictureUrl || null
        }));
    };

    const getChatTitle = (chat) => {
        if (!chat) return 'Чат';
        const type = chat.type;
        const isPrivate = type === 0 || String(type).toLowerCase() === 'private';
        const participants = normalizeParticipants(chat);

        if (isPrivate && participants.length > 0) {
            const other = participants.find(p => {
                if (!p.userId) return false;
                if (!currentUserId) return true; 
                return p.userId.toString().toLowerCase() !== currentUserId.toString().toLowerCase();
            });
            if (other) return other.userName || other.userId || 'Користувач';
            const first = participants[0];
            if (first) return first.userName || first.userId || 'Користувач';
            return chat.title || 'Приватний чат';
        }

        if (chat.title && String(chat.title).trim() !== '') return chat.title;
        if (participants.length > 0) {
            const names = participants.map(p => p.userName || p.userId || 'Користувач');
            return names.join(', ');
        }
        return chat.title || 'Чат';
    };

    const getChatAvatar = (chat) => {
        const ucs = chat.userChats || chat.UserChats || [];
        const participants = (ucs || []).map(uc => ({
            userId: (uc.userId || uc.UserId || '')?.toString(),
            userName: uc.userName || uc.UserName || '',
            profilePictureUrl: uc.profilePictureUrl || uc.ProfilePictureUrl || null
        }));
        const isPrivate = chat.type === 0 || String(chat.type).toLowerCase() === 'private';
        if (isPrivate && participants.length > 0) {
            const other = participants.find(p => p.userId && currentUserId && p.userId.toLowerCase() !== currentUserId.toString().toLowerCase()) || participants[0];
            return other?.profilePictureUrl ?? null;
        }
        return null;
    };

    if (status === 'loading') return <p>Завантаження…</p>;
    if (status === 'error') return <p>Помилка: {error}</p>;
    if (!isAuthenticated) return <p>Авторизуйтесь для доступу до чатів</p>;

    return (
        <div style={{ maxWidth: 800, margin: '24px auto', padding: '0 12px' }}>
            <h2>Мої чати</h2>

            <div style={{ marginBottom: 32 }}>
                <h3>Активні чати</h3>
                {chats.length === 0 ? (
                    <p>У вас поки немає чатів</p>
                ) : (
                    <ul style={{ listStyle: 'none', padding: 0 }}>
                        {chats.map(chat => (
                            <li key={chat.id} style={{ marginBottom: 8 }}>
                                <Link
                                    to={`/chat/${chat.id}`}
                                    style={{
                                        display: 'block',
                                        padding: 12,
                                        border: '1px solid #ddd',
                                        borderRadius: 8,
                                        textDecoration: 'none',
                                        color: 'inherit'
                                    }}
                                >
                                    <div style={{ display: 'flex', alignItems: 'center', gap: 12 }}>
                                        <Avatar url={getChatAvatar(chat)} name={getChatTitle(chat)} />
                                        <div style={{ fontWeight: 'bold' }}>
                                            {getChatTitle(chat)}
                                        </div>
                                    </div>
                                    <div style={{ fontSize: 12, color: '#666', marginTop: 6 }}>
                                        Тип: {chat.type === 0 ? 'Приватний' : chat.type === 1 ? 'Група' : 'Канал'}
                                    </div>
                                </Link>
                            </li>
                        ))}
                    </ul>
                )}
            </div>

            <div>
                <h3>Створити приватний чат</h3>
                <p>Оберіть користувача для початку приватного чату:</p>
                <div style={{ display: 'grid', gap: 8 }}>
                    {users.map(user => (
                        <button
                            key={user.id}
                            onClick={() => createPrivateChat(user.id)}
                            style={{
                                padding: 8,
                                border: '1px solid #ddd',
                                borderRadius: 4,
                                background: 'white',
                                cursor: 'pointer'
                            }}
                        >
                            {user.name} ({user.email})
                        </button>
                    ))}
                </div>
            </div>
        </div>
    );
}