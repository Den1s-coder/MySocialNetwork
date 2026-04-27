import { useState } from 'react';
import { authFetch } from '../hooks/authFetch';
import './ReactionBar.css';

const API_BASE = 'https://localhost:7142';

const REACTION_TYPES = [
    { id: '00000000-0000-0000-0000-000000000001', code: 'like',  symbol: '👍' },
    { id: '00000000-0000-0000-0000-000000000002', code: 'love',  symbol: '❤️' },
    { id: '00000000-0000-0000-0000-000000000003', code: 'laugh', symbol: '😂' },
    { id: '00000000-0000-0000-0000-000000000004', code: 'sad',   symbol: '😢' },
    { id: '00000000-0000-0000-0000-000000000005', code: 'angry', symbol: '😡' },
];

/**
 * @param {{ reactions: {code,symbol,count}[], currentUserReactionCode: string|null, entityId: string, entityType: 'Post'|'Comment'|'Message', authed: boolean, onReactionChanged: (updatedReactions, newCode) => void, currentUserId?: string, entityAuthorId?: string }} props
 */
export default function ReactionBar({ reactions = [], currentUserReactionCode, entityId, entityType = 'Post', authed, onReactionChanged, currentUserId, entityAuthorId }) {
    const [pickerOpen, setPickerOpen] = useState(false);
    const [sending, setSending] = useState(false);

    const isOwnContent = currentUserId && entityAuthorId && 
        String(currentUserId).toLowerCase() === String(entityAuthorId).toLowerCase();

    const toggleReaction = async (reactionTypeId, code) => {
        if (!authed || sending || isOwnContent) return;
        setSending(true);

        const isRemoving = currentUserReactionCode === code;
        const optimisticReactions = buildOptimistic(reactions, code, currentUserReactionCode);
        const optimisticCode = isRemoving ? null : code;
        onReactionChanged?.(optimisticReactions, optimisticCode);
        setPickerOpen(false);

        try {
            let endpoint;
            
            if (entityType === 'Comment') {
                endpoint = `${API_BASE}/api/Comment/${entityId}/react?ReactionTypeId=${reactionTypeId}`;
            } else if (entityType === 'Message') {
                endpoint = `${API_BASE}/api/Chat/${entityId}/react?ReactionTypeId=${reactionTypeId}`;
            } else {
                endpoint = `${API_BASE}/api/Post/${entityId}/react?ReactionTypeId=${reactionTypeId}`;
            }

            const res = await authFetch(endpoint, { method: 'POST' });
            if (!res.ok) throw new Error(`HTTP ${res.status}`);
        } catch (err) {
            console.error('Toggle reaction failed', err);
            onReactionChanged?.(reactions, currentUserReactionCode);
        } finally {
            setSending(false);
        }
    };

    return (
        <div className="reaction-bar">
            {reactions.filter(r => r.count > 0).map(r => (
                <button
                    key={r.code}
                    onClick={() => {
                        if (!authed || isOwnContent) return;
                        const rt = REACTION_TYPES.find(t => t.code === r.code);
                        if (rt) toggleReaction(rt.id, rt.code);
                    }}
                    disabled={!authed || sending || isOwnContent}
                    title={isOwnContent ? 'Не можна ставити реакції на свій контент' : r.code}
                    className={`reaction-button ${currentUserReactionCode === r.code ? 'reaction-button--active' : ''} ${isOwnContent ? 'reaction-button--disabled' : ''}`}
                >
                    <span className="reaction-symbol">{r.symbol}</span>
                    <span className="reaction-count">{r.count}</span>
                </button>
            ))}

            {authed && !isOwnContent && (
                <div className="reaction-picker-container">
                    <button
                        onClick={() => setPickerOpen(o => !o)}
                        disabled={sending}
                        className="reaction-add-btn"
                        title="Додати реакцію"
                    >
                        +
                    </button>

                    {pickerOpen && (
                        <div className="reaction-picker">
                            {REACTION_TYPES.map(rt => (
                                <button
                                    key={rt.id}
                                    onClick={() => toggleReaction(rt.id, rt.code)}
                                    disabled={sending}
                                    className={`reaction-picker-item ${currentUserReactionCode === rt.code ? 'reaction-picker-item--active' : ''}`}
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

function buildOptimistic(reactions, clickedCode, currentCode) {
    const rt = REACTION_TYPES.find(t => t.code === clickedCode);
    if (!rt) return reactions;

    let result = reactions.map(r => ({ ...r }));

    if (currentCode) {
        const prev = result.find(r => r.code === currentCode);
        if (prev) prev.count = Math.max(0, prev.count - 1);
    }

    if (currentCode === clickedCode) {
        return result.filter(r => r.count > 0);
    }

    const existing = result.find(r => r.code === clickedCode);
    if (existing) {
        existing.count += 1;
    } else {
        result.push({ code: rt.code, symbol: rt.symbol, count: 1 });
    }

    return result.filter(r => r.count > 0);
}