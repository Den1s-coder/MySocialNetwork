import { useState } from 'react';
import { useNavigate } from 'react-router-dom';
import { authFetch } from '../hooks/authFetch';

const API_BASE = import.meta.env.VITE_API_BASE || '';

export default function NewPost() {
    const [text, setText] = useState('');
    const [image, setImage] = useState(null);
    const [imagePreview, setImagePreview] = useState(null);
    const [status, setStatus] = useState('idle'); // idle | loading | error | success
    const [error, setError] = useState(null);
    const navigate = useNavigate();

    const handleImageChange = (e) => {
        const file = e.target.files?.[0];
        if (file) {
            if (!file.type.startsWith('image/')) {
                setError('Виберіть файл зображення');
                return;
            }
            
            if (file.size > 10 * 1024 * 1024) {
                setError('Розмір файлу не повинен перевищувати 10MB');
                return;
            }

            setImage(file);
            setError(null);

            const reader = new FileReader();
            reader.onload = (event) => {
                setImagePreview(event.target.result);
            };
            reader.readAsDataURL(file);
        }
    };

    const uploadImage = async () => {
        if (!image) return null;

        setStatus('loading');
        const formData = new FormData();
        formData.append('file', image);

        try {
            const res = await authFetch(`${API_BASE}/api/File/upload`, {
                method: 'POST',
                body: formData
            });

            if (!res.ok) {
                throw new Error(`Помилка завантаження: HTTP ${res.status}`);
            }

            const data = await res.json();
            return data.fileUrl;
        } catch (err) {
            setError('Помилка завантаження зображення: ' + (err.message || 'Невідома помилка'));
            throw err;
        }
    };

    const submit = async (e) => {
        e.preventDefault();
        setStatus('loading');
        setError(null);
        try {
            let imageUrl = null;
            if (image) {
                imageUrl = await uploadImage();
            }

            const res = await authFetch(`${API_BASE}/api/Post`, {
                method: 'POST',
                body: JSON.stringify({ text, imageUrl })
            });

            if (!res.ok) {
                const msg = `HTTP ${res.status}`;
                throw new Error(msg);
            }

            setStatus('success');
            setText('');
            setImage(null);
            setImagePreview(null);
            navigate('/');
        } catch (err) {
            setError(err.message || 'Помилка створення поста');
            setStatus('error');
        }
    };

    const removeImage = () => {
        setImage(null);
        setImagePreview(null);
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
                
                <div>
                    <label style={{ display: 'block', marginBottom: 8 }}>
                        <input
                            type="file"
                            accept="image/*"
                            onChange={handleImageChange}
                            disabled={status === 'loading'}
                            style={{ display: 'block' }}
                        />
                    </label>
                    
                    {imagePreview && (
                        <div style={{ position: 'relative', marginBottom: 12 }}>
                            <img 
                                src={imagePreview} 
                                alt="Попередження" 
                                style={{ maxWidth: '100%', maxHeight: 300, borderRadius: 4 }}
                            />
                            <button
                                type="button"
                                onClick={removeImage}
                                disabled={status === 'loading'}
                                style={{
                                    position: 'absolute',
                                    top: 4,
                                    right: 4,
                                    padding: '4px 8px',
                                    background: 'rgba(0,0,0,0.5)',
                                    color: 'white',
                                    border: 'none',
                                    borderRadius: 4,
                                    cursor: 'pointer'
                                }}
                            >
                                ✕
                            </button>
                        </div>
                    )}
                </div>

                <button disabled={status === 'loading'}>
                    {status === 'loading' ? 'Публікація…' : 'Опублікувати'}
                </button>
            </form>
            {status === 'error' && <p style={{ color: 'crimson' }}>Помилка: {error}</p>}
        </div>
    );
}