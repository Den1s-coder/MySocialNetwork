import { useState, useCallback, useEffect, useRef } from 'react';
import { useParams, useNavigate } from 'react-router-dom';
import { useChatHub } from '../hooks/useChatHub';
import { useAuth } from '../hooks/useAuth';
import { authFetch } from '../hooks/authFetch';
import Avatar from '../components/Avatar';

const BASE_URL = import.meta.env.VITE_API_BASE || '';
const API_BASE = import.meta.env.VITE_API_BASE || '';

const formatDate = (date) => {
    const d = new Date(date);
    const today = new Date();
    const yesterday = new Date(today);
    yesterday.setDate(yesterday.getDate() - 1);

    const dateStr = d.toLocaleDateString('uk-UA');
    const todayStr = today.toLocaleDateString('uk-UA');
    const yesterdayStr = yesterday.toLocaleDateString('uk-UA');

    if (dateStr === todayStr) return 'Сьогодні';
    if (dateStr === yesterdayStr) return 'Вчора';
    return dateStr;
};

const formatTime = (date) => {
    const d = new Date(date);
    return d.toLocaleTimeString('uk-UA', { hour: '2-digit', minute: '2-digit' });
}

const getDateKey = (date) => {
    const d = new Date(date);
    return d.toLocaleDateString('uk-UA');
};

