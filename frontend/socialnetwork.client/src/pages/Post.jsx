import { useEffect, useMemo, useState } from 'react';
import { useParams } from 'react-router-dom';

const API_BASE = 'https://localhost:7142';

export default function Post() {
    const { id } = useParams();
    const [post, setPost] = useState(null);
    const [comments, setComments] = useState([]);
    const [status, setStatus] = useState('loading'); 
    const [error, setError] = useState(null);

    const authed = useMemo(() => Boolean(localStorage.getItem('token')), []);
    const [commentText, setCommentText] = useState('');
    const [sendStatus, setSendStatus] = useState('idle'); 

    useEffect(() => {
        let cancelled = false;

        const load = async () => {
            setStatus('loading');
            setError(null);
            try {
                const resPost = await fetch(`${API_BASE}/api/Post/${id}`);
                if (!resPost.ok) throw new Error(`HTTP ${resPost.status}`);
                const postDto = await resPost.json();

                if (postDto?.isBanned) {
                    if (!cancelled) {
                        setPost(postDto);
                        setComments([]);
                        setStatus('ready');
                    }
                    return;
                }

                let comm = Array.isArray(postDto?.comments) ? postDto.comments : null;
                if (!comm) {
                    const resComments = await fetch(`${API_BASE}/api/Comment/${id}/comments`);
                    if (!resComments.ok) throw new Error(`HTTP ${resComments.status}`);
                    comm = await resComments.json();
                }

                if (!cancelled) {
                    setPost(postDto);
                    setComments(Array.isArray(comm) ? comm : []);
                    setStatus('ready');
                }
            } catch (err) {
                if (!cancelled) {
                    setError(err.message || 'Помилка завантаження');
                    setStatus('error');
                }
            }
        };

        load();
        return () => { cancelled = true; };
    }, [id]);

    const submitComment = async (e) => {
        e.preventDefault();
        if (!authed) return;

        setSendStatus('sending');
        try {
            const token = localStorage.getItem('token');
            const res = await fetch(`${API_BASE}/api/Comment/CreateComment`, {
                method: 'POST',
                headers: {
                    'Content-Type': 'application/json',
                    'Authorization': `Bearer ${token}`,
                },
                body: JSON.stringify({ text: commentText, postId: id }),
            });
            if (!res.ok) throw new Error(`HTTP ${res.status}`);

            setCommentText('');
            const resComments = await fetch(`${API_BASE}/api/Comment/${id}/comments`);
            if (resComments.ok) {
                const data = await resComments.json();
                setComments(Array.isArray(data) ? data : []);
            }
            setSendStatus('idle');
        } catch (err) {
            setSendStatus('error');
        }
    };

    if (status === 'loading') return <div style={{ maxWidth: 700, margin: '24px auto', padding: '0 12px' }}>Завантаження…</div>;
    if (status === 'error') return <div style={{ maxWidth: 700, margin: '24px auto', padding: '0 12px', color: 'crimson' }}>Помилка: {error}</div>;
    if (!post) return null;

    if (post.isBanned) {
        return (
            <div style={{ maxWidth: 700, margin: '24px auto', padding: '0 12px', display: 'grid', gap: 16 }}>
                <article style={{ padding: 16, border: '1px solid #ddd', borderRadius: 8, opacity: 0.85 }}>
                    <div style={{ fontSize: 14, color: '#555' }}>
                        Автор: {post.userName} • {new Date(post.createdAt).toLocaleString()} <span style={{ color: 'crimson' }}>(Заблоковано адміністрацією)</span>
                    </div>
                    <p style={{ marginTop: 8, color: '#666' }}>(Заблоковано адміністрацією)</p>
                </article>
            </div>
        );
    }

    return (
        <div style={{ maxWidth: 700, margin: '24px auto', padding: '0 12px', display: 'grid', gap: 16 }}>
            <article style={{ padding: 16, border: '1px solid #ddd', borderRadius: 8 }}>
                <div style={{ fontSize: 14, color: '#555' }}>
                    Автор: {post.userName} • {new Date(post.createdAt).toLocaleString()}
                </div>
                <p style={{ whiteSpace: 'pre-wrap', marginTop: 8 }}>{post.text}</p>
            </article>

            <section>
                <h3>Коментарі</h3>
                {comments.length === 0 ? (
                    <p>Коментарів ще немає.</p>
                ) : (
                    <div style={{ display: 'grid', gap: 12 }}>
                        {comments.map(c => (
                            <div key={c.id} style={{ padding: 12, border: '1px solid #eee', borderRadius: 6, opacity: c.isBanned ? 0.85 : 1 }}>
                                <div style={{ fontSize: 13, color: '#666' }}>
                                    {c.userName} • {new Date(c.createdAt).toLocaleString()}
                                    {c.isBanned && <span style={{ color: 'crimson', marginLeft: 8 }}>(Заблоковано адміністрацією)</span>}
                                </div>
                                <div style={{ marginTop: 6, whiteSpace: 'pre-wrap', color: c.isBanned ? '#666' : 'inherit' }}>
                                    {c.isBanned ? '(Заблоковано адміністрацією)' : c.text}
                                </div>
                            </div>
                        ))}
                    </div>
                )}
            </section>

            {authed && (
                <section>
                    <h3>Додати коментар</h3>
                    <form onSubmit={submitComment} style={{ display: 'grid', gap: 12 }}>
                        <textarea
                            placeholder="Ваш коментар…"
                            value={commentText}
                            onChange={(e) => setCommentText(e.target.value)}
                            rows={4}
                            required
                        />
                        <button disabled={sendStatus === 'sending'}>
                            {sendStatus === 'sending' ? 'Надсилаємо…' : 'Надіслати'}
                        </button>
                        {sendStatus === 'error' && <p style={{ color: 'crimson' }}>Не вдалося надіслати коментар.</p>}
                    </form>
                </section>
            )}
        </div>
    );
}