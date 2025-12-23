import { useEffect, useState } from 'react';
import { Link, useParams, useNavigate } from 'react-router-dom';
import './Profile.css';

const API_BASE = 'https://localhost:7142';
const guidRegex = /^[0-9a-fA-F]{8}-([0-9a-fA-F]{4}-){3}[0-9a-fA-F]{12}$/;

export default function Profile() {
  const { idOrName } = useParams(); 
  const navigate = useNavigate();

  const [profile, setProfile] = useState(null);
  const [posts, setPosts] = useState([]);
  const [avatarUrl, setAvatarUrl] = useState(null);
  const [status, setStatus] = useState('idle');
  const [error, setError] = useState(null);
  const [isOwner, setIsOwner] = useState(false);
  const token = localStorage.getItem('token');
 
  useEffect(() => {
    const load = async () => {
      setStatus('loading');
      setError(null);

      try {
        let currentUserId = null;
        if (token) {
          const meRes = await fetch(`${API_BASE}/api/User/profile`, {
            headers: { 'Authorization': `Bearer ${token}` }
          });
          if (meRes.ok) {
            const me = await meRes.json();
            currentUserId = me?.id;
          }
        }

        let profileData = null;
        if (!idOrName) {
          if (!token) throw new Error('Требуется авторизація');
          const res = await fetch(`${API_BASE}/api/User/profile`, {
            headers: { 'Authorization': `Bearer ${token}` }
          });
          if (!res.ok) throw new Error(`Не удалось загрузить профиль (${res.status})`);
          profileData = await res.json();
        } else {
          let res;
          if (guidRegex.test(idOrName)) {
            res = await fetch(`${API_BASE}/api/User/users/${idOrName}`, {
              headers: token ? { 'Authorization': `Bearer ${token}` } : undefined
            });
          } else {
            res = await fetch(`${API_BASE}/api/User/users/by-username?username=${encodeURIComponent(idOrName)}`, {
              headers: token ? { 'Authorization': `Bearer ${token}` } : undefined
            });
          }
          if (!res.ok) {
            if (res.status === 404) throw new Error('Користувача не знайдено');
            throw new Error(`Не удалось загрузить профиль (${res.status})`);
          }
          profileData = await res.json();
        }

        setProfile(profileData);
        setAvatarUrl(profileData?.profilePictureUrl ?? profileData?.avatarUrl ?? null);
        setIsOwner(Boolean(!idOrName || (profileData?.id && profileData.id === currentUserId)));

        if (profileData?.id) {
          const postsRes = await fetch(`${API_BASE}/api/Post/user/${profileData.id}`, {
            headers: token ? { 'Authorization': `Bearer ${token}` } : undefined
          });
          if (postsRes.ok) {
            const postsData = await postsRes.json();
            setPosts(Array.isArray(postsData) ? postsData : []);
          } else {
            setPosts([]);
          }
        } else {
          setPosts([]);
        }

        setStatus('idle');
      } catch (e) {
        setError(e.message || 'Помилка завантаження');
        setStatus('error');
      }
    };

    load();
  }, [idOrName, token]);

  const handleFileChange = async (e) => {
    if (!isOwner) return;
    const file = e.target.files && e.target.files[0];
    if (!file) return;
    if (!token) { alert('Требуется авторизація'); return; }

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

      const body = {
        id: profile?.id,
        name: profile?.name ?? profile?.userName ?? '',
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
      setProfile(prev => prev ? { ...prev, profilePictureUrl: fileUrl } : prev);
    } catch (err) {
      console.error(err);
      alert(err.message || 'Upload error');
    }
  };

  const sendFriendRequest = async () => {
    if (!token) { alert('Требуется авторизація'); return; }
    if (!profile?.id) return;
    try {
      const res = await fetch(`${API_BASE}/api/Friend/SendFriendRequest`, {
        method: 'POST',
        headers: {
          'Content-Type': 'application/json',
          'Authorization': `Bearer ${token}`
        },
        body: JSON.stringify(profile.id) 
      });
      if (!res.ok) throw new Error(`HTTP ${res.status}`);
      alert('Заявка відправлена');
    } catch (e) {
      console.error(e);
      alert(e.message || 'Помилка при відправленні заявки');
    }
  };

  const startPrivateChat = async () => {
    if (!token) { alert('Требуется авторизація'); return; }
    if (!profile?.id) return;
    try {
      const res = await fetch(`${API_BASE}/api/Chat/private/${profile.id}`, {
        method: 'POST',
        headers: { 'Authorization': `Bearer ${token}` }
      });
      if (!res.ok) throw new Error(`HTTP ${res.status}`);
      const data = await res.json();
      const chatId = data?.chatId;
      if (chatId) navigate(`/chat/${chatId}`);
    } catch (e) {
      console.error(e);
      alert(e.message || 'Не вдалося створити чат');
    }
  };

  if (status === 'loading') return <p>Завантаження…</p>;
  if (status === 'error') return <p>Помилка: {error}</p>;

  return (
    <div className="container">
      <h2 className="title">{isOwner ? 'Мій профіль' : 'Профіль користувача'}</h2>

      <div style={{ marginBottom: 12 }}>
        {avatarUrl ? (
          <img src={avatarUrl} alt="avatar" style={{ width: 120, height: 120, borderRadius: 8, objectFit: 'cover' }} />
        ) : (
          <div style={{ width: 120, height: 120, background: '#eee', borderRadius: 8 }} />
        )}

        {isOwner && (
          <div style={{ marginTop: 8 }}>
            <input type="file" accept="image/*" onChange={handleFileChange} />
          </div>
        )}
      </div>

      {profile && (
        <div style={{ marginBottom: 12 }}>
          <div><strong>{profile.name ?? profile.userName}</strong></div>
          <div>{profile.email}</div>
          {profile.isBanned && <div style={{ color: 'red' }}>(Користувач заблокований)</div>}
        </div>
      )}

      {!isOwner && profile && (
        <div style={{ display: 'flex', gap: 8, marginBottom: 16 }}>
          <button onClick={sendFriendRequest} style={{ padding: '8px 12px' }}>Відправити заявку в друзі</button>
          <button onClick={startPrivateChat} style={{ padding: '8px 12px' }}>Почати чат</button>
        </div>
      )}

      <h3>Пости</h3>
      {posts.length === 0 ? (
        <p>Пости відсутні.</p>
      ) : (
        <ul className="post-list">
          {posts.map(p => {
            const timeStr = new Date(p.createdAt).toLocaleString();

            return (
              <li key={p.id} className="post-card">
                <div className="post-card__header">
                  <Link to={`/user/${encodeURIComponent(p.userName)}`} className="post-card__meta">
                    {p.userName}
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
      )}
    </div>
  );
}