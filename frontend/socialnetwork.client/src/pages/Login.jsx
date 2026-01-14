import { useState } from 'react';

const API_BASE = 'https://localhost:7142';

export default function Login() {
    const [form, setForm] = useState({ username: '', password: '' });
    const [status, setStatus] = useState('idle'); // idle | loading | error | success
    const [error, setError] = useState(null);

    const onChange = e => setForm(prev => ({ ...prev, [e.target.name]: e.target.value }));

    const submit = async e => {
        e.preventDefault();
        setStatus('loading');
        setError(null);
        try {
            const res = await fetch(`${API_BASE}/api/Auth/login`, {
                method: 'POST',
                headers: { 'Content-Type': 'application/json' },
                body: JSON.stringify({ username: form.username, password: form.password }),
            });
            if (!res.ok) throw new Error(`HTTP ${res.status}`);
            const data = await res.json();
            const access = data.accessToken;
            const refresh = data.refreshToken;

            localStorage.setItem('accessToken', access);
            localStorage.setItem('refreshToken', refresh);

            window.dispatchEvent(new Event('tokens-updated'));

            setStatus('success');
        } catch (err) {
            setError(err.message || 'Помилка входу');
            setStatus('error');
        }
    };

    return (
        <div style={{ maxWidth: 400, margin: '24px auto', padding: '0 12px' }}>
            <h2>Вхід</h2>
            <form onSubmit={submit} style={{ display: 'grid', gap: 12 }}>
                <input
                    name="username"
                    placeholder="Логін"
                    value={form.username}
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
                <button disabled={status === 'loading'}>{status === 'loading' ? 'Вхід…' : 'Увійти'}</button>
            </form>
            {status === 'success' && <p>Успішний вхід. Токен збережено.</p>}
            {status === 'error' && <p style={{ color: 'crimson' }}>Помилка: {error}</p>}
        </div>
    );
}