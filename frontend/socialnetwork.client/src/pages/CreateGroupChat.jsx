import { useState, useEffect } from 'react';
import { useNavigate } from 'react-router-dom';
import { useAuth } from '../hooks/useAuth';
import { authFetch } from '../hooks/authFetch';
import Avatar from '../components/Avatar';

const API_BASE = 'https://localhost:7142';

export default function CreateGroupChat() {
    const navigate = useNavigate();
    const { accessToken, isAuthenticated, currentUserId } = useAuth();
    const [title, setTitle] = useState('');
    const [friends, setFriends] = useState([]);
    const [selectedUsers, setSelectedUsers] = useState([]);
    const [loading, setLoading] = useState(true);
    const [error, setError] = useState(null);

    useEffect(() => {
        if (!isAuthenticated) {
            navigate('/login');
            return;
        }
    }, [isAuthenticated, navigate]);

    useEffect(() => {
        if (!accessToken || !currentUserId) return;

        const loadFriends = async () => {
            try {
                const res = await authFetch(`${API_BASE}/api/Friend/MyFriends`);
                if (!res.ok) {
                    throw new Error(`HTTP ${res.status}`);
                }
                const friendsData = await res.json();

                if (Array.isArray(friendsData) && friendsData.length > 0) {
                    setFriends(friendsData);
                } else {
                    setFriends([]);
                }
                setLoading(false);
            } catch (e) {
                console.error('Error loading friends:', e);
                setError(e.message || 'Помилка завантаження друзів');
                setLoading(false);
            }
        };

        loadFriends();
    }, [accessToken, currentUserId]);

    const toggleUserSelection = (userId) => {
        setSelectedUsers(prev =>
            prev.includes(userId)
                ? prev.filter(id => id !== userId)
                : [...prev, userId]
        );
    };

    const handleCreate = async (e) => {
        e.preventDefault();
        if (!title.trim()) {
            setError('Назва чату обов\'язкова');
            return;
        }

        setError(null);

        try {
            const res = await authFetch(`${API_BASE}/api/Chat/group`, {
                method: 'POST',
                headers: {
                    'Content-Type': 'application/json'
                },
                body: JSON.stringify({ title })
            });

            if (!res.ok) throw new Error(`HTTP ${res.status}`);
            const { chatId } = await res.json();

            for (const userId of selectedUsers) {
                try {
                    await authFetch(`${API_BASE}/api/Chat/group/${chatId}/members/${userId}`, {
                        method: 'POST'
                    });
                } catch (addError) {
                    console.error(`Помилка при додаванні користувача ${userId}:`, addError);
                }
            }

            navigate(`/chat/${chatId}`);
        } catch (e) {
            setError(e.message || 'Помилка створення чату');
        }
    };

    if (loading) return <p>Завантаження…</p>;
    if (!isAuthenticated) return <p>Авторизуйтесь для доступу</p>;

    return (
        <div style={{ maxWidth: 600, margin: '24px auto', padding: '0 12px' }}>
            <h2>Створити груповий чат</h2>
            
            {error && <p style={{ color: 'red' }}>{error}</p>}

            <form onSubmit={handleCreate}>
                <div style={{ marginBottom: 20 }}>
                    <label style={{ display: 'block', marginBottom: 8 }}>
                        Назва чату:
                    </label>
                    <input
                        value={title}
                        onChange={e => setTitle(e.target.value)}
                        placeholder="Введіть назву чату"
                        style={{
                            width: '100%',
                            padding: 10,
                            border: '1px solid #ddd',
                            borderRadius: 4,
                            boxSizing: 'border-box'
                        }}
                    />
                </div>

                <div style={{ marginBottom: 20 }}>
                    <label style={{ display: 'block', marginBottom: 12 }}>
                        Оберіть друзів:
                    </label>
                    {friends.length === 0 ? (
                        <p style={{ color: '#666' }}>У вас поки немає друзів. Додайте друзів, щоб почати груповий чат.</p>
                    ) : (
                        <div style={{ border: '1px solid #ddd', borderRadius: 4, padding: 12, maxHeight: 300, overflowY: 'auto' }}>
                            {friends.map(friend => (
                                <div key={friend.id} style={{ marginBottom: 10, display: 'flex', alignItems: 'center' }}>
                                    <input
                                        type="checkbox"
                                        id={`friend-${friend.id}`}
                                        checked={selectedUsers.includes(friend.id)}
                                        onChange={() => toggleUserSelection(friend.id)}
                                        style={{ marginRight: 10, cursor: 'pointer' }}
                                    />
                                    <label htmlFor={`friend-${friend.id}`} style={{ cursor: 'pointer', flex: 1 }}>
                                        <div style={{ display: 'flex', alignItems: 'center', gap: 8 }}>
                                            <Avatar url={friend.profilePictureUrl} name={friend.name ?? friend.userName} size={32} />
                                            <div>
                                                <div style={{ fontWeight: 500 }}>{friend.name ?? friend.userName}</div>
                                                <div style={{ fontSize: 12, color: '#666' }}>{friend.email}</div>
                                            </div>
                                        </div>
                                    </label>
                                </div>
                            ))}
                        </div>
                    )}
                    <p style={{ fontSize: 12, color: '#666', marginTop: 8 }}>
                        Обрано: {selectedUsers.length} друзів
                    </p>
                </div>

                <div style={{ display: 'flex', gap: 10 }}>
                    <button
                        type="submit"
                        disabled={friends.length === 0 || !title.trim()}
                        style={{
                            flex: 1,
                            padding: 10,
                            background: friends.length === 0 || !title.trim() ? '#ccc' : '#007bff',
                            color: 'white',
                            border: 'none',
                            borderRadius: 4,
                            cursor: friends.length === 0 || !title.trim() ? 'not-allowed' : 'pointer'
                        }}
                    >
                        Створити чат
                    </button>
                    <button
                        type="button"
                        onClick={() => navigate('/chats')}
                        style={{
                            flex: 1,
                            padding: 10,
                            background: '#6c757d',
                            color: 'white',
                            border: 'none',
                            borderRadius: 4,
                            cursor: 'pointer'
                        }}
                    >
                        Скасувати
                    </button>
                </div>
            </form>
        </div>
    );
}