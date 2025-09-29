import { Link, useNavigate } from 'react-router-dom';
import { useEffect, useState } from 'react';

export default function NavBar() {
    const [authed, setAuthed] = useState(false);
    const navigate = useNavigate();

    useEffect(() => {
        const sync = () => setAuthed(Boolean(localStorage.getItem('token')));
        sync();
        window.addEventListener('storage', sync);
        return () => window.removeEventListener('storage', sync);
    }, []);

    const logout = () => {
        localStorage.removeItem('token');
        setAuthed(false);
        navigate('/');
    };

    const headerStyle = {
        position: 'fixed',
        top: 0,
        left: 0,
        right: 0,
        zIndex: 1000,
        background: '#fff',
        borderBottom: '1px solid #ddd'
    };

    const barStyle = {
        display: 'flex',
        justifyContent: 'space-between',
        alignItems: 'center',
        padding: '10px 16px'
    };

    return (
        <header style={headerStyle}>
            <nav style={barStyle}>
                <div style={{ display: 'flex', gap: 12 }}>
                    <Link to="/">Головна</Link>
                    {authed && <Link to="/post/new">Створити пост</Link>}
                </div>
                <div style={{ display: 'flex', gap: 12 }}>
                    {authed ? (
                        <button onClick={logout}>Вийти</button>
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