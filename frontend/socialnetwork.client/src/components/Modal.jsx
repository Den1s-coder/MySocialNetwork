import './Modal.css';

export default function Modal({ isOpen, title, onClose, children, size = 'medium' }) {
    if (!isOpen) return null;

    return (
        <div className="modal-overlay" onClick={onClose}>
            <div className={`modal-content modal-${size}`} onClick={e => e.stopPropagation()}>
                <div className="modal-header">
                    <h2 style={{ margin: 0 }}>{title}</h2>
                    <button 
                        className="modal-close-btn"
                        onClick={onClose}
                        aria-label="Close modal"
                    >
                        ✕
                    </button>
                </div>
                <div className="modal-body">
                    {children}
                </div>
            </div>
        </div>
    );
}