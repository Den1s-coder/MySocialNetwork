import { useState, useRef, useEffect } from 'react';
import EmojiPicker from 'emoji-picker-react';
import { MdEmojiEmotions } from 'react-icons/md';
import './EmojiPickerButton.css';

export default function EmojiPickerButton({ onEmojiSelect }) {
    const [showPicker, setShowPicker] = useState(false);
    const pickerRef = useRef(null);

    useEffect(() => {
        const handleClickOutside = (event) => {
            if (pickerRef.current && !pickerRef.current.contains(event.target)) {
                setShowPicker(false);
            }
        };

        if (showPicker) {
            document.addEventListener('mousedown', handleClickOutside);
            return () => document.removeEventListener('mousedown', handleClickOutside);
        }
    }, [showPicker]);

    const handleEmojiClick = (emojiObject) => {
        onEmojiSelect(emojiObject.emoji);
        setShowPicker(false);
    };

    return (
        <div className="emoji-picker-container" ref={pickerRef}>
            <button
                type="button"
                onClick={() => setShowPicker(!showPicker)}
                className="emoji-picker-button"
                title="Додати емодзі"
                aria-label="Емодзі"
            >
                <MdEmojiEmotions size={20} />
            </button>
            {showPicker && (
                <div className="emoji-picker-wrapper">
                    <EmojiPicker
                        onEmojiClick={handleEmojiClick}
                        height={400}
                        width={320}
                    />
                </div>
            )}
        </div>
    );
}