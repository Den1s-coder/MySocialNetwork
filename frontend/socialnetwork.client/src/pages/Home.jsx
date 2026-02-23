import { useEffect, useState } from 'react';
import { Link } from 'react-router-dom';
import './Home.css';
import { authFetch } from '../hooks/authFetch';
import Avatar from '../components/Avatar';
import ReactionBar from '../components/ReactionBar';
import { useAuth } from '../hooks/useAuth';

const API_BASE = 'https://localhost:7142';
const PAGE_SIZE = 10;

export default function Home() {
    const { isAuthenticated } = useAuth();

    const [posts, setPosts] = useState([]);
    const [status, setStatus] = useState('idle');
    const [error, setError] = useState(null);
    const [page, setPage] = useState(1);
    const [hasMore, setHasMore] = useState(false);
    const [loadingPage, setLoadingPage] = useState(false);

    const [newPostText, setNewPostText] = useState('');
    const [createStatus, setCreateStatus] = useState('idle');
    const [createError, setCreateError] = useState(null);

    const loadPosts = async (pageNumber = 1, signalToken = {}) => {
        setLoadingPage(true);
        setError(null);
        try {
            const res = await authFetch(`${API_BASE}/api/Post/posts?pageNumber=${pageNumber}&pageSize=${PAGE_SIZE}`);
            if (!res.ok) throw new Error(`HTTP ${res.status}`);
            const data = await res.json();

            let items = Array.isArray(data) ? data : data.items ?? data.data ?? [];
            let total = Array.isArray(data) ? null : (data.totalCount ?? data.total ?? null);

            setPosts(prev => pageNumber === 1 ? items : [...prev, ...items]);

            if (total !== null) {
                setHasMore((pageNumber * PAGE_SIZE) < total);
            } else {
                setHasMore(items.length === PAGE_SIZE);
            }

            setStatus('idle');
        } catch (e) {
            setError(e.message || 'Помилка завантаження');
            setStatus('error');
        } finally {
            setLoadingPage(false);
        }
    };

    useEffect(() => {
        let cancelled = false;
        const run = async () => {
            if (cancelled) return;
            await loadPosts(page);
        };
        run();
        return () => { cancelled = true; };
    }, [page]);

    const loadMore = () => {
        if (loadingPage) return;
        setPage(p => p + 1);
    };

    const pickAvatarUrl = (p) => {
        return p.authorProfilePictureUrl || p.profilePictureUrl || p.authorAvatarUrl || p.userProfilePictureUrl || null;
    };

    /** Оптимістичне оновлення реакцій поста у списку */
    const handlePostReactionChanged = (postId, updatedReactions, newCode) => {
        setPosts(prev => prev.map(p =>
            p.id === postId ? { ...p, reactions: updatedReactions, currentUserReactionCode: newCode } : p
        ));
    };

    const submitPost = async (e) => {
        e?.preventDefault();
        if (!isAuthenticated) {
            setCreateError('Потрібна авторизація');
            return;
        }

        const text = (newPostText ?? '').trim();
        if (!text) {
            setCreateError('Текст не може бути порожнім');
            return;
        }
        if (text.length > 5000) {
            setCreateError('Текст занадто довгий');
            return;
        }

        setCreateStatus('loading');
        setCreateError(null);

        try {
            const res = await authFetch(`${API_BASE}/api/Post`, {
                method: 'POST',
                body: JSON.stringify({ text })
            });

            if (!res.ok) {
                const msg = `HTTP ${res.status}`;
                throw new Error(msg);
            }

            setCreateStatus('success');
            setNewPostText('');
            setPage(1);
            await loadPosts(1);
        } catch (err) {
            setCreateError(err.message || 'Помилка створення поста');
            setCreateStatus('error');
        } finally {
            setTimeout(() => setCreateStatus('idle'), 600);
        }
    };

    if (status === 'loading' && page === 1) return <p>Завантаження…</p>;
    if (status === 'error') return <p>Помилка: {error}</p>;

    return (
        <div className="container">
            <h2 className="title">Головна</h2>

            {isAuthenticated ? (
                <section className="create-post" style={{ marginBottom: 16, padding: 12, border: '1px solid #e6e6e6', borderRadius: 8 }}>
                    <form onSubmit={submitPost} style={{ display: 'grid', gap: 8 }}>
                        <textarea
                            placeholder="Що у вас на думці?"
                            value={newPostText}
                            onChange={(e) => setNewPostText(e.target.value)}
                            rows={4}
                            maxLength={5000}
                            style={{ resize: 'vertical' }}
                            required
                        />
                        <div style={{ display: 'flex', justifyContent: 'space-between', alignItems: 'center' }}>
                            <div style={{ fontSize: 12, color: '#666' }}>{newPostText.length}/5000</div>
                            <div style={{ display: 'flex', gap: 8 }}>
                                <button type="button" onClick={() => { setNewPostText(''); setCreateError(null); }} disabled={createStatus === 'loading'}>
                                    Очистити
                                </button>
                                <button type="submit" disabled={createStatus === 'loading'}>
                                    {createStatus === 'loading' ? 'Публікація…' : 'Опублікувати'}
                                </button>
                            </div>
                        </div>
                        {createStatus === 'error' && <div style={{ color: 'crimson' }}>{createError}</div>}
                    </form>
                </section>
            ) : (
                <div style={{ marginBottom: 16, padding: 12, border: '1px solid #f0f0f0', borderRadius: 8 }}>
                    <p>Щоб створювати пости, будь ласка, <Link to="/login">увійдіть</Link> або <Link to="/register">зареєструйтесь</Link>.</p>
                </div>
            )}

            {(!posts || posts.length === 0) ? (
                <p>Пости відсутні.</p>
            ) : (
                <ul className="post-list">
                    {posts.map(p => {
                        const timeStr = new Date(p.createdAt).toLocaleString();
                        const avatarUrl = pickAvatarUrl(p);

                        return (
                            <li key={p.id} className="post-card">
                                <div className="post-card__header" style={{ alignItems: 'center' }}>
                                    <Link to={`/user/${encodeURIComponent(p.userName)}`} className="post-card__meta" style={{ display: 'flex', alignItems: 'center', gap: 8 }}>
                                        <Avatar url={avatarUrl} name={p.userName} />
                                        <span>{p.userName}</span>
                                    </Link>
                                </div>

                                {p.isBanned ? (
                                    <div className="post-card--banned">
                                        <div className="post-card__text">{'(Заблоковано адміністрацією)'}</div>
                                        <div className="post-card__time">{timeStr}</div>
                                    </div>
                                ) : (
                                    <>
                                        <Link to={`/post/${p.id}`} className="post-card__link">
                                            <div className="post-card__text">{p.text}</div>
                                            <div className="post-card__time">{timeStr}</div>
                                        </Link>

                                        {/* Реакції поста */}
                                        <ReactionBar
                                            reactions={p.reactions ?? []}
                                            currentUserReactionCode={p.currentUserReactionCode ?? null}
                                            entityId={p.id}
                                            entityType="Post"
                                            authed={isAuthenticated}
                                            onReactionChanged={(updatedReactions, newCode) =>
                                                handlePostReactionChanged(p.id, updatedReactions, newCode)
                                            }
                                        />
                                    </>
                                )}
                            </li>
                        );
                    })}
                </ul>
            )}
            {hasMore && (
                <div style={{ textAlign: 'center', marginTop: 12 }}>
                    <button onClick={loadMore} disabled={loadingPage}>
                        {loadingPage ? 'Завантаження…' : 'Завантажити ще'}
                    </button>
                </div>
            )}
        </div>
    );
}