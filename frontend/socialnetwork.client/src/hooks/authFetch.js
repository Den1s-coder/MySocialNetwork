const API_BASE = 'https://localhost:7142';

export async function authFetch(input, init = {}) {
    const url = (typeof input === 'string' && !input.startsWith('http')) ? (input.startsWith('/') ? API_BASE + input : API_BASE + '/' + input) : input;
    const accessToken = localStorage.getItem('accessToken');

    const headers = new Headers(init.headers || {});
    if (accessToken) headers.set('Authorization', `Bearer ${accessToken}`);
    if (!headers.has('Content-Type') && !(init.body instanceof FormData)) {
        headers.set('Content-Type', 'application/json');
    }

    let mergedInit = { ...init, headers };

    let response = await fetch(url, mergedInit);
    if (response.status !== 401) {
        return response;
    }

    const refreshToken = localStorage.getItem('refreshToken');
    if (!refreshToken) {
        localStorage.removeItem('accessToken');
        localStorage.removeItem('refreshToken');
        window.dispatchEvent(new Event('tokens-updated'));
        return response;
    }

    const refreshResponse = await fetch(API_BASE + '/api/Auth/refresh-token', {
        method: 'POST',
        headers: { 'Content-Type': 'application/json' },
        body: JSON.stringify(refreshToken)
    });

    if (!refreshResponse.ok) {
        localStorage.removeItem('accessToken');
        localStorage.removeItem('refreshToken');
        window.dispatchEvent(new Event('tokens-updated'));
        return response;
    }

    const refreshData = await refreshResponse.json();
    const newAccessToken = refreshData.accessToken;
    const newRefreshToken = refreshData.refreshToken;

    if (!newAccessToken || !newRefreshToken) {
        localStorage.removeItem('accessToken');
        localStorage.removeItem('refreshToken');
        window.dispatchEvent(new Event('tokens-updated'));
        return response;
    }

    localStorage.setItem('accessToken', newAccessToken);
    localStorage.setItem('refreshToken', newRefreshToken);
    window.dispatchEvent(new Event('tokens-updated'));

    headers.set('Authorization', `Bearer ${newAccessToken}`);
    mergedInit = { ...init, headers };

    const retryResponse = await fetch(url, mergedInit);
    return retryResponse;
}