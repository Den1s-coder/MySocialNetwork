import { useState, useCallback, useEffect, useRef } from 'react';
import { useParams, useNavigate } from 'react-router-dom';
import { useChatHub } from '../hooks/useChatHub';
import { useAuth } from '../hooks/useAuth';
import { authFetch } from '../hooks/authFetch';
import Avatar from '../components/Avatar';
import ReactionBar from '../components/ReactionBar';
import AddUsersToChatModal from '../components/AddUsersToChatModal';

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
            <div style={{ maxWidth: 1200, margin: '24px auto', padding: '0 12px', display: 'flex', gap: 16 }}>
                <div style={{ flex: 1 }}>
                    <div style={{ display: 'flex', justifyContent: 'space-between', alignItems: 'center', marginBottom: 16 }}>
                        <div style={{ display: 'flex', alignItems: 'center', gap: 12 }}>
                            <Avatar url={chatAvatar} name={chatTitle} />
                            <h2 style={{ margin: 0 }}>{chatTitle ? `Чат з ${chatTitle}` : `Чат ${chatId}`}</h2>
                        </div>
                        <div style={{ display: 'flex', gap: 8 }}>
                            <button 
                                onClick={() => setShowParticipants(!showParticipants)}
                                style={{ 
                                    padding: '8px 12px', 
                                    background: '#6c757d', 
                                    color: 'white', 
                                    border: 'none', 
                                    borderRadius: 4,
                                    cursor: 'pointer'
                                }}
                            >
                                {showParticipants ? '← Приховати' : '→ Учасники'}
                            </button>
                            <button onClick={() => navigate('/chats')} style={{ padding: '8px 12px' }}>← До списку чатів</button>
                        </div>
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
                                            <div style={{
                                                background: isCurrentUser ? '#007bff' : '#e9ecef',
                                                color: isCurrentUser ? 'white' : 'black',
                                                padding: 8,
                                                borderRadius: 8,
                                                display: 'inline-block',
                                                wordWrap: 'break-word'
                                            }}>
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

                    <form onSubmit={handleSend} style={{ display: 'flex', gap: 8 }}>
                        <input
                            value={text}
                            onChange={e => setText(e.target.value)}
                            placeholder="Введіть повідомлення..."
                            style={{ flex: 1, padding: 8, border: '1px solid #ddd', borderRadius: 4 }}
                        />
                        <button
                            type="submit"
                            disabled={!connected || !text.trim()}
                            style={{ padding: '8px 16px', background: '#007bff', color: 'white', border: 'none', borderRadius: 4 }}
                        >
                            Відправити
                        </button>
                    </form>
                </div>

                {showParticipants && (
                    <div style={{
                        width: 280,
                        border: '1px solid #ddd',
                        borderRadius: 8,
                        padding: 16,
                        background: '#f9f9f9',
                        height: 'fit-content',
                        maxHeight: 'calc(100vh - 120px)',
                        overflowY: 'auto'
                    }}>
                        <h3 style={{ marginTop: 0, marginBottom: 16 }}>Учасники ({participants.length})</h3>
                        
                        <div style={{ marginBottom: 16 }}>
                            {participants.map(participant => (
                                <div
                                    key={participant.id}
                                    style={{
                                        display: 'flex',
                                        alignItems: 'center',
                                        gap: 10,
                                        padding: 10,
                                        background: 'white',
                                        borderRadius: 6,
                                        marginBottom: 8,
                                        border: '1px solid #e0e0e0'
                                    }}
                                >
                                    <Avatar url={participant.pic} name={participant.name} size={32} />
                                    <div style={{ flex: 1 }}>
                                        <div style={{ fontWeight: 500, fontSize: 14 }}>
                                            {participant.name}
                                            {participant.id === String(currentUserId).toLowerCase() && ' (Ви)'}
                                        </div>
                                        <div style={{ fontSize: 12, color: '#666' }}>
                                            {getRoleName(participant.role)}
                                        </div>
                                    </div>
                                </div>
                            ))}
                        </div>

                        {chatType !== 0 && (userRole === 0 || userRole === 1) && (
                            <div style={{
                                paddingTop: 16,
                                borderTop: '1px solid #ddd',
                                display: 'flex',
                                flexDirection: 'column',
                                gap: 10
                            }}>
                                <button
                                    onClick={() => setShowAddUserModal(true)}
                                    style={{
                                        width: '100%',
                                        padding: '10px 12px',
                                        background: '#28a745',
                                        color: 'white',
                                        border: 'none',
                                        borderRadius: 4,
                                        cursor: 'pointer',
                                        fontWeight: 'bold'
                                    }}
                                >
                                    + Додати користувача
                                </button>
                                <button
                                    onClick={handleDeleteChat}
                                    disabled={deleting}
                                    style={{
                                        width: '100%',
                                        padding: '10px 12px',
                                        background: '#dc3545',
                                        color: 'white',
                                        border: 'none',
                                        borderRadius: 4,
                                        cursor: deleting ? 'not-allowed' : 'pointer',
                                        fontWeight: 'bold',
                                        opacity: deleting ? 0.6 : 1
                                    }}
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