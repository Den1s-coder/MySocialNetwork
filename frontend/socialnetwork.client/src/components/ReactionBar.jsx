import { useState } from 'react';
import { authFetch } from '../hooks/authFetch';

const API_BASE = 'https://localhost:7142';

// Seed-дані з бекенду — порядок відповідає SortOrder
const REACTION_TYPES = [
    { id: '00000000-0000-0000-0000-000000000001', code: 'like',  symbol: '👍' },
    { id: '00000000-0000-0000-0000-000000000002', code: 'love',  symbol: '❤️' },
    { id: '00000000-0000-0000-0000-000000000003', code: 'laugh', symbol: '😂' },
    { id: '00000000-0000-0000-0000-000000000004', code: 'sad',   symbol: '😢' },
    { id: '00000000-0000-0000-0000-000000000005', code: 'angry', symbol: '😡' },
];

/**
 * @param {{ reactions: {code,symbol,count}[], currentUserReactionCode: string|null, entityId: string, entityType: 'Post'|'Comment', authed: boolean, onReactionChanged: (updatedReactions, newCode) => void }} props
 */
export default function ReactionBar({ reactions = [], currentUserReactionCode, entityId, entityType = 'Post', authed, onReactionChanged }) {
    const [pickerOpen, setPickerOpen] = useState(false);
    const [sending, setSending] = useState(false);

    const toggleReaction = async (reactionTypeId, code) => {
        if (!authed || sending) return;
        setSending(true);

        // Оптимістичне оновлення
        const isRemoving = currentUserReactionCode === code;
        const optimisticReactions = buildOptimistic(reactions, code, currentUserReactionCode);
        const optimisticCode = isRemoving ? null : code;
        onReactionChanged?.(optimisticReactions, optimisticCode);
        setPickerOpen(false);

        try {
            const endpoint = entityType === 'Comment'
                ? `${API_BASE}/api/Comment/${entityId}/react?ReactionTypeId=${reactionTypeId}`
                : `${API_BASE}/api/Post/${entityId}/react?ReactionTypeId=${reactionTypeId}`;

            const res = await authFetch(endpoint, { method: 'POST' });
            if (!res.ok) throw new Error(`HTTP ${res.status}`);
        } catch (err) {
            console.error('Toggle reaction failed', err);
            // Відкат
            onReactionChanged?.(reactions, currentUserReactionCode);
        } finally {
            setSending(false);
        }
    };

    return (
        <div style={{ display: 'flex', alignItems: 'center', gap: 6, flexWrap: 'wrap', marginTop: 8 }}>
            {/* Відображення існуючих реакцій */}
            {reactions.filter(r => r.count > 0).map(r => (
                <button
                    key={r.code}
                    onClick={() => {
                        if (!authed) return;
                        const rt = REACTION_TYPES.find(t => t.code === r.code);
                        if (rt) toggleReaction(rt.id, rt.code);
                    }}
                    disabled={!authed || sending}
                    style={{
                        display: 'inline-flex', alignItems: 'center', gap: 4,
                        padding: '4px 8px', border: '1px solid',
                        borderColor: currentUserReactionCode === r.code ? '#4a90d9' : '#ddd',
                        background: currentUserReactionCode === r.code ? '#e8f0fe' : '#f9f9f9',
                        borderRadius: 16, cursor: authed ? 'pointer' : 'default',
                        fontSize: 14, lineHeight: 1,
                    }}
                    title={r.code}
                >
                    <span>{r.symbol}</span>
                    <span style={{ fontSize: 12, color: '#555' }}>{r.count}</span>
                </button>
            ))}

            {/* Кнопка "+" для вибору реакції */}
            {authed && (
                <div style={{ position: 'relative' }}>
                    <button
                        onClick={() => setPickerOpen(o => !o)}
                        disabled={sending}
                        style={{
                            width: 32, height: 32, borderRadius: '50%',
                            border: '1px solid #ddd', background: '#f9f9f9',
                            cursor: 'pointer', fontSize: 16, lineHeight: 1,
                            display: 'flex', alignItems: 'center', justifyContent: 'center',
                        }}
                        title="Додати реакцію"
                    >
                        +
                    </button>

                    {pickerOpen && (
                        <div style={{
                            position: 'absolute', bottom: '110%', left: 0, zIndex: 10,
                            display: 'flex', gap: 4, padding: 6,
                            background: '#fff', border: '1px solid #ddd',
                            borderRadius: 8, boxShadow: '0 2px 8px rgba(0,0,0,0.15)',
                        }}>
                            {REACTION_TYPES.map(rt => (
                                <button
                                    key={rt.id}
                                    onClick={() => toggleReaction(rt.id, rt.code)}
                                    disabled={sending}
                                    style={{
                                        fontSize: 20, background: 'none', border: 'none',
                                        cursor: 'pointer', padding: 4, borderRadius: 4,
                                        outline: currentUserReactionCode === rt.code ? '2px solid #4a90d9' : 'none',
                                    }}
                                    title={rt.code}
                                >
                                    {rt.symbol}
                                </button>
                            ))}
                        </div>
                    )}
                </div>
            )}
        </div>
    );
}

/** Оптимістично перераховує масив реакцій */
function buildOptimistic(reactions, clickedCode, currentCode) {
    const rt = REACTION_TYPES.find(t => t.code === clickedCode);
    if (!rt) return reactions;

    let result = reactions.map(r => ({ ...r }));

    // Зняти попередню реакцію
    if (currentCode) {
        const prev = result.find(r => r.code === currentCode);
        if (prev) prev.count = Math.max(0, prev.count - 1);
    }

    // Якщо натиснули ту саму — просто зняли
    if (currentCode === clickedCode) {
        return result.filter(r => r.count > 0);
    }

    // Додати нову
    const existing = result.find(r => r.code === clickedCode);
    if (existing) {
        existing.count += 1;
    } else {
        result.push({ code: rt.code, symbol: rt.symbol, count: 1 });
    }

    return result.filter(r => r.count > 0);
}