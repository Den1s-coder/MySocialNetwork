import { useEffect, useState } from 'react';
import { Link } from 'react-router-dom';
import './Home.css';
import { authFetch } from '../hooks/authFetch';
import Avatar from '../components/Avatar';

const API_BASE = 'https://localhost:7142';
const PAGE_SIZE = 10;

export default function Home() {
    const [posts, setPosts] = useState([]);
    const [status, setStatus] = useState('idle');
    const [error, setError] = useState(null);
    const [page, setPage] = useState(1);
    const [hasMore, setHasMore] = useState(false);
    const [loadingPage, setLoadingPage] = useState(false);

    useEffect(() => {
        const load = async () => {
            setLoadingPage(true);
            setError(null);
            try {
                const res = await authFetch(`${API_BASE}/api/Post/posts?pageNumber=${page}&pageSize=${PAGE_SIZE}`);
                if (!res.ok) throw new Error(`HTTP ${res.status}`);
                const data = await res.json();

                let items = Array.isArray(data) ? data : data.items ?? data.data ?? [];
                let total = Array.isArray(data) ? null : (data.totalCount ?? data.total ?? null);

                setPosts(prev => page === 1 ? items : [...prev, ...items]);

                if (total !== null) {
                    setHasMore((page * PAGE_SIZE) < total);
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
        load();
    }, [page]);

    const loadMore = () => {
        if (loadingPage) return;
        setPage(p => p + 1);
    };

    if (status === 'loading' && page === 1) return <p>Завантаження…</p>;
    if (status === 'error') return <p>Помилка: {error}</p>;
    if (!posts.length) return <p>Пости відсутні.</p>;

    const pickAvatarUrl = (p) => {
        return p.authorProfilePictureUrl || p.profilePictureUrl || p.authorAvatarUrl || p.userProfilePictureUrl || null;
    };

    return (
        <div className="container">
            <h2 className="title">Головна</h2>
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
                                <Link to={`/post/${p.id}`} className="post-card__link">
                                    <div className="post-card__text">{p.text}</div>
                                    <div className="post-card__time">{timeStr}</div>
                                </Link>
                            )}
                        </li>
                    );
                })}
            </ul>

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