export default function Chat() {
    const { chatId } = useParams();
    const navigate = useNavigate();
    const { accessToken, isAuthenticated, currentUserId, currentUserName } = useAuth();
    const [messages, setMessages] = useState([]);
    const [loading, setLoading] = useState(true);
    const [chatTitle, setChatTitle] = useState(null);
    const [chatAvatar, setChatAvatar] = useState(null);

    const [participantsMap, setParticipantsMap] = useState({});
    const participantsRef = useRef({});
    const updateParticipants = (map) => { setParticipantsMap(map); participantsRef.current = map; };

    if (!chatId) {
        return <div>Chat ID not found in URL</div>;
    }

    const isValidGuid = /^[0-9a-f]{8}-[0-9a-f]{4}-[0-9a-f]{4}-[0-9a-f]{4}-[0-9a-f]{12}$/i.test(chatId);
    if (!isValidGuid) {
        return <div>Invalid chat ID: {chatId}</div>;
    }

    useEffect(() => {
        if (!isAuthenticated) {
            navigate('/login');
            return;
        }
    }, [isAuthenticated, navigate]);

    useEffect(() => {
        if (!accessToken || !chatId) return;

        const loadMessagesAndMeta = async () => {
            setLoading(true);
            try {
                const res = await authFetch(`${API_BASE}/api/Chat/chats/${chatId}/messages`, {
                    headers: { 'Authorization': `Bearer ${accessToken}` }
                });
                if (!res.ok) throw new Error(`HTTP ${res.status}`);
                const data = await res.json();

                const chatsRes = await authFetch(`${API_BASE}/api/Chat/chats`, {
                    headers: { 'Authorization': `Bearer ${accessToken}` }
                });

                let participants = {};
                if (chatsRes.ok) {
                    const chats = await chatsRes.json();
                    const chat = (chats || []).find(c => String(c.id).toLowerCase() === String(chatId).toLowerCase());
                    if (chat) {
                        const ucs = chat.userChats || chat.UserChats || [];
                        (ucs || []).forEach(uc => {
                            const id = (uc.userId || uc.UserId || uc.user?.id || '')?.toString().toLowerCase();
                            const name = uc.userName || uc.UserName || uc.user?.name || uc.user?.name || '';
                            const pic = uc.profilePictureUrl || uc.ProfilePictureUrl || uc.user?.profilePictureUrl || uc.user?.profilePictureUrl || null;
                            if (id) participants[id] = { userName: name, profilePictureUrl: pic };
                        });

                        const title = (() => {
                            const type = chat.type;
                            const isPrivate = type === 0 || String(type).toLowerCase() === 'private';
                            if (isPrivate) {
                                const other = Object.entries(participants).find(([id]) => id !== String(currentUserId).toLowerCase());
                                if (other) return participants[other[0]].userName || null;
                            }
                            return chat.title || null;
                        })();
                        setChatTitle(title);
                        setChatAvatar(chat.avatarUrl || chat.userProfilePictureUrl || chat.profilePictureUrl || null);
                    }
                }

                updateParticipants(participants);

                const mapped = (data || []).map(m => {
                    const sid = (m.senderId || '').toString().toLowerCase();
                    const p = participants[sid];
                    return {
                        ...m,
                        senderName: m.senderName || (p && p.userName) || m.senderName || null,
                        senderProfilePictureUrl: m.senderProfilePictureUrl || (p && p.profilePictureUrl) || null
                    };
                });

                const sortedMessages = mapped.sort((a, b) => new Date(a.sentAt) - new Date(b.sentAt));
                setMessages(sortedMessages);
            } catch (e) {
                console.error('Помилка завантаження повідомлень або метаданих чату:', e);
            } finally {
                setLoading(false);
            }
        };

        loadMessagesAndMeta();
    }, [chatId, accessToken, currentUserId]);

    const onMessage = useCallback((msg) => {
        const sid = (msg.senderId || '').toString().toLowerCase();
        const p = participantsRef.current[sid];
        const enriched = {
            ...msg,
            senderName: msg.senderName || (p && p.userName) || msg.senderName || null,
            senderProfilePictureUrl: msg.senderProfilePictureUrl || (p && p.profilePictureUrl) || null
        };

        setMessages(prev => {
            const updatedMessages = [...prev, enriched];
            return updatedMessages.sort((a, b) => new Date(a.sentAt) - new Date(b.sentAt));
        });
    }, []);

    const getToken = useCallback(() => accessToken, [accessToken]);

    const { connected, sendMessage, joinChat } = useChatHub({
        baseUrl: BASE_URL,
        getToken,
        chatId,
        onMessage
    });

    const [text, setText] = useState('');

    const handleSend = async (e) => {
        e.preventDefault();
        if (!text.trim() || !connected || !chatId) return;
        await sendMessage(chatId, text.trim());
        setText('');
    };

    if (loading) return <p>Завантаження чату…</p>;
    if (!isAuthenticated) return <p>Авторизуйтесь для доступу до чату</p>;

    const handlePhotoSelect = (e) => {
        const file = e.target.files?.[0];
        if (file) {
            setSelectedPhoto(file);
        }
    };

    const uploadPhoto = async (file) => {
        try {
            const formData = new FormData();
            formData.append('file', file);

            const res = await authFetch(`${API_BASE}/api/File/upload`, {
                method: 'POST',
                headers: { 'Authorization': `Bearer ${accessToken}` },
                body: formData
            });

            if (!res.ok) throw new Error(`HTTP ${res.status}`);
            const { fileUrl } = await res.json();
            return fileUrl;
        } catch (e) {
            console.error('Помилка завантаження фото:', e);
            alert('Не вдалося завантажити фото. ' + (e.message || ''));
            return null;
        }
    };

    const handleSend = async (e) => {
        e.preventDefault();
        if ((!text.trim() && !selectedPhoto) || !connected || !chatId) return;

        setUploadingPhoto(true);
        try {
            let photoUrl = null;
            if (selectedPhoto) {
                photoUrl = await uploadPhoto(selectedPhoto);
                if (!photoUrl) {
                    setUploadingPhoto(false);
                    return;
                }
                setSelectedPhoto(null);
                if (fileInputRef.current) {
                    fileInputRef.current.value = '';
                }
            }

            await sendMessage(chatId, text.trim() || '', photoUrl);
            setText('');
        } finally {
            setUploadingPhoto(false);
        }
    };

    const handleEditMessage = (messageId, currentContent) => {
        setEditingMessageId(messageId);
        setEditText(currentContent);
    };

    const handleSaveEdit = async () => {
        if (!editText.trim()) {
            alert('Текст повідомлення не може бути порожнім');
            return;
        }

        try {
            await editMessage(editingMessageId, editText.trim());
        } catch (e) {
            console.error('Помилка редагування повідомлення:', e);
            alert('Не вдалося відредагувати повідомлення');
        }
    };

    const handleCancelEdit = () => {
        setEditingMessageId(null);
        setEditText('');
    };

    const handleDeleteChat = async () => {
        if (!window.confirm('Ви впевнені, що хочете видалити цей чат? Це не можна буде скасувати.')) {
            return;
        }

        setDeleting(true);
        try {
            const res = await authFetch(`${API_BASE}/api/Chat/${chatId}`, {
                method: 'DELETE',
                headers: { 'Authorization': `Bearer ${accessToken}` }
            });

            if (!res.ok) throw new Error(`HTTP ${res.status}`);

            navigate('/chats');
        } catch (e) {
            console.error('Помилка видалення чату:', e);
            alert('Не вдалося видалити чат. ' + (e.message || ''));
        } finally {
            setDeleting(false);
        }
    };

    const handleModalSuccess = () => {
        window.location.reload();
    };

    const getRoleName = (role) => {
        const roles = { 0: 'Owner', 1: 'Admin', 2: 'Member' };
        return roles[role] || 'Unknown';
    };

    const groupedMessages = messages.reduce((groups, message) => {
        const dateKey = getDateKey(message.sentAt);
        if (!groups[dateKey]) {
            groups[dateKey] = [];
        }
        groups[dateKey].push(message);
        return groups;
    }, {});

    const sortedDateKeys = Object.keys(groupedMessages).sort((a, b) => {
        const dateA = new Date(a);
        const dateB = new Date(b);
        return dateA - dateB;
    });

    return (
        <div style={{ maxWidth: 800, margin: '24px auto', padding: '0 12px' }}>
            <div style={{ display: 'flex', justifyContent: 'space-between', alignItems: 'center', marginBottom: 16 }}>
                <div style={{ display: 'flex', alignItems: 'center', gap: 12 }}>
                    <Avatar url={chatAvatar} name={chatTitle} />
                    <h2 style={{ margin: 0 }}>{chatTitle ? `Чат з ${chatTitle}` : `Чат ${chatId}`}</h2>
                </div>
                <button onClick={() => navigate('/chats')}>← До списку чатів</button>
            </div>

            <div style={{
                height: 400,
                border: '1px solid #ddd',
                borderRadius: 8,
                padding: 16,
                overflowY: 'auto',
                marginBottom: 16,
                background: '#f9f9f9'
            }}>
                {messages.length === 0 ? (
                    <p style={{ color: '#666', textAlign: 'center' }}>Повідомлень поки немає</p>
                ) : (
                    messages.map(m => {
                        const isCurrentUser = String(m.senderId).toLowerCase() === String(currentUserId).toString().toLowerCase();

                        return (
                            <div
                                key={m.id ?? `${m.chatId}-${m.sentAt}-${m.senderId}`}
                                style={{
                                    marginBottom: 12,
                                    display: 'flex',
                                    justifyContent: isCurrentUser ? 'flex-end' : 'flex-start',
                                    alignItems: 'flex-end',
                                    gap: 8
                                }}
                            >
                                {!isCurrentUser && <Avatar url={m.senderProfilePictureUrl} name={m.senderName} size={36} />}
                                <div style={{ maxWidth: '70%' }}>
                                    <div style={{
                                        fontSize: 12,
                                        color: '#666',
                                        marginBottom: 4,
                                        textAlign: isCurrentUser ? 'right' : 'left'
                                    }}>
                                        {isCurrentUser ? currentUserName : m.senderName || m.senderId} • {new Date(m.sentAt).toLocaleString()}
                                    </div>
                                ))}
                            </>
                        )}
                    </div>

                    <form onSubmit={handleSend} className="chat-input-form">
                        <input
                            type="file"
                            ref={fileInputRef}
                            onChange={handlePhotoSelect}
                            accept="image/*"
                            className="chat-input-file"
                            style={{ display: 'none' }}
                        />
                        <button
                            type="button"
                            onClick={() => fileInputRef.current?.click()}
                            disabled={uploadingPhoto}
                            className="chat-btn chat-btn--secondary"
                            title="Додати фото"
                        >
                            <AiFillCamera size={20} /> {selectedPhoto ? 'Фото вибрано' : 'Фото'}
                        </button>
                        <div className="chat-input-container">
                            <input
                                value={text}
                                onChange={e => setText(e.target.value)}
                                placeholder="Введіть повідомлення..."
                                className="chat-input"
                            />
                            <div className="emoji-button-wrapper">
                                <EmojiPickerButton
                                    onEmojiSelect={(emoji) => setText(prev => prev + emoji)}
                                />
                            </div>
                        </div>
                        <button
                            type="submit"
                            disabled={!connected || (!text.trim() && !selectedPhoto) || uploadingPhoto}
                            className="chat-btn chat-btn--primary"
                        >
                            {uploadingPhoto ? 'Завантаження...' : 'Відправити'}
                        </button>
                    </form>
                </div>

                {showParticipants && (
                    <div className="chat-sidebar">
                        <h3>Учасники ({participants.length})</h3>

                        <div className="chat-participants">
                            {participants.map(participant => (
                                <div key={participant.id} className="chat-participant">
                                    <Avatar url={participant.pic} name={participant.name} size={32} />
                                    <div className="chat-participant-info">
                                        <div className="chat-participant-name">
                                            {participant.name}
                                            {participant.id === String(currentUserId).toLowerCase() && ' (Ви)'}
                                        </div>
                                        <div className="chat-participant-role">
                                            {getRoleName(participant.role)}
                                        </div>
                                    </div>
                                </div>
                                {isCurrentUser && <Avatar url={m.senderProfilePictureUrl} name={m.senderName} size={36} />}
                            </div>
                        )}
                    </div>
                )}
            </div>

            <AddUsersToChatModal
                isOpen={showAddUserModal}
                chatId={chatId}
                onClose={() => setShowAddUserModal(false)}
                onSuccess={handleModalSuccess}
            />
        </>
    );
}