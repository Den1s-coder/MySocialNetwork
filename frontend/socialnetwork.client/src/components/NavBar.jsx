import { Link, useNavigate, useSearchParams } from 'react-router-dom';
import { useState, useEffect, useRef } from 'react';
import { useAuth } from '../hooks/useAuth';
import { authFetch } from '../hooks/authFetch';
import NotificationBell from './NotificationBell';
import ThemeToggle from './ThemeToggle';
import { useUserRole } from '../hooks/useUserRole';
import './NavBar.css';

const API_BASE = 'https://localhost:7142';
const SUGGESTIONS_LIMIT = 3;

export default function NavBar() {
    const navigate = useNavigate();
    const [searchParams, setSearchParams] = useSearchParams();
    const { isAuthenticated, logout, accessToken, currentUserName } = useAuth();
    const { isAdmin } = useUserRole();

    const [avatarUrl, setAvatarUrl] = useState(null);
    const [menuOpen, setMenuOpen] = useState(false);
    const [searchQuery, setSearchQuery] = useState('');
    const [suggestions, setSuggestions] = useState({ posts: [], users: [] });
    const [showSuggestions, setShowSuggestions] = useState(false);
    const [suggestionsLoading, setSuggestionsLoading] = useState(false);
    const searchRef = useRef(null);
    const avatarRef = useRef(null);

    useEffect(() => {
        let mounted = true;
        const loadProfile = async () => {
            if (!isAuthenticated || !accessToken) {
                setAvatarUrl(null);
                return;
            }
            try {
                const res = await authFetch('/api/User/profile');
                if (!mounted) return;
                if (res.ok) {
                    const data = await res.json();
                    setAvatarUrl(data?.profilePictureUrl ?? data?.avatarUrl ?? null);
                } else {
                    setAvatarUrl(null);
                }
            } catch (e) {
                console.error('NavBar: failed to load profile', e);
                setAvatarUrl(null);
            }
        };

        loadProfile();
        return () => { mounted = false; };
    }, [isAuthenticated, accessToken]);

    useEffect(() => {
        const handleDocClick = (e) => {
            if (!avatarRef.current) return;
            if (!avatarRef.current.contains(e.target)) {
                setMenuOpen(false);
            }
        };
        document.addEventListener('click', handleDocClick);
        return () => document.removeEventListener('click', handleDocClick);
    }, []);

    useEffect(() => {
        const handleClickOutside = (e) => {
            if (!searchRef.current) return;
            if (!searchRef.current.contains(e.target)) {
                setShowSuggestions(false);
            }
        };
        document.addEventListener('click', handleClickOutside);
        return () => document.removeEventListener('click', handleClickOutside);
    }, []);

    useEffect(() => {
        if (!searchQuery.trim()) {
            setSuggestions({ posts: [], users: [] });
            setShowSuggestions(false);
            return;
        }

        const fetchSuggestions = async () => {
            setSuggestionsLoading(true);
            try {
                const [postsRes, usersRes] = await Promise.all([
                    authFetch(
                        `${API_BASE}/api/Post/search?query=${encodeURIComponent(searchQuery)}&pageNumber=1&pageSize=${SUGGESTIONS_LIMIT}`
                    ),
                    authFetch(
                        `${API_BASE}/api/User/search?query=${encodeURIComponent(searchQuery)}&pageNumber=1&pageSize=${SUGGESTIONS_LIMIT}`
                    )
                ]);

                let posts = [];
                let users = [];

                if (postsRes.ok) {
                    const data = await postsRes.json();
                    posts = Array.isArray(data) ? data : data.items ?? data.data ?? [];
                }

                if (usersRes.ok) {
                    const data = await usersRes.json();
                    users = Array.isArray(data) ? data : data.items ?? data.data ?? [];
                }

                setSuggestions({
                    posts: posts.slice(0, SUGGESTIONS_LIMIT),
                    users: users.slice(0, SUGGESTIONS_LIMIT)
                });
                setShowSuggestions(true);
            } catch (e) {
                console.error('Error fetching suggestions:', e);
                setSuggestions({ posts: [], users: [] });
            } finally {
                setSuggestionsLoading(false);
            }
        };

        const debounceTimer = setTimeout(fetchSuggestions, 300);
        return () => clearTimeout(debounceTimer);
    }, [searchQuery]);

    const handleLogout = () => {
        setMenuOpen(false);
        logout();
        navigate('/');
    };

    const handleSearch = (e) => {
        e.preventDefault();
        if (searchQuery.trim()) {
            setShowSuggestions(false);
            setSearchQuery('');
            navigate(`/search?q=${encodeURIComponent(searchQuery)}&type=all`);
        }
    };

    const handleSuggestionClick = (type, value) => {
        setShowSuggestions(false);
        setSearchQuery('');
        if (type === 'post') {
            navigate(`/post/${value}`);
        } else if (type === 'user') {
            navigate(`/user/${encodeURIComponent(value)}`);
        }
    };

    const pickAvatarUrl = (p) => {
        return p.authorProfilePictureUrl || p.profilePictureUrl || p.authorAvatarUrl || p.userProfilePictureUrl || null;
    };

    return (
        <>
            <aside className="nav-sidebar" aria-label="Главная навигация">
                <nav className="sidebar-nav">
                    <Link to="/" className="sidebar-item">Головна</Link>
                    <Link to="/chats" className="sidebar-item">Чати</Link>
                    <Link to="/friends" className="sidebar-item">Друзі</Link>
                    <Link to="/settings" className="sidebar-item">Налаштування</Link>
                </nav>
            </aside>

            <header className="top-header">
                <div className="top-header__inner">
                    <div className="brand">MySocialNetwork</div>

                    <div className="search-wrapper" ref={searchRef}>
                        <form className="search-form" onSubmit={handleSearch}>
                            <input
                                type="text"
                                className="search-input-navbar"
                                placeholder="Пошук постів та користувачів…"
                                value={searchQuery}
                                onChange={(e) => setSearchQuery(e.target.value)}
                                onFocus={() => searchQuery.trim() && setShowSuggestions(true)}
                                aria-label="Пошук"
                            />
                            <button type="submit" className="search-btn-navbar" aria-label="Здійснити пошук">
                                🔍
                            </button>
                        </form>

                        {showSuggestions && (
                            <div className="search-suggestions-dropdown">
                                {suggestionsLoading && (
                                    <div className="suggestions-loading">Пошук…</div>
                                )}

                                {!suggestionsLoading && suggestions.posts.length === 0 && suggestions.users.length === 0 && (
                                    <div className="suggestions-empty">Результатів не знайдено</div>
                                )}

                                {!suggestionsLoading && suggestions.users.length > 0 && (
                                    <div className="suggestions-section">
                                        <div className="suggestions-header">Користувачі</div>
                                        <ul className="suggestions-list">
                                            {suggestions.users.map(user => (
                                                <li key={user.id} className="suggestion-item user-item">
                                                    <button
                                                        type="button"
                                                        className="suggestion-button"
                                                        onClick={() => handleSuggestionClick('user', user.userName)}
                                                    >
                                                        <img
                                                            src={user.profilePictureUrl || 'data:image/svg+xml,%3Csvg xmlns="http://www.w3.org/2000/svg" viewBox="0 0 24 24"%3E%3Ccircle cx="12" cy="12" r="10" fill="%23ddd"/%3E%3C/svg%3E'}
                                                            alt={user.userName}
                                                            className="suggestion-avatar"
                                                        />
                                                        <div className="suggestion-content">
                                                            <div className="suggestion-name">
                                                                {user.name || user.userName}
                                                            </div>
                                                            <div className="suggestion-meta">
                                                                @{user.userName}
                                                            </div>
                                                        </div>
                                                    </button>
                                                </li>
                                            ))}
                                        </ul>
                                    </div>
                                )}

                                {!suggestionsLoading && suggestions.posts.length > 0 && (
                                    <div className="suggestions-section">
                                        <div className="suggestions-header">Пості</div>
                                        <ul className="suggestions-list">
                                            {suggestions.posts.map(post => (
                                                <li key={post.id} className="suggestion-item post-item">
                                                    <button
                                                        type="button"
                                                        className="suggestion-button"
                                                        onClick={() => handleSuggestionClick('post', post.id)}
                                                    >
                                                        <img
                                                            src={pickAvatarUrl(post) || 'data:image/svg+xml,%3Csvg xmlns="http://www.w3.org/2000/svg" viewBox="0 0 24 24"%3E%3Ccircle cx="12" cy="12" r="10" fill="%23ddd"/%3E%3C/svg%3E'}
                                                            alt={post.userName}
                                                            className="suggestion-avatar"
                                                        />
                                                        <div className="suggestion-content">
                                                            <div className="suggestion-name">
                                                                {post.userName}
                                                            </div>
                                                            <div className="suggestion-meta">
                                                                {post.text.substring(0, 50)}…
                                                            </div>
                                                        </div>
                                                    </button>
                                                </li>
                                            ))}
                                        </ul>
                                    </div>
                                )}

                                {!suggestionsLoading && (suggestions.posts.length > 0 || suggestions.users.length > 0) && (
                                    <div className="suggestions-footer">
                                        <button
                                            type="button"
                                            className="see-all-button"
                                            onClick={() => {
                                                setShowSuggestions(false);
                                                navigate(`/search?q=${encodeURIComponent(searchQuery)}&type=all`);
                                            }}
                                        >
                                            Переглянути все результати
                                        </button>
                                    </div>
                                )}
                            </div>
                        )}
                    </div>

                    <div className="nav-right">
                        {isAuthenticated ? (
                            <>
                                <div className="notification-area" style={{ marginRight: 12 }}>
                                    <NotificationBell apiBase="https://localhost:7142" />
                                </div>

                                <ThemeToggle />

                                <div className="avatar-wrapper" ref={avatarRef}>
                                    <button
                                        className="clickable-area"
                                        aria-haspopup="true"
                                        aria-expanded={menuOpen}
                                        onClick={() => setMenuOpen(s => !s)}
                                        title="Керування профілем"
                                        type="button"
                                    >
                                        <span className="avatar-circle">
                                            {avatarUrl ? (
                                                <img src={avatarUrl} alt="avatar" className="avatar-image" />
                                            ) : (
                                                <div className="initials">
                                                    {(currentUserName || '').slice(0, 2).toUpperCase() || 'U'}
                                                </div>
                                            )}
                                        </span>

                                        <span className="username">{currentUserName ?? 'Користувач'}</span>
                                    </button>

                                    <div className={`menu ${menuOpen ? 'open' : ''}`}>
                                        <div className="menu-header">
                                            <div className="menu-hello">Вітаю,</div>
                                            <div className="menu-username">{currentUserName ?? 'Користувач'}</div>
                                        </div>
                                        <button
                                            onClick={() => { setMenuOpen(false); navigate('/profile'); }}
                                            className="menu-button"
                                            type="button"
                                        >
                                            Профіль
                                        </button>
                                        <button
                                            onClick={handleLogout}
                                            className="menu-button menu-button--danger"
                                            type="button"
                                        >
                                            Вийти
                                        </button>
                                    </div>
                                </div>
                            </>
                        ) : (
                            <>
                                <ThemeToggle />
                                <Link to="/login">Увійти</Link>
                                <Link to="/register">Зареєструватися</Link>
                            </>
                        )}
                    </div>
                </div>
            </header>

            {isAdmin && (
                <Link to="/admin" className="nav-link">
                    Адмін панель
                </Link>
            )}
        </>
    );
}