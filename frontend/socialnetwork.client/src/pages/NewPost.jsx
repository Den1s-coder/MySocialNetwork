import { useState } from 'react';
import { useNavigate } from 'react-router-dom';
import { authFetch } from '../hooks/authFetch';

const API_BASE = 'https://localhost:7142';

export default function NewPost() {
    const [text, setText] = useState('');
    const [status, setStatus] = useState('idle'); // idle | loading | error | success
    const [error, setError] = useState(null);
    const navigate = useNavigate();

    const submit = async (e) => {
        e.preventDefault();
        setStatus('loading');
        setError(null);
        try {
            const token = localStorage.getItem('accessToken');
            if (!token) throw new Error('Необхідна авторизація');

            const res = await authFetch(`${API_BASE}/api/Post`, {
                method: 'POST',
                headers: {
                    'Content-Type': 'application/json',
                    'Authorization': `Bearer ${token}`
                },
                body: JSON.stringify({ text })
            });

            if (!res.ok) {
                const msg = `HTTP ${res.status}`;
                throw new Error(msg);
            }

            setStatus('success');
            navigate('/profile');
        } catch (err) {
            setError(err.message || 'Помилка створення поста');
            setStatus('error');
        }
    };

    return (
        <div style={{ maxWidth: 600, margin: '24px auto', padding: '0 12px' }}>
            <h2>Створити пост</h2>
            <form onSubmit={submit} style={{ display: 'grid', gap: 12 }}>
                <textarea
                    placeholder="Текст вашого поста..."
                    value={text}
                    onChange={(e) => setText(e.target.value)}
                    rows={6}
                    required
                />
                <button disabled={status === 'loading'}>
                    {status === 'loading' ? 'Публікація…' : 'Опублікувати'}
                </button>
            </form>
            {status === 'error' && <p style={{ color: 'crimson' }}>Помилка: {error}</p>}
        </div>
    );
}