import { useState, useEffect } from 'react';

export function useAuth() {
    const [token, setToken] = useState(localStorage.getItem('token'));
    const [isAuthenticated, setIsAuthenticated] = useState(Boolean(token));

    useEffect(() => {
        const handleStorageChange = () => {
            const newToken = localStorage.getItem('token');
            setToken(newToken);
            setIsAuthenticated(Boolean(newToken));
        };

        window.addEventListener('storage', handleStorageChange);
        return () => window.removeEventListener('storage', handleStorageChange);
    }, []);

    const logout = () => {
        localStorage.removeItem('token');
        setToken(null);
        setIsAuthenticated(false);
    };

    return { token, isAuthenticated, logout };
}