/* eslint-disable react-refresh/only-export-components */
import { createContext, useState, useEffect } from 'react';

export const ThemeContext = createContext(null);

const LIGHT_THEME = 'light';
const DARK_THEME = 'dark';
const STORAGE_KEY = 'app-theme';

export function ThemeProvider({ children }) {
    const [theme, setTheme] = useState(() => {
        if (typeof window === 'undefined') return LIGHT_THEME;
        try {
            return localStorage.getItem(STORAGE_KEY) || LIGHT_THEME;
        } catch {
            return LIGHT_THEME;
        }
    });

    useEffect(() => {
        try {
            document.documentElement.setAttribute('data-theme', theme);
            localStorage.setItem(STORAGE_KEY, theme);
        } catch (e) {
            console.error('Failed to save theme:', e);
        }
    }, [theme]);

    const toggleTheme = () => {
        setTheme(prevTheme => prevTheme === LIGHT_THEME ? DARK_THEME : LIGHT_THEME);
    };

    const value = {
        theme,
        toggleTheme,
        isLight: theme === LIGHT_THEME,
        isDark: theme === DARK_THEME
    };

    return (
        <ThemeContext.Provider value={value}>
            {children}
        </ThemeContext.Provider>
    );
}