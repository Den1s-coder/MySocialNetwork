import React from 'react';
import './RoleBadge.css';

export default function RoleBadge({ role }) {
  if (!role) return null;

  const normalizedRole = String(role).trim();

  const roleConfig = {
    Admin: {
      color: '#ff6b6b',
      backgroundColor: '#ffe0e0',
      label: 'Адмін',
      emoji: '🔴'
    },
    Moderator: {
      color: '#ffa940',
      backgroundColor: '#ffe7ba',
      label: 'Модератор',
      emoji: '🟠'
    },
    User: {
      color: '#52c41a',
      backgroundColor: '#f6ffed',
      label: 'Користувач',
      emoji: '🟢'
    }
  };

  let config = null;
  for (const [key, value] of Object.entries(roleConfig)) {
    if (key.toLowerCase() === normalizedRole.toLowerCase()) {
      config = value;
      break;
    }
  }

  if (!config) {
    config = {
      color: '#999',
      backgroundColor: '#f0f0f0',
      label: normalizedRole,
      emoji: '⚪'
    };
  }

  return (
    <span
      className="role-badge"
      style={{
        color: config.color,
        backgroundColor: config.backgroundColor
      }}
      title={`Роль: ${config.label}`}
    >
      {config.label}
    </span>
  );
}