import { useState, useEffect } from 'react';

const getUserNameFromToken = (token) => {
    try {
        const base64Url = token.split('.')[1];
        const base64 = base64Url.replace(/-/g, '+').replace(/_/g, '/');
        const jsonPayload = decodeURIComponent(atob(base64).split('').map(function (c) {
            return '%' + ('00' + c.charCodeAt(0).toString(16)).slice(-2);
        }).join(''));
        const payload = JSON.parse(jsonPayload);
        return payload['http://schemas.xmlsoap.org/ws/2005/05/identity/claims/name'];
    } catch (error) {
        console.error('Помилка декодування токена:', error);
        return null;
    }
};

const getUserIdFromToken = (token) => {
    try {
        const base64Url = token.split('.')[1];
        const base64 = base64Url.replace(/-/g, '+').replace(/_/g, '/');
        const jsonPayload = decodeURIComponent(atob(base64).split('').map(function (c) {
            return '%' + ('00' + c.charCodeAt(0).toString(16)).slice(-2);
        }).join(''));
        const payload = JSON.parse(jsonPayload);
        return payload['http://schemas.xmlsoap.org/ws/2005/05/identity/claims/sid'];
    } catch (error) {
        console.error('Помилка декодування токена:', error);
        return null;
    }
};

export function useAuth() {
    const [token, setToken] = useState(localStorage.getItem('token'));
    const [isAuthenticated, setIsAuthenticated] = useState(Boolean(token));
    const [currentUserId, setCurrentUserId] = useState(null);
    const [currentUserName, setCurrentUserName] = useState(null);

    useEffect(() => {
        const handleStorageChange = () => {
            const newToken = localStorage.getItem('token');
            setToken(newToken);
            setIsAuthenticated(Boolean(newToken));
        };

        window.addEventListener('storage', handleStorageChange);
        return () => window.removeEventListener('storage', handleStorageChange);
    }, []);

    useEffect(() => {
        if (token) {
            const userId = getUserIdFromToken(token);
            const userName = getUserNameFromToken(token);
            setCurrentUserId(userId);
            setCurrentUserName(userName);
        } else {
            setCurrentUserId(null);
            setCurrentUserName(null);
        }
    }, [token]);

    const logout = () => {
        localStorage.removeItem('token');
        setToken(null);
        setIsAuthenticated(false);
        setCurrentUserId(null);
        setCurrentUserName(null);
    };

    return {
        token,
        isAuthenticated,
        currentUserId,
        currentUserName,
        logout
    };
}