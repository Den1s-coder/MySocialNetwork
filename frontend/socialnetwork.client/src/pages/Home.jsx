import { useEffect, useState } from 'react';
import { Link } from 'react-router-dom';
import './Home.css';
import { authFetch } from '../hooks/authFetch';
import Avatar from '../components/Avatar';
import ReactionBar from '../components/ReactionBar';
import EmojiPickerButton from '../components/EmojiPickerButton';
import { useAuth } from '../hooks/useAuth';

const API_BASE = import.meta.env.VITE_API_BASE || '';
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
    const [newPostImage, setNewPostImage] = useState(null);
    const [imagePreview, setImagePreview] = useState(null);
    const [createStatus, setCreateStatus] = useState('idle');
    const [createError, setCreateError] = useState(null);

    const loadPosts = async (pageNumber = 1) => {
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

    const handlePostReactionChanged = (postId, updatedReactions, newCode) => {
        setPosts(prev => prev.map(p =>
            p.id === postId ? { ...p, reactions: updatedReactions, currentUserReactionCode: newCode } : p
        ));
    };

    const handleImageChange = (e) => {
        const file = e.target.files?.[0];
        if (file) {
            if (!file.type.startsWith('image/')) {
                setCreateError('Виберіть файл зображення');
                return;
            }
            
            if (file.size > 5 * 1024 * 1024) {
                setCreateError('Розмір файлу не повинен перевищувати 5MB');
                return;
            }

            setNewPostImage(file);
            setCreateError(null);

            const reader = new FileReader();
            reader.onload = (event) => {
                setImagePreview(event.target.result);
            };
            reader.readAsDataURL(file);
        }
    };

    const uploadImage = async () => {
        if (!newPostImage) return null;

        const formData = new FormData();
        formData.append('file', newPostImage);

        try {
            const token = localStorage.getItem('accessToken');
            const res = await fetch(`${API_BASE}/api/File/upload`, {
                method: 'POST',
                headers: {
                    'Authorization': `Bearer ${token}`
                },
                body: formData
            });

            if (!res.ok) {
                throw new Error(`HTTP ${res.status}`);
            }

            const data = await res.json();
            return data.fileUrl;
        } catch (err) {
            setCreateError('Помилка завантаження зображення');
            throw err;
        }
    };

    const removeImage = () => {
        setNewPostImage(null);
        setImagePreview(null);
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
            let imageUrl = null;
            if (newPostImage) {
                imageUrl = await uploadImage();
            }

            const res = await authFetch(`${API_BASE}/api/Post`, {
                method: 'POST',
                body: JSON.stringify({ text, imageUrl })
            });

            if (!res.ok) {
                const msg = `HTTP ${res.status}`;
                throw new Error(msg);
            }

            setCreateStatus('success');
            setNewPostText('');
            setNewPostImage(null);
            setImagePreview(null);
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
                        <div style={{ position: 'relative' }}>
                            <textarea
                                placeholder="Що у вас на думці?"
                                value={newPostText}
                                onChange={(e) => setNewPostText(e.target.value)}
                                rows={4}
                                maxLength={5000}
                                style={{ resize: 'vertical', width: '100%' }}
                                required
                            />
                            <div style={{ position: 'absolute', bottom: 8, left: 8 }}>
                                <EmojiPickerButton 
                                    onEmojiSelect={(emoji) => setNewPostText(prev => prev + emoji)}
                                />
                            </div>
                        </div>
                        
                        <div>
                            <label style={{ display: 'block', marginBottom: 8 }}>
                                <input
                                    type="file"
                                    accept="image/*"
                                    onChange={handleImageChange}
                                    disabled={createStatus === 'loading'}
                                    style={{ display: 'block' }}
                                />
                            </label>
                            
                            {imagePreview && (
                                <div style={{ position: 'relative', marginBottom: 12 }}>
                                    <img 
                                        src={imagePreview} 
                                        alt="Попередження" 
                                        style={{ maxWidth: '100%', maxHeight: 300, borderRadius: 4 }}
                                    />
                                    <button
                                        type="button"
                                        onClick={removeImage}
                                        disabled={createStatus === 'loading'}
                                        style={{
                                            position: 'absolute',
                                            top: 4,
                                            right: 4,
                                            padding: '4px 8px',
                                            background: 'rgba(0,0,0,0.5)',
                                            color: 'white',
                                            border: 'none',
                                            borderRadius: 4,
                                            cursor: 'pointer'
                                        }}
                                    >
                                        ✕
                                    </button>
                                </div>
                            )}
                        </div>

                        <div style={{ display: 'flex', justifyContent: 'space-between', alignItems: 'center' }}>
                            <div style={{ fontSize: 12, color: '#666' }}>{newPostText.length}/5000</div>
                            <div style={{ display: 'flex', gap: 8 }}>
                                <button type="button" onClick={() => { setNewPostText(''); removeImage(); setCreateError(null); }} disabled={createStatus === 'loading'}>
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
                                            {p.imageUrl && (
                                                <img 
                                                    src={p.imageUrl} 
                                                    alt="Пост" 
                                                    style={{
                                                        maxWidth: '100%',
                                                        maxHeight: 400,
                                                        borderRadius: 4,
                                                        marginTop: 8,
                                                        objectFit: 'cover'
                                                    }}
                                                />
                                            )}
                                            <div className="post-card__time">{timeStr}</div>
                                        </Link>

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