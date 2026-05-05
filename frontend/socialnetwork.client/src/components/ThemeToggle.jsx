import { useTheme } from '../hooks/useTheme';
import { FiMoon, FiSun } from 'react-icons/fi';
import './ThemeToggle.css';

export default function ThemeToggle() {
    const { theme, toggleTheme } = useTheme();

    return (
        <button
            className="theme-toggle"
            onClick={toggleTheme}
            title={theme === 'light' ? 'Переключити на темну тему' : 'Переключити на світлу тему'}
            aria-label="Toggle theme"
            type="button"
            style={{ background: 'transparent', boxShadow: 'none' }}
        >
            <span className="theme-icon">
                {theme === 'light' ? (
                    <FiMoon size={20} strokeWidth={2.5} />
                ) : (
                    <FiSun size={20} strokeWidth={2.5} />
                )}
            </span>
        </button>
    );
}