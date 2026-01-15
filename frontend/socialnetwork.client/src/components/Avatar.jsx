import React, { useState } from 'react';

function getInitials(name) {
    if (!name) return '';
    const parts = name.trim().split(/\s+/);
    return ((parts[0]?.[0] ?? '') + (parts[1]?.[0] ?? '')).toUpperCase();
}

export default function Avatar({ url, name, size = 40, className, style }) {
    const [failed, setFailed] = useState(false);
    const showImage = url && !failed;

    const wrapperStyle = {
        width: size,
        height: size,
        borderRadius: '50%',
        background: '#ddd',
        display: 'inline-flex',
        alignItems: 'center',
        justifyContent: 'center',
        fontWeight: 600,
        color: '#333',
        flex: '0 0 auto',
        overflow: 'hidden',
        ...style
    };

    return (
        <div className={className} style={{ width: size, height: size, display: 'inline-block' }}>
            {showImage ? (
                <img
                    src={url}
                    alt={name}
                    style={{ width: size, height: size, borderRadius: '50%', objectFit: 'cover', display: 'block' }}
                    onError={() => setFailed(true)}
                />
            ) : (
                <div style={wrapperStyle}>{getInitials(name)}</div>
            )}
        </div>
    );
}