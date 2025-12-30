import React, { useEffect, useState, useCallback } from "react";

export default function FrindshipList() {
  const [items, setItems] = useState([]);
  const [loading, setLoading] = useState(true);
  const [error, setError] = useState(null);
  const [search, setSearch] = useState("");

  const fetchList = useCallback(async () => {
    setLoading(true);
    setError(null);
    try {
      const res = await fetch("/api/friendships");
      if (!res.ok) throw new Error(`Ошибка: ${res.status}`);
      const data = await res.json();
      setItems(Array.isArray(data) ? data : []);
    } catch (err) {
      setError(err.message || "Не удалось загрузить данные");
    } finally {
      setLoading(false);
    }
  }, []);

  useEffect(() => {
    fetchList();
  }, [fetchList]);

  const onAccept = async (id) => {
    setItems((prev) => prev.map((it) => (it.id === id ? { ...it, status: "accepted" } : it)));
    try {
      const res = await fetch(`/api/friendships/${id}/accept`, { method: "POST" });
      if (!res.ok) throw new Error("Не удалось принять заявку");
    } catch (err) {
      setItems((prev) => prev.map((it) => (it.id === id ? { ...it, status: "pending" } : it)));
      setError(err.message);
    }
  };

  const onRemove = async (id) => {
    // Оптимистично удаляем элемент
    const previous = items;
    setItems((prev) => prev.filter((it) => it.id !== id));
    try {
      const res = await fetch(`/api/friendships/${id}`, { method: "DELETE" });
      if (!res.ok) throw new Error("Не удалось удалить дружбу");
    } catch (err) {
      // Откат
      setItems(previous);
      setError(err.message);
    }
  };

  const filtered = items.filter((it) =>
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
          style={{ padding: 8, width: 300 }}
        />
        <button onClick={fetchList} style={{ marginLeft: 8, padding: "8px 12px" }}>
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

      <ul style={{ listStyle: "none", padding: 0 }}>
        {filtered.map((it) => (
          <li
            key={it.id}
            style={{
              display: "flex",
              alignItems: "center",
              justifyContent: "space-between",
              padding: "8px 12px",
              borderBottom: "1px solid #eee",
            }}
          >
            <div>
              <div style={{ fontWeight: 600 }}>{it.name ?? "Без имени"}</div>
              <div style={{ fontSize: 12, color: "#666" }}>{it.email ?? "-"}</div>
              <div style={{ fontSize: 12, color: "#888" }}>Статус: {it.status ?? "unknown"}</div>
            </div>

            <div>
              {it.status === "pending" && (
                <button onClick={() => onAccept(it.id)} style={{ marginRight: 8 }}>
                  Принять
                </button>
              )}
              <button onClick={() => onRemove(it.id)} style={{ background: "#f5f5f5" }}>
                Удалить
              </button>
            </div>
          </li>
        ))}
      </ul>
    </div>
  );
}