import { useEffect, useState } from 'react';
import { authFetch } from '../hooks/authFetch';
import Avatar from './Avatar';
import Modal from './Modal';

const API_BASE = 'https://localhost:7142';

export default function AddUsersToChatModal({ isOpen, chatId, onClose, onSuccess }) {
    const [friends, setFriends] = useState([]);
    const [currentMembers, setCurrentMembers] = useState([]);
    const [selectedUsers, setSelectedUsers] = useState([]);
    const [loading, setLoading] = useState(true);
    const [submitting, setSubmitting] = useState(false);
    const [error, setError] = useState(null);

    useEffect(() => {
        if (!isOpen || !chatId) return;

        const loadData = async () => {
            try {
                setLoading(true);
                setError(null);
                setSelectedUsers([]);

                const chatsRes = await authFetch(`${API_BASE}/api/Chat/chats`);
                if (!chatsRes.ok) throw new Error('Не вдалося завантажити чати');
                
                const chats = await chatsRes.json();
                const chat = chats.find(c => String(c.id).toLowerCase() === String(chatId).toLowerCase());
                
                if (!chat) {
                    setError('Чат не знайдено');
                    return;
                }

                const memberIds = (chat.userChats || []).map(uc => 
                    (uc.userId || uc.UserId || '').toString().toLowerCase()
                );
                setCurrentMembers(memberIds);

                const friendsRes = await authFetch(`${API_BASE}/api/Friend/MyFriends`);
                if (!friendsRes.ok) throw new Error('Не вдалося завантажити друзів');
                
                const friendsData = await friendsRes.json();

                const availableFriends = (friendsData || []).filter(friend => 
                    !memberIds.includes(String(friend.id).toLowerCase())
                );
                
                setFriends(availableFriends);
            } catch (e) {
                console.error('Помилка завантаження даних:', e);
                setError(e.message || 'Помилка завантаження даних');
            } finally {
                setLoading(false);
            }
        };

        loadData();
    }, [isOpen, chatId]);

    const toggleUserSelection = (userId) => {
        setSelectedUsers(prev =>
            prev.includes(userId)
                ? prev.filter(id => id !== userId)
                : [...prev, userId]
        );
    };

    const handleAddUsers = async (e) => {
        e.preventDefault();
        
        if (selectedUsers.length === 0) {
            setError('Виберіть хоча б одного користувача');
            return;
        }

        setSubmitting(true);
        setError(null);

        try {
            let successCount = 0;
            let failedCount = 0;
            const failedUsers = [];

            for (const userId of selectedUsers) {
                try {
                    const res = await authFetch(
                        `${API_BASE}/api/Chat/group/${chatId}/members/${userId}`,
                        { method: 'POST' }
                    );

                    if (!res.ok) {
                        throw new Error(`HTTP ${res.status}`);
                    }
                    successCount++;
                } catch (addError) {
                    failedCount++;
                    const failedUser = friends.find(f => f.id === userId);
                    failedUsers.push(failedUser?.name || failedUser?.userName || userId);
                    console.error(`Помилка при додаванні користувача ${userId}:`, addError);
                }
            }

            if (successCount > 0) {
                alert(`Успішно додано ${successCount} користувача(ів) до чату`);
                onSuccess?.();
                onClose();
            } else if (failedCount > 0) {
                setError(`Помилка при додаванні користувачів: ${failedUsers.join(', ')}`);
            }
        } catch (e) {
            setError(e.message || 'Помилка додавання користувачів');
        } finally {
            setSubmitting(false);
        }
    };

    return (
        <Modal 
            isOpen={isOpen} 
            title="Додати користувачів до чату" 
            onClose={onClose}
            size="medium"
        >
            {error && (
                <div style={{
                    padding: 12,
                    background: '#f8d7da',
                    color: '#721c24',
                    border: '1px solid #f5c6cb',
                    borderRadius: 4,
                    marginBottom: 16
                }}>
                    {error}
                </div>
            )}

            {loading ? (
                <div style={{ padding: 20, textAlign: 'center', color: '#666' }}>
                    Завантаження…
                </div>
            ) : friends.length === 0 ? (
                <div style={{
                    padding: 20,
                    background: '#e7f3ff',
                    border: '1px solid #b3d9ff',
                    borderRadius: 4,
                    color: '#004085',
                    textAlign: 'center'
                }}>
                    <p>Немає доступних друзів для додавання</p>
                    <p style={{ fontSize: 14, margin: 0 }}>
                        Всі ваші друзі вже в цьому чаті
                    </p>
                </div>
            ) : (
                <form onSubmit={handleAddUsers}>
                    <div style={{
                        border: '1px solid #ddd',
                        borderRadius: 4,
                        padding: 12,
                        maxHeight: 350,
                        overflowY: 'auto',
                        marginBottom: 16
                    }}>
                        {friends.map(friend => (
                            <div
                                key={friend.id}
                                style={{
                                    display: 'flex',
                                    alignItems: 'center',
                                    padding: 10,
                                    borderBottom: '1px solid #f0f0f0',
                                }}
                            >
                                <input
                                    type="checkbox"
                                    id={`user-${friend.id}`}
                                    checked={selectedUsers.includes(friend.id)}
                                    onChange={() => toggleUserSelection(friend.id)}
                                    style={{
                                        marginRight: 12,
                                        cursor: 'pointer',
                                        width: 18,
                                        height: 18
                                    }}
                                />
                                <label
                                    htmlFor={`user-${friend.id}`}
                                    style={{
                                        cursor: 'pointer',
                                        flex: 1,
                                        display: 'flex',
                                        alignItems: 'center',
                                        gap: 10
                                    }}
                                >
                                    <Avatar
                                        url={friend.profilePictureUrl}
                                        name={friend.name ?? friend.userName}
                                        size={36}
                                    />
                                    <div>
                                        <div style={{ fontWeight: 500, fontSize: 14 }}>
                                            {friend.name ?? friend.userName}
                                        </div>
                                        <div style={{ fontSize: 12, color: '#666' }}>
                                            {friend.email}
                                        </div>
                                    </div>
                                </label>
                            </div>
                        ))}
                    </div>

                    <p style={{ fontSize: 13, color: '#666', marginBottom: 16 }}>
                        Обрано: <strong>{selectedUsers.length}</strong> користувача(ів)
                    </p>

                    <div style={{ display: 'flex', gap: 10 }}>
                        <button
                            type="submit"
                            disabled={selectedUsers.length === 0 || submitting}
                            style={{
                                flex: 1,
                                padding: 10,
                                background: selectedUsers.length === 0 || submitting ? '#ccc' : '#28a745',
                                color: 'white',
                                border: 'none',
                                borderRadius: 4,
                                cursor: selectedUsers.length === 0 || submitting ? 'not-allowed' : 'pointer',
                                fontWeight: 'bold',
                                fontSize: 14
                            }}
                        >
                            {submitting ? 'Додавання...' : 'Додати користувачів'}
                        </button>
                        <button
                            type="button"
                            onClick={onClose}
                            disabled={submitting}
                            style={{
                                flex: 1,
                                padding: 10,
                                background: '#6c757d',
                                color: 'white',
                                border: 'none',
                                borderRadius: 4,
                                cursor: submitting ? 'not-allowed' : 'pointer',
                                fontWeight: 'bold',
                                fontSize: 14
                            }}
                        >
                            Скасувати
                        </button>
                    </div>
                </form>
            )}
        </Modal>
    );
}