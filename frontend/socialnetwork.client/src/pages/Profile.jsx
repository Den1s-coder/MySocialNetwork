import { useEffect, useState } from 'react';
import { Link } from 'react-router-dom';
import './Profile.css';

const API_BASE = 'https://localhost:7142';

export default function Profile() {
    const [posts, setPosts] = useState([]);
    const [status, setStatus] = useState('idle');
    const [error, setError] = useState(null);
    const [avatarUrl, setAvatarUrl] = useState(null);

    const token = localStorage.getItem('token');

    useEffect(() => {
        const load = async () => {
            setStatus('loading');
            try {
              
                let profile = null;
                if (token) {
                    const profileRes = await fetch(`${API_BASE}/api/User/profile`, {
                        headers: { 'Authorization': `Bearer ${token}` }
                    });
                    if (profileRes.ok) {
                        profile = await profileRes.json();
                        const url = profile?.profilePictureUrl ?? profile?.avatarUrl ?? null;
                        setAvatarUrl(url);
                    }
                }

         
                let postsData = [];
                if (profile?.id) {
                    const postsRes = await fetch(`${API_BASE}/api/Post/user/${profile.id}`, {
                        headers: { 'Authorization': `Bearer ${token}` }
                    });
                    if (!postsRes.ok) throw new Error(`Posts fetch failed ${postsRes.status}`);
                    postsData = await postsRes.json();
                } else {
                    
                    if (token) {
                        const postsRes = await fetch(`${API_BASE}/api/Post/me`, {
                            headers: { 'Authorization': `Bearer ${token}` }
                        });
                        if (postsRes.ok) postsData = await postsRes.json();
                    }
                }

                setPosts(Array.isArray(postsData) ? postsData : []);
                setStatus('idle');
            } catch (e) {
                setError(e.message || 'Помилка завантаження');
                setStatus('error');
            }
        };
        load();
    }, []);

    const handleFileChange = async (e) => {
        const file = e.target.files && e.target.files[0];
        if (!file) return;
        if (!token) { alert('Not authenticated'); return; }

        try {
            const fd = new FormData();
            fd.append('file', file);

            const uploadRes = await fetch(`${API_BASE}/api/File/upload`, {
                method: 'POST',
                headers: { 'Authorization': `Bearer ${token}` },
                body: fd
            });
            if (!uploadRes.ok) throw new Error('Upload failed');

            const uploadData = await uploadRes.json();
            const fileUrl = uploadData.fileUrl;

            const profileRes = await fetch(`${API_BASE}/api/User/profile`, {
                headers: { 'Authorization': `Bearer ${token}` }
            });
            if (!profileRes.ok) throw new Error('Failed to load profile');
            const profile = await profileRes.json();

            const body = {
                name: profile?.name || profile?.userName || '',
                email: profile?.email,
                profilePictureUrl: fileUrl
            };

            const updateRes = await fetch(`${API_BASE}/api/User/profile`, {
                method: 'PUT',
                headers: {
                    'Content-Type': 'application/json',
                    'Authorization': `Bearer ${token}`
                },
                body: JSON.stringify(body)
            });
            if (!updateRes.ok) throw new Error('Profile update failed');

            setAvatarUrl(fileUrl);
        } catch (err) {
            console.error(err);
            alert(err.message || 'Upload error');
        }
    };

    if (status === 'loading') return <p>Завантаження…</p>;
    if (status === 'error') return <p>Помилка: {error}</p>;

    return (
        <div className="container">
            <h2 className="title">Профіль користувача</h2>

            <div style={{ marginBottom: 12 }}>
                {avatarUrl ? (
                    <img src={avatarUrl} alt="avatar" style={{ width: 120, height: 120, borderRadius: 8, objectFit: 'cover' }} />
                ) : (
                    <div style={{ width: 120, height: 120, background: '#eee', borderRadius: 8 }} />
                )}
                <div style={{ marginTop: 8 }}>
                    <input type="file" accept="image/*" onChange={handleFileChange} />
                </div>
            </div>

            {posts.length === 0 ? (
                <p>Пости відсутні.</p>
            ) : (
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
                                {p.isBanned ? (
                                    <div className="post-card--banned">
                                        {CardInner}
                                    </div>
                                ) : (
                                    <Link to={`/post/${p.id}`} className="post-card__link">
                                        {CardInner}
                                    </Link>
                                )}
                            </li>
                        );
                    })}
                </ul>
            )}
        </div>
    );
}