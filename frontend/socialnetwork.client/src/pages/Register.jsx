import { useState } from 'react';
import { authFetch } from '../hooks/authFetch';

const API_BASE = 'https://localhost:7142';

export default function Register() {
    const [form, setForm] = useState({ userName: '', email: '', password: '' });
    const [status, setStatus] = useState('idle'); 
    const [error, setError] = useState(null);

    const onChange = e => setForm(prev => ({ ...prev, [e.target.name]: e.target.value }));

    const submit = async e => {
        e.preventDefault();
        setStatus('loading');
        setError(null);
        try {
            const res = await authFetch(`${API_BASE}/api/Auth/register`, {
                method: 'POST',
                headers: { 'Content-Type': 'application/json' },
                body: JSON.stringify({
                    userName: form.userName,
                    email: form.email,
                    password: form.password,
                }),
            });
            if (!res.ok) throw new Error(`HTTP ${res.status}`);
            setStatus('success');
        } catch (err) {
            setError(err.message || 'Помилка реєстрації');
            setStatus('error');
        }
    };

    return (
        <div style={{ maxWidth: 400, margin: '24px auto', padding: '0 12px' }}>
            <h2>Реєстрація</h2>
            <form onSubmit={submit} style={{ display: 'grid', gap: 12 }}>
                <input
                    name="userName"
                    placeholder="Ім'я користувача"
                    value={form.userName}
                    onChange={onChange}
                    required
                />
                <input
                    name="email"
                    type="email"
                    placeholder="Email"
                    value={form.email}
                    onChange={onChange}
                    required
                />
                <input
                    name="password"
                    type="password"
                    placeholder="Пароль"
                    value={form.password}
                    onChange={onChange}
                    required
                />
                <button disabled={status === 'loading'}>{status === 'loading' ? 'Реєстрація…' : 'Зареєструватися'}</button>
            </form>
            {status === 'success' && <p>Успішна реєстрація. Тепер можете увійти.</p>}
            {status === 'error' && <p style={{ color: 'crimson' }}>Помилка: {error}</p>}
        </div>
    );
}