import { useEffect, useState } from 'react';
import { authFetch } from '../hooks/authFetch';
import Avatar from './Avatar';
import Modal from './Modal';
import './AddUsersToChatModal.css';

const API_BASE = 'https://localhost:7142';

export default function AddUsersToChatModal({ isOpen, chatId, onClose, onSuccess }) {
    const [friends, setFriends] = useState([]);
    const [currentMembers, setCurrentMembers] = useState([]);
    const [selectedUsers, setSelectedUsers] = useState([]);
    const [loading, setLoading] = useState(true);
    const [submitting, setSubmitting] = useState(false);
    const [error, setError] = useState(null);
    const [searchTerm, setSearchTerm] = useState('');

    useEffect(() => {
        if (!isOpen || !chatId) return;

        const loadData = async () => {
            try {
                setLoading(true);
                setError(null);
                setSelectedUsers([]);
                setSearchTerm('');

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

    const filteredFriends = friends.filter(friend =>
        (friend.name || friend.userName || '').toLowerCase().includes(searchTerm.toLowerCase()) ||
        (friend.email || '').toLowerCase().includes(searchTerm.toLowerCase())
    );

    return (
        <Modal 
            isOpen={isOpen} 
            title="Додати користувачів до чату" 
            onClose={onClose}
            size="medium"
        >
            {error && (
                <div className="modal-error-message">
                    <span className="modal-error-icon">⚠️</span>
                    <span>{error}</span>
                </div>
            )}

            {loading ? (
                <div className="modal-loading">
                    <div className="modal-spinner"></div>
                    <p>Завантаження…</p>
                </div>
            ) : friends.length === 0 ? (
                <div className="modal-empty-state">
                    <div className="modal-empty-icon">👥</div>
                    <p className="modal-empty-title">Немає доступних друзів</p>
                    <p className="modal-empty-description">
                        Всі ваші друзі вже в цьому чаті
                    </p>
                </div>
            ) : (
                <form onSubmit={handleAddUsers}>
                    <div className="modal-search-container">
                        <input
                            type="text"
                            placeholder="Пошук за іменем або email…"
                            value={searchTerm}
                            onChange={(e) => setSearchTerm(e.target.value)}
                            className="modal-search-input"
                        />
                        <span className="modal-search-icon">🔍</span>
                    </div>

                    {filteredFriends.length === 0 ? (
                        <div className="modal-no-results">
                            <p>Користувачі не знайдені</p>
                        </div>
                    ) : (
                        <div className="modal-users-list">
                            {filteredFriends.map(friend => (
                                <div
                                    key={friend.id}
                                    className={`modal-user-item ${selectedUsers.includes(friend.id) ? 'modal-user-item--selected' : ''}`}
                                >
                                    <input
                                        type="checkbox"
                                        id={`user-${friend.id}`}
                                        checked={selectedUsers.includes(friend.id)}
                                        onChange={() => toggleUserSelection(friend.id)}
                                        className="modal-user-checkbox"
                                    />
                                    <label
                                        htmlFor={`user-${friend.id}`}
                                        className="modal-user-label"
                                    >
                                        <Avatar
                                            url={friend.profilePictureUrl}
                                            name={friend.name ?? friend.userName}
                                            size={40}
                                        />
                                        <div className="modal-user-info">
                                            <div className="modal-user-name">
                                                {friend.name ?? friend.userName}
                                            </div>
                                            <div className="modal-user-email">
                                                {friend.email}
                                            </div>
                                        </div>
                                    </label>
                                </div>
                            ))}
                        </div>
                    )}

                    <div className="modal-selection-info">
                        <span>Обрано: <strong>{selectedUsers.length}</strong> користувача(ів)</span>
                    </div>

                    <div className="modal-actions">
                        <button
                            type="submit"
                            disabled={selectedUsers.length === 0 || submitting}
                            className="modal-btn modal-btn--primary"
                        >
                            {submitting ? 'Додавання...' : '✓ Додати користувачів'}
                        </button>
                        <button
                            type="button"
                            onClick={onClose}
                            disabled={submitting}
                            className="modal-btn modal-btn--secondary"
                        >
                            Скасувати
                        </button>
                    </div>
                </form>
            )}
        </Modal>
    );
}