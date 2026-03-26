import { useState, useEffect } from 'react';
import { authFetch } from './authFetch';

const API_BASE = 'https://localhost:7142';

export const useUserRole = () => {
    const [role, setRole] = useState(null);
    const [loading, setLoading] = useState(true);
    const [error, setError] = useState(null);

    useEffect(() => {
        const fetchUserRole = async () => {
            try {
                const accessToken = localStorage.getItem('accessToken');
                if (!accessToken) {
                    setRole(null);
                    setLoading(false);
                    return;
                }

                const res = await authFetch(`${API_BASE}/api/User/profile`);
                if (!res.ok) throw new Error(`HTTP ${res.status}`);
                
                const user = await res.json();
                setRole(user.role);
            } catch (err) {
                console.error('Error fetching user role:', err);
                setError(err.message);
                setRole(null);
            } finally {
                setLoading(false);
            }
        };

        fetchUserRole();
    }, []);

    return { role, loading, error, isAdmin: role === 'Admin', isModeratorOrAdmin: role === 'Admin' || role === 'Moderator' };
};