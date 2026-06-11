import { useState, useCallback, useEffect, useRef } from 'react';
import { useParams, useNavigate } from 'react-router-dom';
import { useChatHub } from '../hooks/useChatHub';
import { useAuth } from '../hooks/useAuth';
import { authFetch } from '../hooks/authFetch';
import Avatar from '../components/Avatar';
import ReactionBar from '../components/ReactionBar';
import AddUsersToChatModal from '../components/AddUsersToChatModal';
import EmojiPickerButton from '../components/EmojiPickerButton';
import { AiFillEdit, AiFillCamera } from "react-icons/ai";
import './Chat.css';

const BASE_URL = 'https://localhost:7142';
const API_BASE = 'https://localhost:7142';

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
    const [chatType, setChatType] = useState(null);
    const [participants, setParticipants] = useState([]);
    const [showParticipants, setShowParticipants] = useState(false);
    const [userRole, setUserRole] = useState(null);
    const [deleting, setDeleting] = useState(false);
    const [showAddUserModal, setShowAddUserModal] = useState(false);
    const [uploadingPhoto, setUploadingPhoto] = useState(false);
    const [selectedPhoto, setSelectedPhoto] = useState(null);
    const [editingMessageId, setEditingMessageId] = useState(null);
    const [editText, setEditText] = useState('');
    const [text, setText] = useState('');
    const fileInputRef = useRef(null);

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

                let participantsData = {};
                if (chatsRes.ok) {
                    const chats = await chatsRes.json();
                    const chat = (chats || []).find(c => String(c.id).toLowerCase() === String(chatId).toLowerCase());
                    if (chat) {
                        const ucs = chat.userChats || chat.UserChats || [];
                        const normalizedParticipants = [];
                        
                        (ucs || []).forEach(uc => {
                            const id = (uc.userId || uc.UserId || uc.user?.id || '')?.toString().toLowerCase();
                            const name = uc.userName || uc.UserName || uc.user?.name || uc.user?.name || '';
                            const pic = uc.profilePictureUrl || uc.ProfilePictureUrl || uc.user?.profilePictureUrl || uc.user?.profilePictureUrl || null;
                            const role = uc.role || uc.Role || 0;
                            
                            if (id) {
                                participantsData[id] = { userName: name, profilePictureUrl: pic, role };
                                normalizedParticipants.push({ id, name, pic, role });
                                
                                if (id === String(currentUserId).toLowerCase()) {
                                    setUserRole(role);
                                }
                            }
                        });

                        setParticipants(normalizedParticipants);

                        const title = (() => {
                            const type = chat.type;
                            const isPrivate = type === 0 || String(type).toLowerCase() === 'private';
                            if (isPrivate) {
                                const other = Object.entries(participantsData).find(([id]) => id !== String(currentUserId).toLowerCase());
                                if (other) return participantsData[other[0]].userName || null;
                            }
                            return chat.title || null;
                        })();
                        setChatTitle(title);
                        setChatAvatar(chat.avatarUrl || chat.userProfilePictureUrl || chat.profilePictureUrl || null);
                        setChatType(chat.type);
                    }
                }

                updateParticipants(participantsData);

                const mapped = (data || []).map(m => {
                    const sid = (m.senderId || '').toString().toLowerCase();
                    const p = participantsData[sid];
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

    const onMessageUpdated = useCallback((updatedMsg) => {
        console.log('onMessageUpdated called with:', updatedMsg);
        setMessages(prev => 
            prev.map(msg => 
                msg.id === updatedMsg.id 
                    ? { ...msg, content: updatedMsg.content, editedAt: updatedMsg.editedAt }
                    : msg
            )
        );
        setEditingMessageId(null);
        setEditText('');
    }, []);

    const getToken = useCallback(() => accessToken, [accessToken]);

    const { connected, sendMessage, joinChat, editMessage } = useChatHub({
        baseUrl: BASE_URL,
        getToken,
        chatId,
        onMessage,
        onMessageUpdated
    });

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

    if (loading) return <p>Завантаження чату…</p>;
    if (!isAuthenticated) return <p>Авторизуйтесь для доступу до чату</p>;

    return (
        <>
            <div className="chat-container">
                <div className="chat-main">
                    <div className="chat-header">
                        <div className="chat-title-wrapper">
                            <Avatar url={chatAvatar} name={chatTitle} />
                            <h2>{chatTitle ? `Чат з ${chatTitle}` : `Чат ${chatId}`}</h2>
                        </div>
                        <div className="chat-header-buttons">
                            <button 
                                onClick={() => setShowParticipants(!showParticipants)}
                                className="chat-btn chat-btn--secondary"
                            >
                                {showParticipants ? '← Приховати' : '→ Учасники'}
                            </button>
                            <button onClick={() => navigate('/chats')} className="chat-btn chat-btn--secondary">
                                ← До списку чатів
                            </button>
                        </div>
                    </div>

                    <div className="chat-messages">
                        {messages.length === 0 ? (
                            <p className="chat-empty">Повідомлень поки немає</p>
                        ) : (
                            <>
                                {sortedDateKeys.map(dateKey => (
                                    <div key={dateKey}>
                                        <div className="chat-date-separator">
                                            <span className="chat-date-label">{formatDate(groupedMessages[dateKey][0].sentAt)}</span>
                                        </div>
                                        {groupedMessages[dateKey].map(m => {
                                            const isCurrentUser = String(m.senderId).toLowerCase() === String(currentUserId).toString().toLowerCase();
                                            const isEditing = editingMessageId === m.id;

                                            return (
                                                <div
                                                    key={m.id ?? `${m.chatId}-${m.sentAt}-${m.senderId}`}
                                                    className={`chat-message ${isCurrentUser ? 'chat-message--sent' : 'chat-message--received'}`}
                                                >
                                                    {!isCurrentUser && <Avatar url={m.senderProfilePictureUrl} name={m.senderName} size={36} />}
                                                    <div className="chat-message-content">
                                                        <div className="chat-message-meta">
                                                            {isCurrentUser ? currentUserName : m.senderName || m.senderId} • {formatTime(m.sentAt)}
                                                            {m.editedAt && <span className="chat-message-edited"> (відредаговано)</span>}
                                                        </div>
                                                        {isEditing ? (
                                                            <div className="chat-message-edit-form">
                                                                <input
                                                                    type="text"
                                                                    value={editText}
                                                                    onChange={(e) => setEditText(e.target.value)}
                                                                    className="chat-message-edit-input"
                                                                    autoFocus
                                                                />
                                                                <button
                                                                    onClick={handleSaveEdit}
                                                                    className="chat-btn chat-btn--primary"
                                                                >
                                                                    Зберегти
                                                                </button>
                                                                <button
                                                                    onClick={handleCancelEdit}
                                                                    className="chat-btn chat-btn--secondary"
                                                                >
                                                                    Відміна
                                                                </button>
                                                            </div>
                                                        ) : (
                                                            <>
                                                                <div className={`chat-message-bubble ${isCurrentUser ? 'chat-message-bubble--sent' : 'chat-message-bubble--received'}`}>
                                                                    {m.photoUrl && (
                                                                        <img 
                                                                            src={m.photoUrl} 
                                                                            alt="Chat photo" 
                                                                            className="chat-message-photo"
                                                                        />
                                                                    )}
                                                                    {m.content && <p>{m.content}</p>}
                                                                </div>
                                                                {isCurrentUser && (
                                                                    <button
                                                                        onClick={() => handleEditMessage(m.id, m.content)}
                                                                        className="chat-message-action-btn"
                                                                        title="Редагувати повідомлення"
                                                                        >
                                                                            <AiFillEdit size={20} style={{color: "gray"}} />
                                                                    </button>
                                                                )}
                                                            </>
                                                        )}
                                                        <ReactionBar 
                                                            reactions={m.reactions || []}
                                                            currentUserReactionCode={m.currentUserReactionCode}
                                                            entityId={m.id}
                                                            entityType="Message"
                                                            authed={true}
                                                            currentUserId={currentUserId}
                                                            entityAuthorId={m.senderId}
                                                            onReactionChanged={(updatedReactions, newCode) => {
                                                                setMessages(messages.map(msg => 
                                                                    msg.id === m.id 
                                                                        ? { ...msg, reactions: updatedReactions, currentUserReactionCode: newCode }
                                                                        : msg
                                                                ));
                                                            }}
                                                        />
                                                    </div>
                                                    {isCurrentUser && <Avatar url={m.senderProfilePictureUrl} name={m.senderName} size={36} />}
                                                </div>
                                            );
                                        })}
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
                            <AiFillCamera size={ 20 } /> {selectedPhoto ? 'Фото вибрано' : 'Фото'}
                        </button>
                        <div style={{ position: 'relative' }} className="chat-input-container">
                            <input
                                value={text}
                                onChange={e => setText(e.target.value)}
                                placeholder="Введіть повідомлення..."
                                className="chat-input"
                            />
                            <div style={{ position: 'absolute', bottom: 8, left: 8 }}>
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
                            ))}
                        </div>

                        {chatType !== 0 && (userRole === 0 || userRole === 1) && (
                            <div className="chat-sidebar-actions">
                                <button
                                    onClick={() => setShowAddUserModal(true)}
                                    className="chat-btn chat-btn--primary"
                                >
                                    + Додати користувача
                                </button>
                                <button
                                    onClick={handleDeleteChat}
                                    disabled={deleting}
                                    className="chat-btn chat-btn--danger"
                                >
                                    {deleting ? 'Видалення...' : 'Видалити чат'}
                                </button>
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