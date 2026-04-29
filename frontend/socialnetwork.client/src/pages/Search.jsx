import { useState, useEffect } from 'react';
import { useSearchParams, useNavigate } from 'react-router-dom';
import { authFetch } from '../hooks/authFetch';
import Avatar from '../components/Avatar';
import { Link } from 'react-router-dom';
import './Search.css';

const API_BASE = 'https://localhost:7142';
const PAGE_SIZE = 10;

export default function Search() {
    const [searchParams, setSearchParams] = useSearchParams();
    const navigate = useNavigate();
    const query = searchParams.get('q') || '';
    const searchType = searchParams.get('type') || 'all'; 

    const [posts, setPosts] = useState([]);
    const [users, setUsers] = useState([]);
    const [loading, setLoading] = useState(false);
    const [error, setError] = useState(null);
    const [postPage, setPostPage] = useState(1);
    const [userPage, setUserPage] = useState(1);
    const [hasMorePosts, setHasMorePosts] = useState(false);
    const [hasMoreUsers, setHasMoreUsers] = useState(false);

    useEffect(() => {
        if (!query.trim()) {
            setPosts([]);
            setUsers([]);
            return;
        }

        const fetchResults = async () => {
            setLoading(true);
            setError(null);
            setPosts([]);
            setUsers([]);
            setPostPage(1);
            setUserPage(1);

            try {
                const tasks = [];

                if (searchType === 'all' || searchType === 'posts') {
                    tasks.push(
                        authFetch(
                            `${API_BASE}/api/Post/search?query=${encodeURIComponent(query)}&pageNumber=1&pageSize=${PAGE_SIZE}`
                        )
                            .then(res => {
                                if (!res.ok) throw new Error(`HTTP ${res.status}`);
                                return res.json();
                            })
                            .then(data => {
                                const items = Array.isArray(data) ? data : data.items ?? data.data ?? [];
                                const total = Array.isArray(data) ? null : (data.totalCount ?? data.total ?? null);
                                setPosts(items);
                                if (total !== null) {
                                    setHasMorePosts((1 * PAGE_SIZE) < total);
                                } else {
                                    setHasMorePosts(items.length === PAGE_SIZE);
                                }
                            })
                    );
                }

                if (searchType === 'all' || searchType === 'users') {
                    tasks.push(
                        authFetch(
                            `${API_BASE}/api/User/search?query=${encodeURIComponent(query)}&pageNumber=1&pageSize=${PAGE_SIZE}`
                        )
                            .then(res => {
                                if (!res.ok) throw new Error(`HTTP ${res.status}`);
                                return res.json();
                            })
                            .then(data => {
                                const items = Array.isArray(data) ? data : data.items ?? data.data ?? [];
                                const total = Array.isArray(data) ? null : (data.totalCount ?? data.total ?? null);
                                setUsers(items);
                                if (total !== null) {
                                    setHasMoreUsers((1 * PAGE_SIZE) < total);
                                } else {
                                    setHasMoreUsers(items.length === PAGE_SIZE);
                                }
                            })
                    );
                }

                await Promise.all(tasks);
            } catch (e) {
                setError(e.message || 'Помилка пошуку');
            } finally {
                setLoading(false);
            }
        };

        fetchResults();
    }, [query, searchType]);

    const loadMorePosts = async () => {
        const nextPage = postPage + 1;
        try {
            const res = await authFetch(
                `${API_BASE}/api/Post/search?query=${encodeURIComponent(query)}&pageNumber=${nextPage}&pageSize=${PAGE_SIZE}`
            );
            if (!res.ok) throw new Error(`HTTP ${res.status}`);
            const data = await res.json();
            const items = Array.isArray(data) ? data : data.items ?? data.data ?? [];
            setPosts(prev => [...prev, ...items]);
            setHasMorePosts(items.length === PAGE_SIZE);
            setPostPage(nextPage);
        } catch (e) {
            setError(e.message || 'Помилка завантаження постів');
        }
    };

    const loadMoreUsers = async () => {
        const nextPage = userPage + 1;
        try {
            const res = await authFetch(
                `${API_BASE}/api/User/search?query=${encodeURIComponent(query)}&pageNumber=${nextPage}&pageSize=${PAGE_SIZE}`
            );
            if (!res.ok) throw new Error(`HTTP ${res.status}`);
            const data = await res.json();
            const items = Array.isArray(data) ? data : data.items ?? data.data ?? [];
            setUsers(prev => [...prev, ...items]);
            setHasMoreUsers(items.length === PAGE_SIZE);
            setUserPage(nextPage);
        } catch (e) {
            setError(e.message || 'Помилка завантаження користувачів');
        }
    };

    const handleSearchChange = (e) => {
        const newQuery = e.target.value;
        setSearchParams({ q: newQuery, type: searchType });
    };

    const handleTypeChange = (newType) => {
        setSearchParams({ q: query, type: newType });
    };

    const pickAvatarUrl = (p) => {
        return p.authorProfilePictureUrl || p.profilePictureUrl || p.authorAvatarUrl || p.userProfilePictureUrl || null;
    };

    return (
        <div className="search-container">
            <div className="search-header">
                <h1>Пошук</h1>
                <div className="search-input-wrapper">
                    <input
                        type="text"
                        className="search-input"
                        placeholder="Пошук постів та користувачів…"
                        value={query}
                        onChange={handleSearchChange}
                        autoFocus
                    />
                </div>

                {query.trim() && (
                    <div className="search-filters">
                        <button
                            className={`filter-btn ${searchType === 'all' ? 'active' : ''}`}
                            onClick={() => handleTypeChange('all')}
                        >
                            Усе
                        </button>
                        <button
                            className={`filter-btn ${searchType === 'posts' ? 'active' : ''}`}
                            onClick={() => handleTypeChange('posts')}
                        >
                            Пости ({posts.length})
                        </button>
                        <button
                            className={`filter-btn ${searchType === 'users' ? 'active' : ''}`}
                            onClick={() => handleTypeChange('users')}
                        >
                            Користувачі ({users.length})
                        </button>
                    </div>
                )}
            </div>

            {error && <div className="search-error">Помилка: {error}</div>}

            {loading && <div className="search-loading">Завантаження…</div>}

            {!query.trim() ? (
                <div className="search-empty">
                    <p>Введіть пошуковий запит</p>
                </div>
            ) : posts.length === 0 && users.length === 0 && !loading ? (
                <div className="search-empty">
                    <p>Результатів не знайдено</p>
                </div>
            ) : (
                <>
                    {(searchType === 'all' || searchType === 'posts') && (
                        <div className="search-section">
                            <h2>Пості</h2>
                            {posts.length === 0 ? (
                                <p className="search-no-results">Постів не знайдено</p>
                            ) : (
                                <>
                                    <ul className="search-posts-list">
                                        {posts.map(post => {
                                            const timeStr = new Date(post.createdAt).toLocaleString('uk-UA');
                                            const avatarUrl = pickAvatarUrl(post);

                                            return (
                                                <li key={post.id} className="search-post-card">
                                                    <Link to={`/user/${encodeURIComponent(post.userName)}`} className="search-post-header">
                                                        <Avatar url={avatarUrl} name={post.userName} size={40} />
                                                        <div className="search-post-meta">
                                                            <strong>{post.userName}</strong>
                                                            <small>{timeStr}</small>
                                                        </div>
                                                    </Link>
                                                    <Link to={`/post/${post.id}`} className="search-post-text">
                                                        {post.text}
                                                    </Link>
                                                </li>
                                            );
                                        })}
                                    </ul>
                                    {hasMorePosts && (
                                        <button className="load-more-btn" onClick={loadMorePosts}>
                                            Завантажити ще пості
                                        </button>
                                    )}
                                </>
                            )}
                        </div>
                    )}

                    {(searchType === 'all' || searchType === 'users') && (
                        <div className="search-section">
                            <h2>Користувачі</h2>
                            {users.length === 0 ? (
                                <p className="search-no-results">Користувачів не знайдено</p>
                            ) : (
                                <>
                                    <ul className="search-users-list">
                                        {users.map(user => (
                                            <li key={user.id} className="search-user-card">
                                                <Link to={`/user/${encodeURIComponent(user.userName)}`} className="search-user-link">
                                                    <Avatar url={user.profilePictureUrl} name={user.userName} size={48} />
                                                    <div className="search-user-info">
                                                        <div className="search-user-name">
                                                            {user.name || user.userName}
                                                        </div>
                                                        <div className="search-user-username">
                                                            @{user.userName}
                                                        </div>
                                                        <div className="search-user-email">
                                                            {user.email}
                                                        </div>
                                                    </div>
                                                </Link>
                                            </li>
                                        ))}
                                    </ul>
                                    {hasMoreUsers && (
                                        <button className="load-more-btn" onClick={loadMoreUsers}>
                                            Завантажити ще користувачів
                                        </button>
                                    )}
                                </>
                            )}
                        </div>
                    )}
                </>
            )}
        </div>
    );
}