import React, { useEffect, useState, useCallback, useMemo } from "react";
import { Link } from "react-router-dom";
import { useAuth } from "../hooks/useAuth";
import { authFetch } from "../hooks/authFetch";
import Avatar from "../components/Avatar";

const API_BASE = "https://localhost:7142";

export default function FriendshipList() {
    const { accessToken, isAuthenticated, currentUserId } = useAuth();

    const [pending, setPending] = useState([]);
    const [friends, setFriends] = useState([]);
    const [loading, setLoading] = useState(true);
    const [error, setError] = useState(null);
    const [search, setSearch] = useState("");
    const [busyIds, setBusyIds] = useState(new Set());

    const headers = useMemo(() => (accessToken ? { Authorization: `Bearer ${accessToken}` } : undefined), [accessToken]);

    const fetchUser = useCallback(
        async (userId) => {
            try {
                const res = await authFetch(`${API_BASE}/api/User/users/${userId}`, { headers });
                if (!res.ok) return null;
                return await res.json();
            } catch {
                return null;
            }
        },
        [headers]
    );

    const fetchPending = useCallback(async () => {
        setError(null);
        try {
            const res = await authFetch(`${API_BASE}/api/Friend/PendingRequests`, { headers });
            if (!res.ok) throw new Error(`HTTP ${res.status}`);
            const data = await res.json();

            const items = await Promise.all(
                (Array.isArray(data) ? data : []).map(async (fr) => {
                    const requester = await fetchUser(fr.requesterId ?? fr.RequesterId);
                    const displayName = requester?.name ?? "Без імені";
                    return {
                        id: fr.id ?? fr.Id,
                        friendshipId: fr.id ?? fr.Id,
                        userId: fr.requesterId ?? fr.RequesterId,
                        displayName: displayName,
                        email: requester?.email ?? "-",
                        avatar: requester?.profilePictureUrl ?? null,
                        status: "pending",
                        requestedAt: fr.requestedAt ?? fr.RequestedAt,
                    };
                })
            );

            setPending(items);
        } catch (err) {
            setError(err.message || "Не вдалося завантажити заявки");
        }
    }, [fetchUser, headers]);

    const fetchFriends = useCallback(async () => {
        setError(null);
        try {
            const res = await authFetch(`${API_BASE}/api/Friend/MyFriends`, { headers });
            if (!res.ok) throw new Error(`HTTP ${res.status}`);
            const data = await res.json();

            // API повертає UserDto об'єкти напрямо
            const items = (Array.isArray(data) ? data : []).map((user) => ({
                id: `friend-${user.id}`,
                friendshipId: user.id,
                userId: user.id,
                displayName: user.name ?? "Без імені",
                email: user.email ?? "-",
                avatar: user.profilePictureUrl ?? null,
                status: "accepted",
            }));

            setFriends(items);
        } catch (err) {
            setError(err.message || "Не вдалося завантажити друзів");
        }
    }, [headers]);

    const fetchAll = useCallback(async () => {
        setLoading(true);
        await Promise.all([fetchPending(), fetchFriends()]);
        setLoading(false);
    }, [fetchPending, fetchFriends]);

    useEffect(() => {
        if (!isAuthenticated) {
            setPending([]);
            setFriends([]);
            setLoading(false);
            return;
        }
        fetchAll();
    }, [isAuthenticated, fetchAll]);

    const setBusy = (id, value) => {
        setBusyIds((s) => {
            const next = new Set(s);
            if (value) next.add(String(id));
            else next.delete(String(id));
            return next;
        });
    };

    const onAccept = async (friendshipId) => {
        setError(null);
        setBusy(friendshipId, true);

        const prevPending = pending;
        setPending((p) => p.filter((it) => String(it.friendshipId) !== String(friendshipId)));

        try {
            const res = await authFetch(`${API_BASE}/api/Friend/AcceptFriendRequest/${friendshipId}`, {
                method: "POST",
                headers,
            });
            if (!res.ok) throw new Error(`Не вдалося прийняти заявку (${res.status})`);

            await fetchFriends();
        } catch (err) {
            setPending(prevPending);
            setError(err.message || "Помилка при прийнятті заявки");
        } finally {
            setBusy(friendshipId, false);
        }
    };

    const onDecline = async (friendshipId) => {
        setError(null);
        setBusy(friendshipId, true);

        const prevPending = pending;
        setPending((p) => p.filter((it) => String(it.friendshipId) !== String(friendshipId)));

        try {
            const res = await authFetch(`${API_BASE}/api/Friend/DeclineFriendRequest/${friendshipId}`, {
                method: "POST",
                headers,
            });
            if (!res.ok) throw new Error(`Не вдалося відхилити заявку (${res.status})`);
        } catch (err) {
            setPending(prevPending);
            setError(err.message || "Помилка при відхиленні заявки");
        } finally {
            setBusy(friendshipId, false);
        }
    };

    const onRemove = async (friendUserId) => {
        setError(null);
        setBusy(friendUserId, true);

        const prevFriends = friends;
        setFriends((f) => f.filter((it) => String(it.userId) !== String(friendUserId)));

        try {
            const res = await authFetch(`${API_BASE}/api/Friend/RemoveFriend/${friendUserId}`, {
                method: "DELETE",
                headers,
            });
            if (!res.ok) throw new Error(`Не вдалося видалити друга (${res.status})`);
        } catch (err) {
            setFriends(prevFriends);
            setError(err.message || "Помилка при видаленні друга");
        } finally {
            setBusy(friendUserId, false);
        }
    };

    const filtered = [...pending, ...friends].filter((it) =>
        `${it.displayName ?? ""} ${it.email ?? ""}`.toLowerCase().includes(search.trim().toLowerCase())
    );

    const renderFriendItem = (it, actions, isRequest = false) => (
        <li
            key={`${it.status}-${it.friendshipId}`}
            style={{
                display: "flex",
                alignItems: "center",
                justifyContent: "space-between",
                padding: "12px",
                borderBottom: "1px solid #eee",
                borderRadius: "8px",
                marginBottom: "8px",
                backgroundColor: "#fafafa",
                transition: "background-color 0.2s",
            }}
        >
            <Link
                to={`/user/${it.userId}`}
                style={{
                    display: "flex",
                    alignItems: "center",
                    gap: 12,
                    flex: 1,
                    textDecoration: "none",
                    color: "inherit",
                }}
            >
                <Avatar url={it.avatar} name={it.displayName} size={48} />
                <div>
                    <div style={{ fontWeight: 600 }}>{it.displayName}</div>
                    <div style={{ fontSize: 12, color: "#666" }}>{it.email}</div>
                    <div style={{ fontSize: 12, color: "#888" }}>
                        Статус: {isRequest ? "Заявка" : it.status}
                    </div>
                </div>
            </Link>
            {actions}
        </li>
    );

    return (
        <div style={{ padding: 20 }}>
            <h2>Друзі та заявки</h2>

            <div style={{ marginBottom: 12 }}>
                <input
                    placeholder="Пошук за ім'ям або email"
                    value={search}
                    onChange={(e) => setSearch(e.target.value)}
                    style={{
                        padding: 8,
                        width: 320,
                        borderRadius: "4px",
                        border: "1px solid #ddd",
                    }}
                />
                <button
                    onClick={fetchAll}
                    style={{
                        marginLeft: 8,
                        padding: "8px 12px",
                        cursor: "pointer",
                        borderRadius: "4px",
                        border: "1px solid #ddd",
                        backgroundColor: "#f5f5f5",
                    }}
                >
                    Оновити
                </button>
            </div>

            {loading && <div>Завантаження...</div>}

            {error && (
                <div
                    style={{
                        color: "red",
                        marginBottom: 12,
                        padding: "12px",
                        backgroundColor: "#ffebee",
                        borderRadius: "4px",
                    }}
                >
                    Помилка: {error}
                </div>
            )}

            {!loading && filtered.length === 0 && (
                <div style={{ color: "#999" }}>Список порожній.</div>
            )}

            <h3>Заявки ({pending.length})</h3>
            <ul style={{ listStyle: "none", padding: 0 }}>
                {pending.length === 0 ? (
                    <li style={{ padding: "12px", color: "#999" }}>Немає заявок</li>
                ) : (
                    pending.map((it) =>
                        renderFriendItem(
                            it,
                            (
                                <div style={{ display: "flex", gap: "8px" }}>
                                    <button
                                        onClick={() => onAccept(it.friendshipId)}
                                        disabled={busyIds.has(String(it.friendshipId))}
                                        style={{
                                            padding: "6px 12px",
                                            cursor: busyIds.has(String(it.friendshipId))
                                                ? "not-allowed"
                                                : "pointer",
                                            opacity: busyIds.has(String(it.friendshipId)) ? 0.6 : 1,
                                            borderRadius: "4px",
                                            border: "1px solid #ddd",
                                            backgroundColor: "#fff",
                                        }}
                                    >
                                        {busyIds.has(String(it.friendshipId)) ? "..." : "Прийняти"}
                                    </button>
                                    <button
                                        onClick={() => onDecline(it.friendshipId)}
                                        disabled={busyIds.has(String(it.friendshipId))}
                                        style={{
                                            padding: "6px 12px",
                                            cursor: busyIds.has(String(it.friendshipId))
                                                ? "not-allowed"
                                                : "pointer",
                                            opacity: busyIds.has(String(it.friendshipId)) ? 0.6 : 1,
                                            borderRadius: "4px",
                                            border: "1px solid #ddd",
                                            backgroundColor: "#f5f5f5",
                                        }}
                                    >
                                        {busyIds.has(String(it.friendshipId)) ? "..." : "Відхилити"}
                                    </button>
                                </div>
                            ),
                            true
                        )
                    )
                )}
            </ul>

            <h3>Друзі ({friends.length})</h3>
            <ul style={{ listStyle: "none", padding: 0 }}>
                {friends.length === 0 ? (
                    <li style={{ padding: "12px", color: "#999" }}>Немає друзів</li>
                ) : (
                    friends.map((it) =>
                        renderFriendItem(
                            it,
                            (
                                <div>
                                    <button
                                        onClick={() => onRemove(it.userId)}
                                        disabled={busyIds.has(String(it.userId))}
                                        style={{
                                            padding: "6px 12px",
                                            cursor: busyIds.has(String(it.userId))
                                                ? "not-allowed"
                                                : "pointer",
                                            opacity: busyIds.has(String(it.userId)) ? 0.6 : 1,
                                            borderRadius: "4px",
                                            border: "1px solid #ddd",
                                            backgroundColor: "#f5f5f5",
                                        }}
                                    >
                                        {busyIds.has(String(it.userId)) ? "..." : "Видалити"}
                                    </button>
                                </div>
                            ),
                            false
                        )
                    )
                )}
            </ul>
        </div>
    );
}