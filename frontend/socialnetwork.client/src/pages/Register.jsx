import { useState, useEffect } from 'react';
import { authFetch } from '../hooks/authFetch';

const API_BASE = 'https://localhost:7142';
const GOOGLE_CLIENT_ID = import.meta.env.VITE_GOOGLE_CLIENT_ID || '';

export default function Register() {
    const [form, setForm] = useState({ userName: '', email: '', password: '' });
    const [status, setStatus] = useState('idle');
    const [error, setError] = useState(null);

    const onChange = e => setForm(prev => ({ ...prev, [e.target.name]: e.target.value }));

    useEffect(() => {
        let mounted = true;
        let pollHandle = null;

        const initGsi = () => {
            if (!mounted) return;
            if (!window.google || !GOOGLE_CLIENT_ID) return;
            try {
                window.google.accounts.id.initialize({
                    client_id: GOOGLE_CLIENT_ID,
                    callback: handleGoogleCredentialResponse
                });
                const container = document.getElementById('g_id_signin');
                if (container) {
                    window.google.accounts.id.renderButton(container, { theme: 'outline', size: 'large' });
                }
            } catch (err) {
                console.error('GSI init failed', err);
            }
        };

        if (window.google) initGsi();

        const onGsiLoaded = () => initGsi();
        window.addEventListener('gsi-loaded', onGsiLoaded);

        pollHandle = setInterval(() => {
            if (window.google) {
                initGsi();
                clearInterval(pollHandle);
                pollHandle = null;
            }
        }, 200);

        return () => {
            mounted = false;
            window.removeEventListener('gsi-loaded', onGsiLoaded);
            if (pollHandle) clearInterval(pollHandle);
        };
    }, []);

    const handleGoogleCredentialResponse = async (response) => {
        try {
            setStatus('loading');
            setError(null);
            const res = await fetch(`${API_BASE}/api/Auth/google`, {
                method: 'POST',
                headers: { 'Content-Type': 'application/json' },
                body: JSON.stringify({ idToken: response.credential }),
            });
            if (!res.ok) throw new Error(`HTTP ${res.status}`);
            const data = await res.json();
            localStorage.setItem('accessToken', data.accessToken);
            localStorage.setItem('refreshToken', data.refreshToken);
            window.dispatchEvent(new Event('tokens-updated'));
            setStatus('success');
        } catch (err) {
            setError(err.message || 'Google login error');
            setStatus('error');
        }
    };

    const handleGoogleSignInClick = () => {
        if (window.google && window.google.accounts && window.google.accounts.id) {
            try {
                window.google.accounts.id.prompt();
            } catch (e) {
                console.error('GSI prompt failed', e);
                setError('Помилка ініціалізації Google Sign-In');
            }
        } else {
            setError('Google SDK ще не завантажений — перезавантажте сторінку');
        }
    };

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

            <div style={{ marginTop: 12 }}>
                <div id="g_id_signin"></div>

                <button type="button" onClick={handleGoogleSignInClick} style={{ marginTop: 8 }}>
                    Зареєструватися через Google
                </button>
            </div>

            {status === 'success' && <p>Успішна реєстрація. Тепер можете увійти.</p>}
            {status === 'error' && <p style={{ color: 'crimson' }}>Помилка: {error}</p>}
        </div>
    );
}