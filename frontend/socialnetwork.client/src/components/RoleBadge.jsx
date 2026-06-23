import React from 'react';
import { FiShield, FiFlag } from 'react-icons/fi';
import { MdAdminPanelSettings } from 'react-icons/md';
import './RoleBadge.css';

export default function RoleBadge({ role }) {
  if (!role) return null;

  const normalizedRole = String(role).trim();

  const roleConfig = {
    Admin: {
      color: '#ff6b6b',
      backgroundColor: '#ffe0e0',
      label: 'Адмін',
      icon: <MdAdminPanelSettings size={14} />
    },
    Moderator: {
      color: '#ffa940',
      backgroundColor: '#ffe7ba',
      label: 'Модератор',
      icon: <FiFlag size={14} />
    },
    User: {
      color: '#52c41a',
      backgroundColor: '#f6ffed',
      label: 'Користувач',
      icon: <FiShield size={14} />
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
      icon: <FiShield size={14} />
    };
  }

  return (
    <span
      className="role-badge"
      style={{
        color: config.color,
        backgroundColor: config.backgroundColor,
        display: 'flex',
        alignItems: 'center',
        gap: '4px'
      }}
      title={`Роль: ${config.label}`}
    >
      {config.icon}
      {config.label}
    </span>
  );
}