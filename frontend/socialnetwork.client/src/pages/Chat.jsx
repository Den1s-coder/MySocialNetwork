import { useState, useCallback, useEffect, useRef } from 'react';
import { useParams, useNavigate } from 'react-router-dom';
import { useChatHub } from '../hooks/useChatHub';
import { useAuth } from '../hooks/useAuth';
import { authFetch } from '../hooks/authFetch';
import Avatar from '../components/Avatar';
import ReactionBar from '../components/ReactionBar';
import AddUsersToChatModal from '../components/AddUsersToChatModal';
import './Chat.css';

const BASE_URL = 'https://localhost:7142';
const API_BASE = 'https://localhost:7142';

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
                            messages.map(m => {
                                const isCurrentUser = String(m.senderId).toLowerCase() === String(currentUserId).toString().toLowerCase();

                                return (
                                    <div
                                        key={m.id ?? `${m.chatId}-${m.sentAt}-${m.senderId}`}
                                        className={`chat-message ${isCurrentUser ? 'chat-message--sent' : 'chat-message--received'}`}
                                    >
                                        {!isCurrentUser && <Avatar url={m.senderProfilePictureUrl} name={m.senderName} size={36} />}
                                        <div className="chat-message-content">
                                            <div className="chat-message-meta">
                                                {isCurrentUser ? currentUserName : m.senderName || m.senderId} • {new Date(m.sentAt).toLocaleString()}
                                            </div>
                                            <div className={`chat-message-bubble ${isCurrentUser ? 'chat-message-bubble--sent' : 'chat-message-bubble--received'}`}>
                                                {m.content}
                                            </div>
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
                            })
                        )}
                    </div>

                    <form onSubmit={handleSend} className="chat-input-form">
                        <input
                            value={text}
                            onChange={e => setText(e.target.value)}
                            placeholder="Введіть повідомлення..."
                            className="chat-input"
                        />
                        <button
                            type="submit"
                            disabled={!connected || !text.trim()}
                            className="chat-btn chat-btn--primary"
                        >
                            Відправити
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