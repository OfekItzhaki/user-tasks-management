import React from 'react';
import { encodeHtml } from '../utils/security';

interface SafeTextProps {
  text: string | null | undefined;
  className?: string;
  as?: 'span' | 'div' | 'p';
}

/**
 * Component that safely renders text with HTML encoding to prevent XSS
 */
export const SafeText: React.FC<SafeTextProps> = ({ 
  text, 
  className = '', 
  as: Component = 'span' 
}) => {
  const encodedText = encodeHtml(text);
  
  return (
    <Component 
      className={className}
      dangerouslySetInnerHTML={{ __html: encodedText }}
    />
  );
};
