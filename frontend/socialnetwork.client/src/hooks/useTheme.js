import { useContext } from 'react';
import { ThemeContext } from '../context/ThemeContext';

export function useTheme() {
    const context = useContext(ThemeContext);
    
    if (!context) {
        console.error('useTheme must be used within ThemeProvider');
        return {
            theme: 'light',
            toggleTheme: () => {},
            isLight: true,
            isDark: false
        };
    }
    
    return context;
}