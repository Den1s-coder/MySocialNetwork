import { useEffect, useState } from 'react';
import { Link } from 'react-router-dom';

const API_BASE = 'https://localhost:7142';

export default function Profile() {
    const [posts, setPosts] = useState([]);
    const [status, setStatus] = useState('idle'); 
    const [error, setError] = useState(null);

    useEffect(() => {
        const load = async () => {
            setStatus('loading');
            try {
                const token = localStorage.getItem("token");
                const res = await fetch(`${API_BASE}/api/Post/profile`, {
                    headers: {
                        "Content-Type": "application/json",
                        "Authorization": `Bearer ${token}`
                    }
                });
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
        <div style={{ maxWidth: 640, margin: '24px auto', padding: '0 12px' }}>
            <h2>Профіль користувача</h2>
            <ul style={{ listStyle: 'none', padding: 0, margin: 0 }}>
                {posts.map(p => {
                    const CardInner = (
                        <>
                            <div style={{ fontSize: 14, color: '#666' }}>{p.userName}</div>
                            <div style={{ fontSize: 16, marginTop: 6, whiteSpace: 'pre-wrap' }}>
                                {p.isBanned ? '(Заблоковано адміністрацією)' : p.text}
                            </div>
                            <div style={{ fontSize: 12, marginTop: 5 }}>{new Date(p.createdAt).toLocaleString()}</div>
                        </>
                    );

                    return (
                        <li key={p.id} style={{ border: '1px solid #ddd', borderRadius: 8, padding: 0, marginBottom: 12 }}>
                            {p.isBanned ? (
                                <div style={{ display: 'block', padding: 12, color: '#666', textDecoration: 'none', cursor: 'default', opacity: 0.8 }}>
                                    {CardInner}
                                </div>
                            ) : (
                                <Link to={`/post/${p.id}`} style={{ display: 'block', padding: 12, color: 'inherit', textDecoration: 'none' }}>
                                    {CardInner}
                                </Link>
                            )}
                        </li>
                    );
                })}
            </ul>
        </div>
    );
}