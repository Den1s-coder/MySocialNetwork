import React, { useEffect, useState, useCallback, useMemo } from "react";
import { useAuth } from "../hooks/useAuth";
import { authFetch } from "../hooks/authFetch";
import Avatar from "../components/Avatar";

const API_BASE = "https://localhost:7142";

export default function FrindshipList() {
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
                    return {
                        id: fr.id ?? fr.Id,
                        friendshipId: fr.id ?? fr.Id,
                        userId: fr.requesterId ?? fr.RequesterId,
                        name: requester?.name ?? requester?.userName ?? "Без имени",
                        email: requester?.email ?? "-",
                        avatar: requester?.profilePictureUrl ?? requester?.avatarUrl ?? null,
                        status: "pending",
                        requestedAt: fr.requestedAt ?? fr.RequestedAt,
                    };
                })
            );

            setPending(items);
        } catch (err) {
            setError(err.message || "Не удалось загрузить заявки");
        }
    }, [fetchUser, headers]);

    const fetchFriends = useCallback(async () => {
        setError(null);
        try {
            const res = await authFetch(`${API_BASE}/api/Friend/MyFriends`, { headers });
            if (!res.ok) throw new Error(`HTTP ${res.status}`);
            const data = await res.json();

            const items = await Promise.all(
                (Array.isArray(data) ? data : []).map(async (f) => {
                    const requesterId = f.requesterId ?? f.RequesterId;
                    const addresseeId = f.addresseeId ?? f.AddresseeId;
                    const otherId =
                        String(requesterId).toLowerCase() === String(currentUserId)?.toLowerCase()
                            ? addresseeId
                            : requesterId;

                    const user = await fetchUser(otherId);
                    return {
                        id: otherId,
                        friendshipId: f.id ?? f.Id,
                        userId: otherId,
                        name: user?.name ?? user?.userName ?? "Без имени",
                        email: user?.email ?? "-",
                        avatar: user?.profilePictureUrl ?? user?.avatarUrl ?? null,
                        status: "accepted",
                    };
                })
            );

            setFriends(items);
        } catch (err) {
            setError(err.message || "Не удалось загрузить друзей");
        }
    }, [fetchUser, headers, currentUserId]);

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
            if (!res.ok) throw new Error(`Не удалось принять заявку (${res.status})`);

            await fetchFriends();
        } catch (err) {
            setPending(prevPending);
            setError(err.message || "Ошибка при принятии заявки");
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
            if (!res.ok) throw new Error(`Не удалось отклонить заявку (${res.status})`);
        } catch (err) {
            setPending(prevPending);
            setError(err.message || "Ошибка при отклонении заявки");
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
            if (!res.ok) throw new Error(`Не удалось удалить друга (${res.status})`);
        } catch (err) {
            setFriends(prevFriends);
            setError(err.message || "Ошибка при удалении друга");
        } finally {
            setBusy(friendUserId, false);
        }
    };

    const combined = [...pending.map((p) => ({ ...p })), ...friends.map((f) => ({ ...f }))];

    const filtered = combined.filter((it) =>
        `${it.name ?? ""} ${it.email ?? ""}`.toLowerCase().includes(search.trim().toLowerCase())
    );

    return (
        <div style={{ padding: 20 }}>
            <h2>Друзья и заявки</h2>

            <div style={{ marginBottom: 12 }}>
                <input
                    placeholder="Поиск по имени или email"
                    value={search}
                    onChange={(e) => setSearch(e.target.value)}
                    style={{ padding: 8, width: 320 }}
                />
                <button onClick={fetchAll} style={{ marginLeft: 8, padding: "8px 12px" }}>
                    Обновить
                </button>
            </div>

            {loading && <div>Загрузка...</div>}

            {error && (
                <div style={{ color: "red", marginBottom: 12 }}>
                    Ошибка: {error}
                </div>
            )}

            {!loading && filtered.length === 0 && <div>Список пуст.</div>}

            <h3>Заявки</h3>
            <ul style={{ listStyle: "none", padding: 0 }}>
                {pending.map((it) => (
                    <li
                        key={it.friendshipId}
                        style={{
                            display: "flex",
                            alignItems: "center",
                            justifyContent: "space-between",
                            padding: "8px 12px",
                            borderBottom: "1px solid #eee",
                        }}
                    >
                        <div style={{ display: "flex", alignItems: "center", gap: 12 }}>
                            <Avatar url={it.avatar} name={it.name} size={48} />
                            <div>
                                <div style={{ fontWeight: 600 }}>{it.name}</div>
                                <div style={{ fontSize: 12, color: "#666" }}>{it.email}</div>
                                <div style={{ fontSize: 12, color: "#888" }}>Статус: {it.status}</div>
                            </div>
                        </div>

                        <div>
                            <button
                                onClick={() => onAccept(it.friendshipId)}
                                disabled={busyIds.has(String(it.friendshipId))}
                                style={{ marginRight: 8 }}
                            >
                                {busyIds.has(String(it.friendshipId)) ? "..." : "Принять"}
                            </button>
                            <button
                                onClick={() => onDecline(it.friendshipId)}
                                disabled={busyIds.has(String(it.friendshipId))}
                                style={{ marginRight: 8, background: "#fff" }}
                            >
                                {busyIds.has(String(it.friendshipId)) ? "..." : "Отклонить"}
                            </button>
                        </div>
                    </li>
                ))}
            </ul>

            <h3>Друзья</h3>
            <ul style={{ listStyle: "none", padding: 0 }}>
                {friends.map((it) => (
                    <li
                        key={it.userId}
                        style={{
                            display: "flex",
                            alignItems: "center",
                            justifyContent: "space-between",
                            padding: "8px 12px",
                            borderBottom: "1px solid #eee",
                        }}
                    >
                        <div style={{ display: "flex", alignItems: "center", gap: 12 }}>
                            <Avatar url={it.avatar} name={it.name} size={48} />
                            <div>
                                <div style={{ fontWeight: 600 }}>{it.name}</div>
                                <div style={{ fontSize: 12, color: "#666" }}>{it.email}</div>
                                <div style={{ fontSize: 12, color: "#888" }}>Статус: {it.status}</div>
                            </div>
                        </div>

                        <div>
                            <button
                                onClick={() => onRemove(it.userId)}
                                disabled={busyIds.has(String(it.userId))}
                                style={{ background: "#f5f5f5" }}
                            >
                                {busyIds.has(String(it.userId)) ? "..." : "Удалить"}
                            </button>
                        </div>
                    </li>
                ))}
            </ul>
        </div>
    );
}