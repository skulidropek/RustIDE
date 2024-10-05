import React, { useState, useCallback, useEffect } from 'react';
import './Resizer.css';

interface ResizerProps {
  onResize: (newSize: number) => void;
  orientation: 'horizontal' | 'vertical';
}

const Resizer: React.FC<ResizerProps> = ({ onResize, orientation }) => {
  const [isResizing, setIsResizing] = useState(false);

  const startResizing = useCallback((e: React.MouseEvent) => {
    e.preventDefault();
    setIsResizing(true);
  }, []);

  const stopResizing = useCallback(() => {
    setIsResizing(false);
  }, []);

  const resize = useCallback(
    (e: MouseEvent) => {
      if (isResizing) {
        onResize(orientation === 'vertical' ? e.clientX : e.clientY);
      }
    },
    [isResizing, onResize, orientation]
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