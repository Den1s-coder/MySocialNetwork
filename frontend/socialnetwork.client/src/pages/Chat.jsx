import { useState, useCallback, useEffect } from 'react';
import { useParams, useNavigate } from 'react-router-dom';
import { useChatHub } from '../hooks/useChatHub';
import { useAuth } from '../hooks/useAuth';

const BASE_URL = 'https://localhost:7142';
const API_BASE = 'https://localhost:7142';

export default function Chat() {
    const { chatId } = useParams();
    const navigate = useNavigate();
    const { token, isAuthenticated, currentUserId, currentUserName } = useAuth();
    const [messages, setMessages] = useState([]);
    const [loading, setLoading] = useState(true);

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
        if (!token || !chatId) return;

        const loadMessages = async () => {
            try {
                console.log('Loading messages for chatId:', chatId);
                const res = await fetch(`${API_BASE}/api/Chat/chats/${chatId}/messages`, {
                    headers: { 'Authorization': `Bearer ${token}` }
                });
                if (!res.ok) throw new Error(`HTTP ${res.status}`);
                const data = await res.json();

                const sortedMessages = data.sort((a, b) => new Date(a.sentAt) - new Date(b.sentAt));
                setMessages(sortedMessages);
                setLoading(false);
            } catch (e) {
                console.error('Помилка завантаження повідомлень:', e);
                setLoading(false);
            }
        };
        loadMessages();
    }, [chatId, token]);

    const onMessage = useCallback((msg) => {
        setMessages(prev => {
            const updatedMessages = [...prev, msg];
            return updatedMessages.sort((a, b) => new Date(a.sentAt) - new Date(b.sentAt));
        });
    }, []);

    const getToken = () => token;

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

        console.log('Sending message:', { chatId, content: text.trim() });
        await sendMessage(chatId, text.trim());
        setText('');
    };

    if (loading) return <p>Завантаження чату…</p>;
    if (!isAuthenticated) return <p>Авторизуйтесь для доступу до чату</p>;

    return (
        <div style={{ maxWidth: 800, margin: '24px auto', padding: '0 12px' }}>
            <div style={{ display: 'flex', justifyContent: 'space-between', alignItems: 'center', marginBottom: 16 }}>
                <h2>Чат {chatId}</h2>
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
                        const isCurrentUser = m.senderId === currentUserId;

                        return (
                            <div
                                key={m.id ?? `${m.chatId}-${m.sentAt}-${m.senderId}`}
                                style={{
                                    marginBottom: 12,
                                    display: 'flex',
                                    justifyContent: isCurrentUser ? 'flex-end' : 'flex-start'
                                }}
                            >
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