import { useState, useCallback, useEffect, useRef } from 'react';
import { useParams, useNavigate } from 'react-router-dom';
import { useChatHub } from '../hooks/useChatHub';
import { useAuth } from '../hooks/useAuth';
import { authFetch } from '../hooks/authFetch';
import Avatar from '../components/Avatar';

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
    );
}