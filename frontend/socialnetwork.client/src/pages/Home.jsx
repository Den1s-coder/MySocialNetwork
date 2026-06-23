import { useEffect, useState } from 'react';
import { Link } from 'react-router-dom';
import './Home.css';

const API_BASE = 'https://localhost:7142';

export default function Home() {
    const [posts, setPosts] = useState([]);
    const [status, setStatus] = useState('idle'); // idle | loading | error
    const [error, setError] = useState(null);

    useEffect(() => {
        const load = async () => {
            setStatus('loading');
            try {
                const res = await fetch(`${API_BASE}/api/Post`);
                if (!res.ok) throw new Error(`HTTP ${res.status}`);
                const data = await res.json();
                setPosts(Array.isArray(data) ? data : []);
                setStatus('idle');
            } catch (e) {
                setError(e.message || 'Помилка завантаження');
                setStatus('error');
            }
        };
        load();
    }, []);

    if (status === 'loading') return <p>Завантаження…</p>;
    if (status === 'error') return <p>Помилка: {error}</p>;

    if (!posts.length) return <p>Пости відсутні.</p>;

    return (
        <div className="container">
            <h2 className="title">Головна</h2>
            <ul className="post-list">
                {posts.map(p => {
                    const timeStr = new Date(p.createdAt).toLocaleString();
                    const CardInner = (
                        <>
                            <div className="post-card__meta">{p.userName}</div>
                            <div className="post-card__text">
                                {p.isBanned ? '(Заблоковано адміністрацією)' : p.text}
                            </div>
                            <div className="post-card__time">{timeStr}</div>
                        </>
                    );

                        return (
                            <li key={p.id} className="post-card">
                                <div className="post-card__header" style={{ alignItems: 'center' }}>
                                    <Link to={`/User/by-username/${encodeURIComponent(p.userName)}`} className="post-card__meta" style={{ display: 'flex', alignItems: 'center', gap: 8 }}>
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