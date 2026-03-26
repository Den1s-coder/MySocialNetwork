import { useEffect, useState } from 'react';
import { useNavigate } from 'react-router-dom';
import { authFetch } from '../hooks/authFetch';
import { useUserRole } from '../hooks/useUserRole';
import './AdminPanel.css';

const API_BASE = 'https://localhost:7142';

export default function AdminPanel() {
    const navigate = useNavigate();
    const { isAdmin, loading: roleLoading } = useUserRole();

    const [users, setUsers] = useState([]);
    const [loading, setLoading] = useState(true);
    const [error, setError] = useState(null);
    const [searchTerm, setSearchTerm] = useState('');
    const [filterRole, setFilterRole] = useState('');
    const [actionInProgress, setActionInProgress] = useState(false);

    if (roleLoading) return <div style={{ padding: '20px' }}>Завантаження…</div>;
    if (!isAdmin) return <div style={{ padding: '20px', color: 'red' }}>Доступ заборонений</div>;

    const loadUsers = async () => {
        setLoading(true);
        setError(null);
        try {
            const res = await authFetch(`${API_BASE}/api/User/users`);
            if (!res.ok) throw new Error(`HTTP ${res.status}`);
            const data = await res.json();
            setUsers(Array.isArray(data) ? data : []);
        } catch (err) {
            setError(err.message || 'Помилка завантаження користувачів');
        } finally {
            setLoading(false);
        }
    };

    useEffect(() => {
        loadUsers();
    }, []);

    const handleChangeRole = async (userId, newRole) => {
        if (!window.confirm(`Змінити роль користувача на ${newRole}?`)) return;

        setActionInProgress(true);
        try {
            const res = await authFetch(`${API_BASE}/api/User/users/${userId}/role`, {
                method: 'POST',
                headers: { 'Content-Type': 'application/json' },
                body: JSON.stringify({ newRole })
            });
            if (!res.ok) throw new Error(`HTTP ${res.status}`);

            setUsers(prev => prev.map(u => 
                u.id === userId ? { ...u, role: newRole } : u
            ));
            alert('Роль змінена');
        } catch (err) {
            alert(`Помилка: ${err.message}`);
        } finally {
            setActionInProgress(false);
        }
    };

    const handleBanUser = async (userId, userName) => {
        if (!window.confirm(`Блокувати користувача ${userName}?`)) return;

        setActionInProgress(true);
        try {
            const res = await authFetch(`${API_BASE}/api/User/users/${userId}/ban`, {
                method: 'POST'
            });
            if (!res.ok) throw new Error(`HTTP ${res.status}`);

            setUsers(prev => prev.map(u => 
                u.id === userId ? { ...u, isBanned: true } : u
            ));
            alert('Користувач заблокований');
        } catch (err) {
            alert(`Помилка: ${err.message}`);
        } finally {
            setActionInProgress(false);
        }
    };

    const filteredUsers = users.filter(u => {
        const matchesSearch = u.name?.toLowerCase().includes(searchTerm.toLowerCase()) ||
                            u.email?.toLowerCase().includes(searchTerm.toLowerCase());
        const matchesRole = !filterRole || u.role === filterRole;
        return matchesSearch && matchesRole;
    });

    if (loading) return <div style={{ padding: '20px' }}>Завантаження користувачів…</div>;

    return (
        <div className="container">
            <h2>Адмін панель</h2>

            {error && <div style={{ color: 'red', marginBottom: 16 }}>Помилка: {error}</div>}

            <div className="admin-filters" style={{ marginBottom: 16, display: 'grid', gap: 12, gridTemplateColumns: '1fr 1fr' }}>
                <input
                    type="text"
                    placeholder="Пошук за ім'ям або email…"
                    value={searchTerm}
                    onChange={(e) => setSearchTerm(e.target.value)}
                    style={{ padding: 8, border: '1px solid #ddd', borderRadius: 4 }}
                />
                <select
                    value={filterRole}
                    onChange={(e) => setFilterRole(e.target.value)}
                    style={{ padding: 8, border: '1px solid #ddd', borderRadius: 4 }}
                >
                    <option value="">Усі ролі</option>
                    <option value="User">Користувач</option>
                    <option value="Moderator">Модератор</option>
                    <option value="Admin">Адміністратор</option>
                </select>
            </div>

            <div style={{ marginBottom: 12, color: '#666' }}>
                Всього користувачів: <strong>{filteredUsers.length}</strong>
            </div>

            <div className="users-table" style={{ overflowX: 'auto', marginBottom: 16 }}>
                <table style={{ width: '100%', borderCollapse: 'collapse' }}>
                    <thead>
                        <tr style={{ backgroundColor: '#f5f5f5', borderBottom: '2px solid #ddd' }}>
                            <th style={{ padding: 12, textAlign: 'left' }}>Ім'я</th>
                            <th style={{ padding: 12, textAlign: 'left' }}>Email</th>
                            <th style={{ padding: 12, textAlign: 'left' }}>Роль</th>
                            <th style={{ padding: 12, textAlign: 'left' }}>Статус</th>
                            <th style={{ padding: 12, textAlign: 'center' }}>Дії</th>
                        </tr>
                    </thead>
                    <tbody>
                        {filteredUsers.map(user => (
                            <tr key={user.id} style={{ borderBottom: '1px solid #eee' }}>
                                <td style={{ padding: 12 }}>{user.name || user.userName}</td>
                                <td style={{ padding: 12 }}>{user.email}</td>
                                <td style={{ padding: 12 }}>
                                    <select
                                        value={user.role}
                                        onChange={(e) => handleChangeRole(user.id, e.target.value)}
                                        disabled={actionInProgress}
                                        style={{ padding: 4, borderRadius: 4 }}
                                    >
                                        <option value="User">Користувач</option>
                                        <option value="Moderator">Модератор</option>
                                        <option value="Admin">Адміністратор</option>
                                    </select>
                                </td>
                                <td style={{ padding: 12 }}>
                                    {user.isBanned ? (
                                        <span style={{ color: 'red', fontWeight: 'bold' }}>Заблокований</span>
                                    ) : (
                                        <span style={{ color: 'green' }}>Активний</span>
                                    )}
                                </td>
                                <td style={{ padding: 12, textAlign: 'center' }}>
                                    {!user.isBanned && (
                                        <button
                                            onClick={() => handleBanUser(user.id, user.name || user.userName)}
                                            disabled={actionInProgress}
                                            style={{
                                                padding: '6px 12px',
                                                backgroundColor: '#ff4444',
                                                color: 'white',
                                                border: 'none',
                                                borderRadius: 4,
                                                cursor: 'pointer',
                                                opacity: actionInProgress ? 0.6 : 1
                                            }}
                                        >
                                            Блокувати
                                        </button>
                                    )}
                                </td>
                            </tr>
                        ))}
                    </tbody>
                </table>
            </div>

            {filteredUsers.length === 0 && (
                <div style={{ textAlign: 'center', padding: 20, color: '#666' }}>
                    Користувачів не знайдено
                </div>
            )}
        </div>
    );
}