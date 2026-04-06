import { useEffect, useState } from 'react';
import { Link, useParams, useNavigate } from 'react-router-dom';
import './Profile.css';
import { authFetch } from '../hooks/authFetch';

const API_BASE = 'https://localhost:7142';
const guidRegex = /^[0-9a-fA-F]{8}-([0-9a-fA-F]{4}-){3}[0-9a-fA-F]{12}$/;

export default function Profile() {
  const { idOrName } = useParams(); 
  const navigate = useNavigate();

  const [profile, setProfile] = useState(null);
  const [posts, setPosts] = useState([]);
  const [friends, setFriends] = useState([]);
  const [avatarUrl, setAvatarUrl] = useState(null);
  const [status, setStatus] = useState('idle');
  const [error, setError] = useState(null);
  const [isOwner, setIsOwner] = useState(false);
  const [isEditingProfile, setIsEditingProfile] = useState(false);
  const [editData, setEditData] = useState({ name: '', bio: '' });
  const [showFriends, setShowFriends] = useState(false);
  const accessToken = localStorage.getItem('accessToken');
 
  useEffect(() => {
    const load = async () => {
      setStatus('loading');
      setError(null);

      try {
        let currentUserId = null;
        if (accessToken) {
          const meRes = await authFetch(`${API_BASE}/api/User/profile`, {
            headers: { 'Authorization': `Bearer ${accessToken}` }
          });
          if (meRes.ok) {
            const me = await meRes.json();
            currentUserId = me?.id;
          }
        }

        let profileData = null;
        if (!idOrName) {
          if (!accessToken) throw new Error('Требуется авторизація');
          const res = await authFetch(`${API_BASE}/api/User/profile`, {
            headers: { 'Authorization': `Bearer ${accessToken}` }
          });
          if (!res.ok) throw new Error(`Не удалось загрузить профиль (${res.status})`);
          profileData = await res.json();
        } else {
          let res;
          if (guidRegex.test(idOrName)) {
            res = await authFetch(`${API_BASE}/api/User/users/${idOrName}`, {
              headers: accessToken ? { 'Authorization': `Bearer ${accessToken}` } : undefined
            });
          } else {
            res = await authFetch(`${API_BASE}/api/User/users/by-username?username=${encodeURIComponent(idOrName)}`, {
              headers: accessToken ? { 'Authorization': `Bearer ${accessToken}` } : undefined
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
        setEditData({ name: profileData?.name ?? profileData?.userName ?? '', bio: profileData?.bio ?? '' });
        setIsOwner(Boolean(!idOrName || (profileData?.id && profileData.id === currentUserId)));

        if (profileData?.id) {
          const postsRes = await authFetch(`${API_BASE}/api/Post/user/${profileData.id}`, {
            headers: accessToken ? { 'Authorization': `Bearer ${accessToken}` } : undefined
          });
          if (postsRes.ok) {
            const postsData = await postsRes.json();
            setPosts(Array.isArray(postsData) ? postsData : []);
          } else {
            setPosts([]);
          }

          try {
            const friendsRes = await authFetch(`${API_BASE}/api/Friend/user/${profileData.id}`, {
              headers: accessToken ? { 'Authorization': `Bearer ${accessToken}` } : undefined
            });
            if (friendsRes.ok) {
              const friendsData = await friendsRes.json();
              setFriends(Array.isArray(friendsData) ? friendsData : []);
            }
          } catch (e) {
            console.error('Failed to load friends:', e);
          }
        } else {
          setPosts([]);
          setFriends([]);
        }

        setStatus('idle');
      } catch (e) {
        setError(e.message || 'Помилка завантаження');
        setStatus('error');
      }
    };

    load();
  }, [idOrName, accessToken]);

  const handleFileChange = async (e) => {
    if (!isOwner) return;
    const file = e.target.files && e.target.files[0];
    if (!file) return;
    if (!accessToken) { alert('Требуется авторизація'); return; }

    try {
      const fd = new FormData();
      fd.append('file', file);

      const uploadRes = await authFetch(`${API_BASE}/api/File/upload`, {
        method: 'POST',
        headers: { 'Authorization': `Bearer ${accessToken}` },
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

      const updateRes = await authFetch(`${API_BASE}/api/User/profile`, {
        method: 'PUT',
        headers: {
          'Content-Type': 'application/json',
          'Authorization': `Bearer ${accessToken}`
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

  const handleAvatarClick = () => {
    if (isOwner) {
      document.querySelector('.avatar-file-input')?.click();
    }
  };

  const handleSaveProfile = async () => {
    if (!isOwner || !accessToken) return;

    try {
      const body = {
        id: profile?.id,
        name: editData.name || profile?.userName,
        email: profile?.email,
        profilePictureUrl: avatarUrl,
        bio: editData.bio
      };

      const updateRes = await authFetch(`${API_BASE}/api/User/profile`, {
        method: 'PUT',
        headers: {
          'Content-Type': 'application/json',
          'Authorization': `Bearer ${accessToken}`
        },
        body: JSON.stringify(body)
      });

      if (!updateRes.ok) throw new Error('Profile update failed');
      
      setProfile(prev => prev ? { ...prev, ...body } : prev);
      setIsEditingProfile(false);
      alert('Профіль оновлено');
    } catch (err) {
      console.error(err);
      alert(err.message || 'Update error');
    }
  };

  const sendFriendRequest = async () => {
    if (!accessToken) { alert('Требуется авторізація'); return; }
    if (!profile?.id) return;
    try {
      const res = await authFetch(`${API_BASE}/api/Friend/SendFriendRequest`, {
        method: 'POST',
        headers: {
          'Content-Type': 'application/json',
          'Authorization': `Bearer ${accessToken}`
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
    if (!accessToken) { alert('Требуется авторизація'); return; }
    if (!profile?.id) return;
    try {
      const res = await authFetch(`${API_BASE}/api/Chat/private/${profile.id}`, {
        method: 'POST',
        headers: { 'Authorization': `Bearer ${accessToken}` }
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

  if (status === 'loading') return <div className="loading">Завантаження…</div>;
  if (status === 'error') return <div className="error">Помилка: {error}</div>;

  return (
    <div className="container">
      <h2 className="title">{isOwner ? 'Мій профіль' : 'Профіль користувача'}</h2>

      <div className="profile-header">
        <div className={`profile-avatar ${isOwner ? 'profile-avatar--editable' : ''}`} onClick={handleAvatarClick}>
          {avatarUrl ? (
            <img src={avatarUrl} alt="avatar" className="avatar-image" />
          ) : (
            <div className="avatar-placeholder">👤</div>
          )}

          {isOwner && ( 
            <>
              <div className="avatar-overlay">
                <div className="avatar-plus-icon">+</div>
              </div>
              <input 
                type="file" 
                accept="image/*" 
                onChange={handleFileChange}
                className="avatar-file-input"
                style={{ display: 'none' }}
              />
            </>
          )}
        </div>

        {profile && (
          <div className="profile-info">
            {isEditingProfile ? (
              <div className="edit-form">
                <input 
                  type="text" 
                  value={editData.name} 
                  onChange={(e) => setEditData({...editData, name: e.target.value})}
                  className="edit-input"
                  placeholder="Ім'я"
                />
                <textarea 
                  value={editData.bio} 
                  onChange={(e) => setEditData({...editData, bio: e.target.value})}
                  className="edit-textarea"
                  placeholder="Про вас"
                  rows="3"
                />
                <div className="edit-actions">
                  <button onClick={handleSaveProfile} className="edit-save">Зберегти</button>
                  <button onClick={() => setIsEditingProfile(false)} className="edit-cancel">Скасувати</button>
                </div>
              </div>
            ) : (
              <>
                <div className="profile-name-section">
                  <div className="profile-name">{profile.name ?? profile.userName}</div>
                  {isOwner && (
                    <button 
                      onClick={() => setIsEditingProfile(true)} 
                      className="profile-edit-btn"
                      title="Редагувати профіль"
                    >
                      ✏️
                    </button>
                  )}
                </div>
                <div className="profile-email">{profile.email}</div>
                {profile.bio && <div className="profile-bio">{profile.bio}</div>}
                {profile.isBanned && <div className="profile-banned">🚫 Заблокований</div>}
              </>
            )}
          </div>
        )}
      </div>

      {profile && (
        <div className="profile-stats">
          <div className="stat-item">
            <div className="stat-number">{posts.length}</div>
            <div className="stat-label">Пости</div>
          </div>
          <div className="stat-item">
            <div className="stat-number">{friends.length}</div>
            <div className="stat-label">Друзі</div>
          </div>
          <div className="stat-item">
            <div className="stat-number">0</div>
            <div className="stat-label">Переглядів</div>
          </div>
        </div>
      )}

      {!isOwner && profile && (
        <div className="profile-actions">
          <button onClick={sendFriendRequest} className="profile-button profile-button--primary">➕ Додати в друзі</button>
          <button onClick={startPrivateChat} className="profile-button">💬 Написати</button>
        </div>
      )}

      {friends.length > 0 && (
        <div className="friends-section">
          <h3 onClick={() => setShowFriends(!showFriends)} className="friends-title">
            {showFriends ? '▼' : '▶'} Друзі ({friends.length})
          </h3>
          {showFriends && (
            <div className="friends-grid">
              {friends.slice(0, 6).map(friend => (
                <Link 
                  key={friend.id} 
                  to={`/user/${friend.id}`}
                  className="friend-card"
                >
                  <img src={friend.profilePictureUrl || '👤'} alt={friend.name} className="friend-avatar" />
                  <div className="friend-name">{friend.name}</div>
                </Link>
              ))}
            </div>
          )}
        </div>
      )}

      <h3>Пости</h3>
      {posts.length === 0 ? (
        <div className="posts-empty">Пости відсутні.</div>
      ) : (
        <ul className="post-list">
          {posts.map(p => {
            const timeStr = new Date(p.createdAt).toLocaleString();

            return (
              <li key={p.id} className="post-card">
                <div className="post-card__header">
                  <Link to={`/user/${p.userId}`} className="post-card__meta">
                    {p.userName}
                  </Link>
                </div>

                {p.isBanned ? (
                  <div className="post-card--banned">
                    <div className="post-card__text">Заблоковано адміністрацією</div>
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