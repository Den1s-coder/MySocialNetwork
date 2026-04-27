import { createContext, useState, useEffect } from 'react';

export const ThemeContext = createContext();

const LIGHT_THEME = 'light';
const DARK_THEME = 'dark';
const STORAGE_KEY = 'app-theme';

export function ThemeProvider({ children }) {
    const [theme, setTheme] = useState(() => {
        // Спочатку перевіряємо localStorage
        const savedTheme = localStorage.getItem(STORAGE_KEY);
        if (savedTheme) {
            return savedTheme;
        }
        
        // Потім перевіряємо системні налаштування
        if (window.matchMedia && window.matchMedia('(prefers-color-scheme: dark)').matches) {
            return DARK_THEME;
        }
        
        return LIGHT_THEME;
    });

    // Застосовуємо тему до DOM
    useEffect(() => {
        document.documentElement.setAttribute('data-theme', theme);
        localStorage.setItem(STORAGE_KEY, theme);
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