import { Link, useNavigate } from 'react-router-dom';
import { useState, useEffect, useRef } from 'react';
import { useAuth } from '../hooks/useAuth';
import { authFetch } from '../hooks/authFetch';
import './NavBar.css';

export default function NavBar() {
    const navigate = useNavigate();
    const { isAuthenticated, logout, accessToken, currentUserName } = useAuth();

    const [avatarUrl, setAvatarUrl] = useState(null);
    const [menuOpen, setMenuOpen] = useState(false);
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

    const handleLogout = () => {
        setMenuOpen(false);
        logout();
        navigate('/');
    };

    return (
        <header className="nav-header">
            <nav className="nav-bar">
                <div className="nav-left">
                    <Link to="/">Головна</Link>
                    {isAuthenticated && <Link to="/post/new">Створити пост</Link>}
                    {isAuthenticated && <Link to="/chats">Чати</Link>}
                </div>

                <div className="nav-right">
                    {isAuthenticated ? (
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
                    ) : (
                        <>
                            <Link to="/login">Увійти</Link>
                            <Link to="/register">Зареєструватися</Link>
                        </>
                    )}
                </div>
            </nav>
        </header>
    );
}