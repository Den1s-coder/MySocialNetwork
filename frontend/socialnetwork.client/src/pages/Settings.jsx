import React, { useState } from 'react';
import { authFetch } from '../hooks/authFetch';

export default function Settings() {
    const [showPasswordForm, setShowPasswordForm] = useState(false);
    const [showEmailForm, setShowEmailForm] = useState(false);

    const [currentPassword, setCurrentPassword] = useState('');
    const [newPassword, setNewPassword] = useState('');
    const [passwordConfirm, setPasswordConfirm] = useState('');

    const [newEmail, setNewEmail] = useState('');
    const [Password, setPassword] = useState('');

    const [message, setMessage] = useState(null);
    const [error, setError] = useState(null);

    async function handleChangePassword(e) {
        e.preventDefault();
        setMessage(null);
        setError(null);

        if (newPassword !== passwordConfirm) {
            setError('Паролі не співпадають');
            return;
        }

        const dto = { currentPassword, newPassword };
        const res = await authFetch('/api/User/change-password', {
            method: 'POST',
            body: JSON.stringify(dto)
        });

        if (res.ok) {
            setMessage('Пароль успішно змінено');
            setCurrentPassword('');
            setNewPassword('');
            setPasswordConfirm('');
            setShowPasswordForm(false);
        } else {
            const text = await res.text();
            setError(text || 'Помилка при зміні пароля');
        }
    }

    async function handleChangeEmail(e) {
        e.preventDefault();
        setMessage(null);
        setError(null);

        if (!newEmail) {
            setError('Вкажіть новий email');
            return;
        }
        if (!Password) {
            setError('Підтвердіть зміни поточним паролем');
            return;
        }

        const dto = { newEmail, Password };
        const res = await authFetch('/api/User/change-email', {
            method: 'POST',
            body: JSON.stringify(dto)
        });

        if (res.ok) {
            setMessage('Email успішно змінено');
            setNewEmail('');
            setPassword('');
            setShowEmailForm(false);
        } else {
            const text = await res.text();
            setError(text || 'Помилка при зміні email');
        }
    }

    return (
        <div style={{ maxWidth: 600, margin: '0 auto' }}>
            <h2>Налаштування профілю</h2>

            <div style={{ marginBottom: 16 }}>
                <button type="button" onClick={() => { setShowPasswordForm(v => !v); setError(null); setMessage(null); }}>
                    {showPasswordForm ? 'Сховати зміну пароля' : 'Змінити пароль'}
                </button>
            </div>

            {showPasswordForm && (
                <form onSubmit={handleChangePassword} style={{ marginBottom: 20 }}>
                    <h3>Змінити пароль</h3>
                    <div>
                        <label>Поточний пароль</label>
                        <input type="password" value={currentPassword} onChange={e => setCurrentPassword(e.target.value)} required />
                    </div>
                    <div>
                        <label>Новий пароль</label>
                        <input type="password" value={newPassword} onChange={e => setNewPassword(e.target.value)} required />
                    </div>
                    <div>
                        <label>Підтвердження пароля</label>
                        <input type="password" value={passwordConfirm} onChange={e => setPasswordConfirm(e.target.value)} required />
                    </div>
                    <div style={{ marginTop: 8 }}>
                        <button type="submit">Змінити пароль</button>
                        <button type="button" onClick={() => { setShowPasswordForm(false); setCurrentPassword(''); setNewPassword(''); setPasswordConfirm(''); }} style={{ marginLeft: 8 }}>Відміна</button>
                    </div>
                </form>
            )}

            <hr />

            <div style={{ margin: '16px 0' }}>
                <button type="button" onClick={() => { setShowEmailForm(v => !v); setError(null); setMessage(null); }}>
                    {showEmailForm ? 'Сховати зміну email' : 'Змінити email'}
                </button>
            </div>

            {showEmailForm && (
                <form onSubmit={handleChangeEmail} style={{ marginBottom: 20 }}>
                    <h3>Змінити email</h3>
                    <div>
                        <label>Новий email</label>
                        <input type="email" value={newEmail} onChange={e => setNewEmail(e.target.value)} required />
                    </div>
                    <div>
                        <label>Поточний пароль (підтвердження)</label>
                        <input type="password" value={Password} onChange={e => setPassword(e.target.value)} required />
                    </div>
                    <div style={{ marginTop: 8 }}>
                        <button type="submit">Змінити email</button>
                        <button type="button" onClick={() => { setShowEmailForm(false); setNewEmail(''); setPassword(''); }} style={{ marginLeft: 8 }}>Відміна</button>
                    </div>
                </form>
            )}

            {message && <div style={{ color: 'green', marginTop: 8 }}>{message}</div>}
            {error && <div style={{ color: 'red', marginTop: 8 }}>{error}</div>}
        </div>
    );
}