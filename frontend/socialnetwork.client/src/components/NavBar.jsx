import { Link, useNavigate } from 'react-router-dom';
import { useAuth } from '../hooks/useAuth';

export default function NavBar() {
    const navigate = useNavigate();
    const { isAuthenticated, logout } = useAuth();

    const handleLogout = () => {
        logout();
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
                    {isAuthenticated && <Link to="/post/new">Створити пост</Link>}
                    {isAuthenticated && <Link to="/chats">Чати</Link>}
                </div>
                <div style={{ display: 'flex', gap: 12 }}>
                    {isAuthenticated ? (
                        <>
                            <Link to="/profile">Профіль</Link>
                            <button onClick={handleLogout}>Вийти</button>
                        </>
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