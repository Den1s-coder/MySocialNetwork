import { Navigate } from 'react-router-dom';
import { useUserRole } from '../hooks/useUserRole';

export default function ProtectedRoute({ children, requiredRole = 'Admin' }) {
    const { role, loading } = useUserRole();

    if (loading) {
        return <div style={{ padding: '20px' }}>Завантаження…</div>;
    }

    if (!role || (requiredRole === 'Admin' && role !== 'Admin') || (requiredRole === 'Moderator' && role !== 'Admin' && role !== 'Moderator')) {
        return <Navigate to="/" replace />;
    }

    return children;
}