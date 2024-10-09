import React, { useState, useCallback, useEffect } from 'react';
import './Resizer.css';

interface ResizerProps {
  onResize: (delta: number) => void;
  orientation: 'horizontal' | 'vertical';
}

const Resizer: React.FC<ResizerProps> = ({ onResize, orientation }) => {
  const [isResizing, setIsResizing] = useState(false);
  const [startPosition, setStartPosition] = useState(0);

  const startResizing = useCallback((e: React.MouseEvent) => {
    e.preventDefault();
    setIsResizing(true);
    setStartPosition(orientation === 'vertical' ? e.clientX : e.clientY);
  }, [orientation]);

  const stopResizing = useCallback(() => {
    setIsResizing(false);
  }, []);

  const resize = useCallback(
    (e: MouseEvent) => {
      if (isResizing) {
        const currentPosition = orientation === 'vertical' ? e.clientX : e.clientY;
        const delta = currentPosition - startPosition;
        
        if (orientation === 'horizontal') {
          onResize(-delta);
        } else {
          onResize(delta);
        }
        
        setStartPosition(currentPosition);
      }
    },
    [isResizing, onResize, orientation, startPosition]
  );

  useEffect(() => {
    document.addEventListener('mousemove', resize);
    document.addEventListener('mouseup', stopResizing);

    return () => {
      document.removeEventListener('mousemove', resize);
      document.removeEventListener('mouseup', stopResizing);
    };
  }, [resize, stopResizing]);

  return (
    <div
      className={`resizer ${orientation}`}
      onMouseDown={startResizing}
    />
  );
};

export default Resizer;